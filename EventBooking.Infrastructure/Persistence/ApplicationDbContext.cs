using EventBooking.Application.Abstractions;
using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace EventBooking.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Event> Events => Set<Event>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.City).HasMaxLength(100).IsRequired();
                entity.Property(l => l.PostalCode).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(50);
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Location)
                    .WithMany()
                    .HasForeignKey(e => e.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.StatusCode).HasMaxLength(20);
                entity.Property(r => r.SeatNumbers)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null)!)
                    .Metadata.SetValueComparer(new ValueComparer<List<int>>(
                        (a, b) => a != null && b != null && a.SequenceEqual(b),
                        v => v.Aggregate(0, (hash, x) => HashCode.Combine(hash, x.GetHashCode())),
                        v => v.ToList()));

                entity.HasOne(r => r.Event)
                    .WithMany()
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReservationStatus)
                    .WithMany()
                    .HasForeignKey(r => r.StatusCode)
                    .HasPrincipalKey(s => s.Code)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).HasMaxLength(256);
                entity.Property(u => u.Role).HasMaxLength(20);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Koncert" },
                new Category { Id = 2, Name = "Konferencija" },
                new Category { Id = 3, Name = "Sport" },
                new Category { Id = 4, Name = "Izložba" },
                new Category { Id = 5, Name = "Festival" },
                new Category { Id = 6, Name = "Radionica" }
            );

            modelBuilder.Entity<ReservationStatus>().HasData(
                new ReservationStatus { Id = 1, Code = "PENDING",    Name = "Pending" },
                new ReservationStatus { Id = 2, Code = "CONFIRMED",  Name = "Confirmed" },
                new ReservationStatus { Id = 3, Code = "CANCELLED",  Name = "Cancelled" },
                new ReservationStatus { Id = 4, Code = "WAITLIST",   Name = "Waitlist" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
