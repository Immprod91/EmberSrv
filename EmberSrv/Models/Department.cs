using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TestApp.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class DepartmentsContext : DbContext
    {
        public DepartmentsContext()
                : base("name=DepartmentsContext")
        {
        }
        public DbSet<Department> Departments { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<DepartmentsContext>(null);
        }
    }
}