using FlightTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightTracker.Infrastructure
{
    /// <summary>
    /// Entity Framework Core DbContext for Flight Tracker, configured for TimescaleDB/PostgreSQL.
    /// </summary>
    public class FlightDbContext : DbContext
    {
        public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options) { }

        public DbSet<Airport> Airports => Set<Airport>();
        public DbSet<Airline> Airlines => Set<Airline>();
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<FlightSegment> FlightSegments => Set<FlightSegment>();
        public DbSet<FlightQuery> FlightQueries => Set<FlightQuery>();
        public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("timescaledb");

            modelBuilder.Entity<Airport>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.Property(e => e.Code).HasColumnType("CHAR(3)");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Airline>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.Property(e => e.Code).HasColumnType("CHAR(2)");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.AirlineCode).IsRequired().HasMaxLength(3);
                entity.Property(e => e.AirlineName).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Origin)
                    .WithMany()
                    .HasForeignKey("OriginCode")
                    .IsRequired(false);
                entity.HasOne(e => e.Destination)
                    .WithMany()
                    .HasForeignKey("DestinationCode")
                    .IsRequired(false);
                
                // Configure Money as an owned type
                entity.OwnsOne(e => e.Price, price =>
                {
                    price.Property(p => p.Amount).HasColumnName("PriceAmount").IsRequired();
                    price.Property(p => p.Currency).HasColumnName("PriceCurrency").HasMaxLength(3).IsRequired();
                });
                
                // Configure the collection of segments
                entity.HasMany(e => e.Segments)
                    .WithOne()
                    .HasForeignKey("FlightId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FlightSegment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.AirlineCode).IsRequired().HasMaxLength(3);
                entity.Property(e => e.OriginCode).IsRequired().HasMaxLength(3);
                entity.Property(e => e.DestinationCode).IsRequired().HasMaxLength(3);
                entity.HasOne(e => e.Origin)
                    .WithMany()
                    .HasForeignKey(e => e.OriginCode);
                entity.HasOne(e => e.Destination)
                    .WithMany()
                    .HasForeignKey(e => e.DestinationCode);
                entity.HasOne(e => e.Airline)
                    .WithMany()
                    .HasForeignKey(e => e.AirlineCode);
            });

            modelBuilder.Entity<FlightQuery>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.UserId).HasMaxLength(256); // Optional user tracking
                entity.HasOne(e => e.Origin)
                    .WithMany()
                    .HasForeignKey(e => e.OriginCode);
                entity.HasOne(e => e.Destination)
                    .WithMany()
                    .HasForeignKey(e => e.DestinationCode);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.LastSearchedAt).HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<PriceSnapshot>(entity =>
            {
                entity.HasKey(e => new { e.QueryId, e.AirlineCode, e.Cabin, e.CollectedAt });
                entity.HasOne(e => e.FlightQuery)
                    .WithMany(q => q.PriceSnapshots)
                    .HasForeignKey(e => e.QueryId);
                entity.HasOne(e => e.Airline)
                    .WithMany()
                    .HasForeignKey(e => e.AirlineCode);
                // Configure Money as an owned type
                entity.OwnsOne(e => e.Price, price =>
                {
                    price.Property(p => p.Amount).HasColumnName("PriceAmount").IsRequired();
                    price.Property(p => p.Currency).HasColumnName("PriceCurrency").HasMaxLength(3).IsRequired();
                });
            });
        }
    }
}
