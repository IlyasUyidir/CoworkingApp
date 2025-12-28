using System;
using System.Collections.Generic;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Models
{
    public class Space
    {
        public Guid SpaceId { get; set; }
        public string Name { get; set; }
        public SpaceType Type { get; set; }
        public int Capacity { get; set; }
        public string Floor { get; set; }
        public float Area { get; set; }
        public string Description { get; set; }
        public decimal PricePerHour { get; set; }
        public SpaceStatus Status { get; set; }

        public Guid LocationId { get; set; }
        public Location Location { get; set; }

        public ICollection<Amenity> Amenities { get; set; }
        public ICollection<TimeSlot> TimeSlots { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Review> Reviews { get; set; }
        
        // Navigation for Manager relationship
        public Guid? ManagerId { get; set; }
        public Manager Manager { get; set; }
    }
}