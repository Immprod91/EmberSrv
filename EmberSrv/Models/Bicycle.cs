using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TestApp.Models
{
    public class Bicycle
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Price { get; set; }
        public int? DepId { get; set; }
        public bool Status { get; set; }
        public string RentTime { get; set; }
    }

    public class BicyclesContext : DbContext
    {
        public BicyclesContext()
                : base("name=BicyclesContext")
        {
        }
        public DbSet<Bicycle> Bicycles { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<BicyclesContext>(null);
        }
    }
}