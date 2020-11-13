using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSearchAppCore
{
    public class FlightApiResponse
    {
        public string SessionKey { get; set; }
        public Query Query { get; set; }
        public string Status { get; set; }
        public List<Itinerary> Itineraries { get; set; }
        public List<Leg> Legs { get; set; }
        public List<Segment> Segments { get; set; }
        public List<Carrier> Carriers { get; set; }
        public List<Agent> Agents { get; set; }
        public List<Place> Places { get; set; }
        public List<Currency> Currencies { get; set; }

    }

    public class Query
    {
        public string Country { get; set; }
        public string Currency { get; set; }
        public string Locale { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }
        public string OriginPlace { get; set; }
        public string DestinationPlace { get; set; }
        public string OutboundDate { get; set; }
        public string InboundDate { get; set; }
        public string LocationSchema { get; set; }
        public string CabinClass { get; set; }
        public bool GroupPricing { get; set; }
    }

    public class PricingOption
    {
        public List<int> Agents { get; set; }
        public int QuoteAgeInMinutes { get; set; }
        public double Price { get; set; }
        public string DeeplinkUrl { get; set; }
    }

    public class BookingDetailsLink
    {
        public string Uri { get; set; }
        public string Body { get; set; }
        public string Method { get; set; }
    }

    public class Itinerary
    {
        public string OutboundLegId { get; set; }
        public string InboundLegId { get; set; }
        public List<PricingOption> PricingOptions { get; set; }
        public BookingDetailsLink BookingDetailsLink { get; set; }
    }

    public class FlightNumber
    {
        public string flightNumber { get; set; }
        public int CarrierId { get; set; }
    }

    public class Leg
    {
        public string Id { get; set; }
        public List<int> SegmentIds { get; set; }
        public int OriginStation { get; set; }
        public int DestinationStation { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public int Duration { get; set; }
        public string JourneyMode { get; set; }
        public List<object> Stops { get; set; }
        public List<int> Carriers { get; set; }
        public List<int> OperatingCarriers { get; set; }
        public string Directionality { get; set; }
        public List<FlightNumber> FlightNumbers { get; set; }
    }

    public class Segment
    {
        public int Id { get; set; }
        public int OriginStation { get; set; }
        public int DestinationStation { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public int Carrier { get; set; }
        public int OperatingCarrier { get; set; }
        public int Duration { get; set; }
        public string FlightNumber { get; set; }
        public string JourneyMode { get; set; }
        public string Directionality { get; set; }
    }

    public class Carrier
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string DisplayCode { get; set; }
    }

    public class Agent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public bool OptimisedForMobile { get; set; }
        public string BookingNumber { get; set; }
        public string Type { get; set; }
    }

    public class Place
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class Currency
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string ThousandsSeparator { get; set; }
        public string DecimalSeparator { get; set; }
        public bool SymbolOnLeft { get; set; }
        public bool SpaceBetweenAmountAndSymbol { get; set; }
        public int RoundingCoefficient { get; set; }
        public int DecimalDigits { get; set; }
    }
}
