using Microsoft.EntityFrameworkCore;
using CoworkingApp.API.Models;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Data
{
    public class CoworkingContext : DbContext
    {
        public CoworkingContext(DbContextOptions<CoworkingContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Space> Spaces { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Pricing> Pricings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Calendar> Calendars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TPH Inheritance for User
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Member>("Member")
                .HasValue<Administrator>("Admin")
                .HasValue<Manager>("Manager");

            // Precision for Decimals (Money)
            var decimalProps = new[]
            {
                (typeof(Space), "PricePerHour"),
                (typeof(Reservation), "TotalPrice"),
                (typeof(Payment), "Amount"),
                (typeof(Invoice), "Subtotal"),
                (typeof(Invoice), "Tax"),
                (typeof(Invoice), "Total"),
                (typeof(InvoiceItem), "UnitPrice"),
                (typeof(InvoiceItem), "Amount"),
                (typeof(Pricing), "HourlyRate"),
                (typeof(Pricing), "DailyRate"),
                (typeof(Pricing), "WeeklyRate"),
                (typeof(Pricing), "MonthlyRate"),
                (typeof(Analytics), "TotalRevenue")
            };

            foreach (var (type, prop) in decimalProps)
            {
                modelBuilder.Entity(type).Property(prop).HasColumnType("decimal(18,2)");
            }

            // Relationships

            // Space <-> Amenity (Many-to-Many)
            modelBuilder.Entity<Space>()
                .HasMany(s => s.Amenities)
                .WithMany(a => a.Spaces)
                .UsingEntity(j => j.ToTable("SpaceAmenities"));

            // Reservation <-> Payment (One-to-One)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Payment)
                .WithOne(p => p.Reservation)
                .HasForeignKey<Payment>(p => p.ReservationId);

            // Payment <-> Invoice (One-to-One)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithOne(i => i.Payment)
                .HasForeignKey<Invoice>(i => i.PaymentId);
            
            // Manager <-> Location (One-to-One per diagram logic, though code allows many)
            modelBuilder.Entity<Manager>()
                .HasOne(m => m.ManagedLocation)
                .WithMany() // Assuming a Location can have multiple Managers, or adjust if 1:1
                .HasForeignKey("ManagedLocationId"); // Shadow property if not in model

            // Review Uniqueness (One review per member per space? Optional but good practice)
            // modelBuilder.Entity<Review>().HasIndex(r => new { r.MemberId, r.SpaceId }).IsUnique();
        }
    }
}