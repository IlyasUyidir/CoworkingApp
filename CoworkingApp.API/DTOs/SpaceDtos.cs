using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.DTOs
{
    public class CreateSpaceRequest
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public SpaceType Type { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a positive number.")]
        public int Capacity { get; set; }
        
        public string Floor { get; set; }
        public float Area { get; set; }
        public string Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal PricePerHour { get; set; }
        
        [Required]
        public Guid LocationId { get; set; }
        
        public List<Guid> AmenityIds { get; set; }
    }

    public class SpaceResponse
    {
        public Guid SpaceId { get; set; }
        public string Name { get; set; }
        public SpaceType Type { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerHour { get; set; }
        public SpaceStatus Status { get; set; }
        public string LocationName { get; set; }
        public List<string> Amenities { get; set; }
    }
    
    public class SpaceSearchRequest
    {
        public DateTime? Date { get; set; }
        public SpaceType? Type { get; set; }
        public Guid? LocationId { get; set; }
    }
    public class UpdateSpaceRequest
    {
        [MinLength(1)]
        public string Name { get; set; }
        public SpaceType? Type { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a positive number.")]
        public int? Capacity { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal? PricePerHour { get; set; }
        public string Description { get; set; }
        public SpaceStatus? Status { get; set; } // Used for Maintenance/Active toggling
    }
}
