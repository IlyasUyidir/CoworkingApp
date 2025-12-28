using System;

namespace CoworkingApp.API.DTOs
{
    public class AmenityDto
    {
        public Guid AmenityId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class LocationDto
    {
        public Guid LocationId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
    }
}