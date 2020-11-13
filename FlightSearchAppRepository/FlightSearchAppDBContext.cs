using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSearchAppRepository
{
    public class FlightSearchAppDBContext : DbContext
    {
        public DbSet<FlightDBEntity> Flights { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    Database.SetInitializer<FlightSearchAppDBContext>(null);
        //    base.OnModelCreating(modelBuilder);
        //}
    }
   
}
