using FlightSearchAppRepository;
using FlightSerachAppEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSearchAppCore
{
    public class FlightsRetriever
    {
        private readonly log4net.ILog log;

        public FlightsRetriever()
        {
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public IEnumerable<Flight> GetFlightsBySearchParams(DestinationPlace destinationPlace,DateTime requestedInboundDate, DateTime requestedOutboundDate)
        {
            IEnumerable<FlightDBEntity> flightDBEntities;
            List<Flight> flights = new List<Flight>();
            using (FlightRepository flightRepository = new FlightRepository())
            {
                flightDBEntities = flightRepository.GetFlightsBySearchParams(destinationPlace, requestedInboundDate, requestedOutboundDate);
            }
            foreach (FlightDBEntity flightDBEntity in flightDBEntities)
            {
                flights.Add(MapFlightDBEntityToFlight(flightDBEntity));
            }
            return flights;
        }

        public void InsertFlights(IEnumerable<Flight> flights)
        {
            List<FlightDBEntity> flightDBEntities = new List<FlightDBEntity>();
            foreach (Flight flight in flights)
            {
                FlightDBEntity flightDBEntity = MapFlightToFlightDBEntity(flight);
                flightDBEntity.InsertDate = DateTime.Now;
                flightDBEntities.Add(flightDBEntity);
            }
            using (FlightRepository flightRepository = new FlightRepository())
            {
                flightRepository.Insert(flightDBEntities);
            }
        }

        private Flight MapFlightDBEntityToFlight(FlightDBEntity flightDBEntity)
        {
            Flight flight = new Flight();
            flight.AgentName = flightDBEntity.AgentName;
            flight.AgentType = flightDBEntity.AgentType;
            flight.linkToBookAgent = new Uri(flightDBEntity.linkToBookAgent);
            flight.DestinationPlace = flightDBEntity.DestinationPlace;
            flight.InboundArrivalDate = flightDBEntity.InboundArrivalDate;
            flight.InboundDepartureDate = flightDBEntity.InboundDepartureDate;
            flight.OutboundArrivalDate = flightDBEntity.OutboundArrivalDate;
            flight.OutboundDepartureDate = flightDBEntity.OutboundDepartureDate;
            flight.Price = flightDBEntity.Price;
            flight.RequestedInboundDate = flightDBEntity.RequestedInboundDate;
            flight.RequestedOutboundDate = flightDBEntity.RequestedOutboundDate;
            return flight;
        }

        private FlightDBEntity MapFlightToFlightDBEntity(Flight flight)
        {
            FlightDBEntity flightDBEntity = new FlightDBEntity();
            flightDBEntity.AgentName = flight.AgentName;
            flightDBEntity.AgentType = flight.AgentType;
            flightDBEntity.linkToBookAgent = flight.linkToBookAgent.ToString();
            flightDBEntity.DestinationPlace = flight.DestinationPlace;
            flightDBEntity.InboundArrivalDate = flight.InboundArrivalDate;
            flightDBEntity.InboundDepartureDate = flight.InboundDepartureDate;
            flightDBEntity.OutboundArrivalDate = flight.OutboundArrivalDate;
            flightDBEntity.OutboundDepartureDate = flight.OutboundDepartureDate;
            flightDBEntity.Price = flight.Price;
            flightDBEntity.RequestedInboundDate = flight.RequestedInboundDate;
            flightDBEntity.RequestedOutboundDate = flight.RequestedOutboundDate;
            return flightDBEntity;
        }
    }

}
