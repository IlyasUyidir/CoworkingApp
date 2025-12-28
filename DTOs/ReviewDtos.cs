using System;
using System.ComponentModel.DataAnnotations;

namespace CoworkingApp.API.DTOs
{
    public class CreateReviewRequest
    {
        [Required]
        public Guid SpaceId { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public string Comment { get; set; }
    }

    public class ReviewResponse
    {
        public Guid ReviewId { get; set; }
        public string MemberName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}