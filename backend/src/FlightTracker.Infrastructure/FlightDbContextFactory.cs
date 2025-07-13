using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace FlightTracker.Infrastructure
{
    public class FlightDbContextFactory : IDesignTimeDbContextFactory<FlightDbContext>
    {
        public FlightDbContext CreateDbContext(string[] args)
        {
            // Try to load configuration from appsettings.Development.json or environment
            var basePath = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? config["ConnectionStrings:DefaultConnection"]
                ?? "Host=localhost;Port=5432;Database=flighttracker;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<FlightDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new FlightDbContext(optionsBuilder.Options);
        }
    }
}
