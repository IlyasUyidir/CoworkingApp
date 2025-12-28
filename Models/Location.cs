using System;
using System.Collections.Generic;

namespace CoworkingApp.API.Models
{
    public class Location
    {
        public Guid LocationId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Coordinates { get; set; } // GeoPoint stored as string
        public string Facilities { get; set; } // JSON or CSV
        public string OpeningHours { get; set; } 
        
        public ICollection<Space> Spaces { get; set; }
    }
}