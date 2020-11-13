using FlightSerachAppEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSearchAppRepository
{
    public class FlightRepository : IDisposable
    {
        private FlightSearchAppDBContext context;

        public FlightRepository()
        {
            context = new FlightSearchAppDBContext();
        }

        //Get Flight By Id
        public FlightDBEntity GetFlightById(Guid id)
        {
            return context.Flights.Find(id);
        }

        //Get All Flights
        public IEnumerable<FlightDBEntity> GetAllFlights()
        {
            return context.Flights.ToList();
        }

        //Get All Flights
        public IEnumerable<FlightDBEntity> GetFlightsBySearchParams(DestinationPlace destinationPlace, DateTime requestedInboundDate, DateTime requestedOutboundDate)
        {
            return context.Flights.Where
                (f=> f.DestinationPlace == destinationPlace &&
                f.RequestedInboundDate == requestedInboundDate &&
                f.RequestedOutboundDate == requestedOutboundDate)
                .ToList();
        }

        //Get All Available Cars for users only
        //public IEnumerable<Car> GetAllAvailableCars()
        //{
        //    string Available = "available";
        //    return cars.Where(c => c.Availability == Available).ToList();
        //}

        //Get Cars By Model
        //public IEnumerable<Car> GetCarsByModel(string model)
        //{
        //    return cars.Where(c => c.Model == model);
        //}

        //Get Car By Number
        //public Car GetCarByNumber(string Number)
        //{
        //    return cars.FirstOrDefault(c => c.CarNumber == Number);
        //}

        //Add flight to database
        public void Insert(FlightDBEntity flight)
        {
            context.Flights.Add(flight);

            context.SaveChanges();
        }

        public void Insert(IEnumerable<FlightDBEntity> flights)
        {
            context.Flights.AddRange(flights);

            context.SaveChanges();
        }

        //Update existed car
        //public void Update(string carNum, Car carForUpdate)
        //{
        //    Car car = context.Cars.FirstOrDefault(c => c.CarNumber == carNum);
        //    if (car != null)
        //    {
        //        car.Photo = carForUpdate.Photo;
        //        car.Model = carForUpdate.Model;
        //        car.PricePerDay = carForUpdate.PricePerDay;
        //        car.CostOfDayOverdue = carForUpdate.CostOfDayOverdue;
        //        car.Availability = carForUpdate.Availability;
        //        car.Year = carForUpdate.Year;
        //        car.Mileage = carForUpdate.Mileage;
        //        car.CarNumber = carForUpdate.CarNumber;
        //        car.Branch = carForUpdate.Branch;
        //        context.SaveChanges();
        //    }
        //}

        //// Rent Car (for users)
        //public void RentCar(string carNum)
        //{
        //    Car car = context.Cars.FirstOrDefault(c => c.CarNumber == carNum);
        //    if (car != null)
        //    {

        //        car.Availability = "Not Available";

        //        context.SaveChanges();
        //    }

        //}

        //// Delete car
        //public void Delete(string num)
        //{
        //    Car car = context.Cars.FirstOrDefault(c => c.CarNumber == num);
        //    context.Cars.Remove(car);
        //    context.SaveChanges();
        //}

        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
            }
        }
    }
}
