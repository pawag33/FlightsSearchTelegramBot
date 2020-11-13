using FlightSerachAppEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSearchAppCore
{
    public class FlightApiSearchParams
    {
        public DestinationPlace DestinationPlace { get; set; }
        public DateTime OutboundDate { get; set; }
        public DateTime InboundDate { get; set; }

        public override string ToString()
        {
            return string.Format(@"DestinationPlace is {0},
                            OutboundDate is {1},
                            InboundDate is {2}  ",
                            DestinationPlace.ToString(),
                            OutboundDate.ToString("dd/MM/yyyy"),
                            InboundDate.ToString("dd/MM/yyyy"));
        }
    }

}
