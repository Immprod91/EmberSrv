using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TestApp.Models
{
    public class History
    {
        public int Id { get; set; }
        public int BicId { get; set; }
        public String Start_date { get; set; }
        public int? Start_dep { get; set; }
        public String End_date { get; set; }
        public int? End_dep { get; set; }
    }

    public class HistoriesContext : DbContext
    {
        public HistoriesContext()
                : base("name=HistoriesContext")
        {
        }
        public DbSet<History> Histories { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<HistoriesContext>(null);
        }
    }
}