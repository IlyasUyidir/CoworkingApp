using System;
using System.ComponentModel.DataAnnotations; // <--- This was missing

namespace CoworkingApp.API.Models
{
    public class TimeSlot
    {
        [Key] // <--- This marks SlotId as the Primary Key
        public Guid SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Available { get; set; }

        public Guid SpaceId { get; set; }
        public Space Space { get; set; }
    }
}