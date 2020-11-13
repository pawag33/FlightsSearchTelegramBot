using FlightSerachAppEntities;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlightSearchAppCore
{
    public class Core
    {
        private readonly log4net.ILog log;
        private readonly object locker = new object();

        public Core()
        {
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void GetFlights(FlightsPresetSearchParams flightsPresetSearchParams, long chatId, Action<IEnumerable<Flight>, DestinationPlace, long, bool> callback)
        {
            List<FlightSearchParams> flightSearchParams = new List<FlightSearchParams>();
            // missed logic of range search
            foreach (DestinationPlace dPlace in flightsPresetSearchParams.DestinationPlaces)
            {
                FlightSearchParams flightSearchParam = new FlightSearchParams();
                flightSearchParam.DestinationPlace = dPlace;
                flightSearchParam.InboundDate = flightsPresetSearchParams.EndDateOfRange;
                flightSearchParam.OutboundDate = flightsPresetSearchParams.StartDateOfRange;
                flightSearchParams.Add(flightSearchParam);
            }
            int maxRetryAttempts = 3;
            TimeSpan pauseBetweenFailures = TimeSpan.FromMinutes(5);
            int iteration = 0;
            Parallel.ForEach(flightSearchParams, async (flightSearchParam) =>
            {
                log.Info("start get data for " + flightSearchParams.ToString());
                var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);
                try
                {
                    await retryPolicy.ExecuteAsync(async () =>
                    {
                       
                        log.Info(string.Format("Getting data for search params {0} , try no [{1}]  ", flightSearchParams.ToString(), iteration));
                        IEnumerable<Flight> flights = GetFlights(flightSearchParam, flightsPresetSearchParams.FlightSerachFilter);
                        log.Info(string.Format("Data for search params {0} , received ", flightSearchParams.ToString()));
                        bool res;
                        lock (locker)
                        {
                            iteration++;
                            res = iteration == flightSearchParams.Count;
                        }
                        if (res)
                        {
                            callback(flights, flightSearchParam.DestinationPlace, chatId, true);
                        }
                        else
                        {
                            callback(flights, flightSearchParam.DestinationPlace, chatId, false);
                        }
                    });
                }
                catch (Exception e)
                {
                    log.Error(string.Format("Getting data for search params {0} , failed ", flightSearchParams.ToString()));
                }
            });
        }


        public IEnumerable<Flight> GetFlights(FlightSearchParams flightSearchParam, FlightSerachFilter flightSerachFilter)
        {
            IEnumerable<Flight> flightsResponse = null;
            try
            {
                FlightApiSearchParams flightApiSearchParam = new FlightApiSearchParams();
                flightApiSearchParam.DestinationPlace = flightSearchParam.DestinationPlace;
                flightApiSearchParam.InboundDate = flightSearchParam.InboundDate;
                flightApiSearchParam.OutboundDate = flightSearchParam.OutboundDate;
                FlightsRetriever flightsRetriever = new FlightsRetriever();
                List<Flight> flights = flightsRetriever.GetFlightsBySearchParams(flightSearchParam.DestinationPlace, flightSearchParam.InboundDate, flightSearchParam.OutboundDate).ToList();
                // check if flights exist in db
                if (flights.Any() == false)
                {
                    log.Info(string.Format("Flights with params : {0},{1},{2} not exist in db ", flightSearchParam.DestinationPlace, flightSearchParam.OutboundDate.ToString("dd/MM/yyyy"), flightSearchParam.InboundDate.ToString("dd/MM/yyyy")));
                    string sessionId = this.CreateSession(flightApiSearchParam);
                    FlightApiResponse flightApiResponse = this.PullSessionResult(sessionId);
                    flights = this.ConvertFlightApiResponseToFlight(flightApiResponse, flightSearchParam.DestinationPlace, flightSearchParam.InboundDate, flightSearchParam.OutboundDate).ToList();
                    // store flight in db 
                    flightsRetriever.InsertFlights(flights);
                }
                else
                {
                    log.Info(string.Format("Flights with params : {0},{1},{2} found in db ", flightSearchParam.DestinationPlace, flightSearchParam.OutboundDate.ToString("dd/MM/yyyy"), flightSearchParam.InboundDate.ToString("dd/MM/yyyy")));
                }
                FlightFilterProcessor flightFilterProcessor = new FlightFilterProcessor();
                flightsResponse = flightFilterProcessor.FilterFlights(flights, flightSerachFilter);
            }
            catch (Exception e)
            {
                log.Info(string.Format("GetFlights error with params : {0},{1},{2}   ", flightSearchParam.DestinationPlace, flightSearchParam.InboundDate.ToString("dd/MM/yyyy"), flightSearchParam.OutboundDate.ToString("dd/MM/yyyy")));
                log.Error(string.Format("Error is : {0} stack trace is : {1} ", e.Message, e.StackTrace));
            }
            return flightsResponse;
        }

        private IEnumerable<Flight> ConvertFlightApiResponseToFlight(FlightApiResponse flightApiResponse, DestinationPlace destinationPlace, DateTime requestedInboundDate, DateTime requestedOutboundDate)
        {
            List<Flight> flights = new List<Flight>();
            foreach (var itinerary in flightApiResponse.Itineraries)
            {
                List<Flight> subflights = new List<Flight>(new Flight[itinerary.PricingOptions.Count]);
                for (int i = 0; i < itinerary.PricingOptions.Count; i++)
                {
                    subflights[i] = new Flight();
                    subflights[i].Price = Convert.ToDecimal(itinerary.PricingOptions[i].Price);
                    int agentId = itinerary.PricingOptions[i].Agents.First();
                    Agent agent = flightApiResponse.Agents.Single(a => a.Id == agentId);
                    subflights[i].AgentName = agent.Name;
                    subflights[i].AgentType = agent.Type.ToLower() == "Airline" ? AgentType.AIRLINEE : AgentType.TRAVEL_AGENT;
                    subflights[i].linkToBookAgent = new Uri(itinerary.PricingOptions[i].DeeplinkUrl);
                    subflights[i].DestinationPlace = destinationPlace;
                    subflights[i].RequestedInboundDate = requestedInboundDate;
                    subflights[i].RequestedOutboundDate = requestedOutboundDate;
                }
                Leg outboundLeg = flightApiResponse.Legs.Single(l => l.Id == itinerary.OutboundLegId);
                Leg inboundLeg = flightApiResponse.Legs.Single(l => l.Id == itinerary.InboundLegId);

                foreach (Flight flight in subflights)
                {
                    flight.InboundDepartureDate = inboundLeg.Departure;
                    flight.InboundArrivalDate = inboundLeg.Arrival;
                    flight.OutboundArrivalDate = outboundLeg.Arrival;
                    flight.OutboundDepartureDate = outboundLeg.Departure;
                }
                flights.AddRange(subflights);
            }
            return flights;
        }



        private FlightApiResponse PullSessionResult(string sessionId)
        {
            log.Info(" PullSessionResult with sessionId : " + sessionId);
            FlightApiResponse flightApiResponse;
            int stops = 0;
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(string.Format("https://skyscanner-skyscanner-flight-search-v1.p.mashape.com/apiservices/pricing/uk2/v1.0/{0}?stops={1}&sortType=price&sortOrder=asc&pageIndex=0&pageSize=10", sessionId, stops.ToString())),
                    Method = HttpMethod.Get
                })
                {
                    request.Headers.Add("X-Mashape-Key", "w5UPMnTuLkmshCqyal7H1pJ2Vkufp17T5LMjsnMUyFZtroAC6a");
                    request.Headers.Add("X-Mashape-Host", "skyscanner-skyscanner-flight-search-v1.p.mashape.com");

                    HttpResponseMessage result = client.SendAsync(request).Result;
                    if (result.IsSuccessStatusCode == false)
                    {
                        string jsonErr = result.Content.ReadAsStringAsync().Result;
                        log.Info("Error on pull session result " + jsonErr);
                        throw new Exception(jsonErr);
                    }
                    log.Info("Pull Session Result was complete with http status code  " + result.StatusCode);


                    string jsonData = result.Content.ReadAsStringAsync().Result;
                    flightApiResponse = JsonConvert.DeserializeObject<FlightApiResponse>(jsonData);
                    log.Info(string.Format("Pull Session Result was complete , api response  contains [{0}] itineraries ", flightApiResponse.Itineraries.Count));
                }
            }
            return flightApiResponse;
        }

        private string CreateSession(FlightApiSearchParams flightApiSearchParam)
        {
            log.Info(" CreateSession with flightApiSearchParam : " + flightApiSearchParam.ToString());
            string sessionId = null;
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://skyscanner-skyscanner-flight-search-v1.p.mashape.com/apiservices/pricing/v1.0"),
                    Method = HttpMethod.Post
                })
                {
                    Dictionary<string, string> requestParams = new Dictionary<string, string>();
                    requestParams.Add("Country", "IL-sky");
                    requestParams.Add("Currency", "USD");
                    requestParams.Add("Locale", "enl-US");
                    requestParams.Add("OriginPlace", "TLV-sky");
                    requestParams.Add("DestinationPlace", this.GetDestinationPlaceApiCode(flightApiSearchParam.DestinationPlace));
                    requestParams.Add("OutboundDate", flightApiSearchParam.OutboundDate.ToString("yyyy-MM-dd"));
                    requestParams.Add("InboundDate", flightApiSearchParam.InboundDate != null ? flightApiSearchParam.InboundDate.ToString("yyyy-MM-dd") : "");
                    requestParams.Add("CabinClass", "economy");
                    requestParams.Add("Ddults", "1");
                    requestParams.Add("Children", "0");
                    requestParams.Add("Infants", "0");
                    requestParams.Add("IncludeCarriers", "");
                    requestParams.Add("ExcludeCarriers", "");

                    request.Content = new FormUrlEncodedContent(requestParams);

                    request.Content.Headers.ContentType =
                      new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    request.Headers.Add("X-Mashape-Key", "Your RapidApi KEY !!!");
                    request.Headers.Add("X-Mashape-Host", "skyscanner-skyscanner-flight-search-v1.p.mashape.com");

                    HttpResponseMessage result = client.SendAsync(request).Result;
                    if (result.IsSuccessStatusCode == false)
                    {
                        string jsonErr = result.Content.ReadAsStringAsync().Result;
                        log.Info("Error on getting session id " + jsonErr);
                        throw new Exception(jsonErr);
                    }
                    else
                    {
                        log.Info("Session id was got successfully");
                    }

                    sessionId = result.Headers.Location.ToString().Split('/').Last();
                }
            }
            return sessionId;
        }

        private string GetDestinationPlaceApiCode(DestinationPlace destinationPlace)
        {
            switch (destinationPlace)
            {
                case DestinationPlace.UKRAINE_KIEV:
                    return "KIEV-sky";
                case DestinationPlace.UKRAINE_ODESA:
                    return "ODS-sky";
                case DestinationPlace.UKRAINE_LVIV:
                    return "LWO-sky";
                default:
                    throw new Exception("Unknown Destination Place");
            }
        }
    }
}
