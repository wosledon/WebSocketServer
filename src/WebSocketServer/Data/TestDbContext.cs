using Microsoft.EntityFrameworkCore;
using System;
using WebSocketServer.Entities;

namespace WebSocketServer.Data
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<RunClock> RunClocks { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasData(
                    new Company()
                    {
                        Id = Guid.Parse("bbdee09c-089b-4d30-bece-44df5923716c"),
                        Name = "Microsoft",
                        Introduction = "Great Company"
                    },
                    new Company()
                    {
                        Id = Guid.Parse("6fb600c1-9011-4fd7-9234-881379716440"),
                        Name = "Google",
                        Introduction = "Don't be evil"
                    });

            modelBuilder.Entity<Employee>()
                .HasData(
                    new Employee()
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = Guid.Parse("bbdee09c-089b-4d30-bece-44df5923716c"),
                        Name = "A",
                        Age = 15
                    },
                    new Employee()
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = Guid.Parse("bbdee09c-089b-4d30-bece-44df5923716c"),
                        Name = "A",
                        Age = 18
                    },
                    new Employee()
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = Guid.Parse("bbdee09c-089b-4d30-bece-44df5923716c"),
                        Name = "A",
                        Age = 20
                    },
                    new Employee()
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = Guid.Parse("6fb600c1-9011-4fd7-9234-881379716440"),
                        Name = "A",
                        Age = 99
                    },
                    new Employee()
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = Guid.Parse("6fb600c1-9011-4fd7-9234-881379716440"),
                        Name = "A",
                        Age = 59
                    });
        }
    }
}