using System;

namespace CoworkingApp.API.Models
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Helpful { get; set; }

        public Guid SpaceId { get; set; }
        public Space Space { get; set; }

        public Guid MemberId { get; set; }
        public Member Member { get; set; }
    }
}