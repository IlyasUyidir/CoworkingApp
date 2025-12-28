using System;

namespace CoworkingApp.API.Models
{
    public class TimeSlot
    {
        public Guid SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Available { get; set; }
        
        public Guid SpaceId { get; set; }
        public Space Space { get; set; }
    }
}