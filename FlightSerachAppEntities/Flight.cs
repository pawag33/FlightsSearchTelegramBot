using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSerachAppEntities
{
    public class Flight
    {
        public decimal Price { get; set; }
        public DateTime OutboundDepartureDate { get; set; }
        public DateTime OutboundArrivalDate { get; set; }
        public DateTime InboundDepartureDate { get; set; }
        public DateTime InboundArrivalDate { get; set; }
        public DestinationPlace DestinationPlace { get; set; }
        public Uri linkToBookAgent { get; set; }
        public AgentType AgentType { get; set; }
        public string AgentName { get; set; }
        public DateTime RequestedOutboundDate { get; set; }
        public DateTime RequestedInboundDate { get; set; }
    }


}
