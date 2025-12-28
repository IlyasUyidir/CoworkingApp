using System;
using System.Collections.Generic;

namespace CoworkingApp.API.Models
{
    public class Amenity
    {
        public Guid AmenityId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        
        public ICollection<Space> Spaces { get; set; }
    }
}