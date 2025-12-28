using System;
using System.Linq;
using BCrypt.Net;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(CoworkingContext context)
        {
            context.Database.EnsureCreated();

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            // 1. Create Super Admin
            var admin = new Administrator
            {
                UserId = Guid.NewGuid(),
                Email = "admin@coworking.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "System",
                LastName = "Admin",
                PhoneNumber = "1234567890",
                Status = UserStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow,
                AdminLevel = AdminLevel.SUPER_ADMIN
            };

            context.Administrators.Add(admin);

            // 2. Create a Sample Member
            var member = new Member
            {
                UserId = Guid.NewGuid(),
                Email = "member@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Member123!"),
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "0987654321",
                Status = UserStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow,
                MembershipType = MembershipType.BASIC,
                MembershipStartDate = DateTime.UtcNow,
                TotalBookings = 0
            };

            context.Members.Add(member);
            
            // 3. Create Location and Amenities
            if (!context.Locations.Any())
            {
                var location = new Location
                {
                    LocationId = Guid.NewGuid(),
                    Name = "Downtown Hub",
                    Address = "123 Main St",
                    City = "Tech City",
                    Country = "Innovation Land",
                    PostalCode = "90210",
                    OpeningHours = "08:00 - 22:00",
                    Coordinates = "40.7128° N, 74.0060° W", // <--- ADDED THIS
                    Facilities = "Parking, Gym, Lounge"     // Added just in case
                };
                context.Locations.Add(location);

                var wifi = new Amenity 
                { 
                    AmenityId = Guid.NewGuid(), 
                    Name = "High-Speed WiFi", 
                    Icon = "wifi",
                    Description = "Fiber optic internet connection 1Gbps" // <--- ADDED THIS
                };
                
                var coffee = new Amenity 
                { 
                    AmenityId = Guid.NewGuid(), 
                    Name = "Free Coffee", 
                    Icon = "coffee",
                    Description = "Unlimited premium roasted coffee" // <--- ADDED THIS
                };
                
                var printer = new Amenity 
                { 
                    AmenityId = Guid.NewGuid(), 
                    Name = "Printing Services", 
                    Icon = "print",
                    Description = "Color and BW laser printing included" // <--- ADDED THIS
                };
        
                context.Amenities.AddRange(wifi, coffee, printer);
            }

            context.SaveChanges();
        }
    }
}