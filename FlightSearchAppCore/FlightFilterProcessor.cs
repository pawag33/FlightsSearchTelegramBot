using FlightSerachAppEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSearchAppCore
{
    public class FlightFilterProcessor
    {
        public IEnumerable<Flight> FilterFlights(IEnumerable<Flight> flight, FlightSerachFilter FlightSerachFilter)
        {
            return flight.OrderBy(f=> f.Price).Take(3);
        }

        private IEnumerable<Flight> DayFilter(IEnumerable<Flight> flight, bool DayFlight)
        {
            throw new  NotImplementedException();
        }

        private IEnumerable<Flight> AgentFilter(IEnumerable<Flight> flight, AgentType agentType)
        {
            throw new NotImplementedException();
        }
    }
}
