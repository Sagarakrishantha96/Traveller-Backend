using Microsoft.EntityFrameworkCore;

namespace Traveller_backend.EntityModels
{
    public class TravelDbContext : DbContext
    {
        public TravelDbContext(DbContextOptions<TravelDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Place> Places => Set<Place>();
        public DbSet<Attraction> Attractions => Set<Attraction>();
        public DbSet<Stay> Stays => Set<Stay>();
        public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();
        public DbSet<RouteStop> RouteStops => Set<RouteStop>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<TripDay> TripDays => Set<TripDay>();
        public DbSet<TripItem> TripItems => Set<TripItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(150);
                entity.Property(u => u.PasswordHash).HasMaxLength(255);
                entity.Property(u => u.Phone).HasMaxLength(20);
            });

            // route_stops.route_id -> transport_routes.id (name doesn't follow EF's naming convention)
            modelBuilder.Entity<RouteStop>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Stops)
                .HasForeignKey(s => s.RouteId);

            // activities.day_id -> itinerary_days.id
            modelBuilder.Entity<TripItem>()
                .HasOne(i => i.Day)
                .WithMany(d => d.Items)
                .HasForeignKey(i => i.DayId);
        }
    }
}
