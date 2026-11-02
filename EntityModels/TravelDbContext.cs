using Microsoft.EntityFrameworkCore;

namespace Traveller_backend.EntityModels
{
    public class TravelDbContext : DbContext
    {
        public TravelDbContext(DbContextOptions<TravelDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

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
        }
    }
}
