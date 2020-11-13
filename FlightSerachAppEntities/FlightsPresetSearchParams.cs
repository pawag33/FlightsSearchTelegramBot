using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSerachAppEntities
{
    public class FlightsPresetSearchParams
    {
        public FlightSerachFilter FlightSerachFilter { get; set; }
        public bool DatesRangeSearch { get; set; }
        public int MinimumDaysTrip { get; set; }
        public IEnumerable<DestinationPlace> DestinationPlaces { get; set; }
        public DateTime StartDateOfRange { get; set; }
        public DateTime EndDateOfRange { get; set; }
    }
}
