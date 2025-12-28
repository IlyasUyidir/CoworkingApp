using System;
using System.Collections.Generic;

namespace CoworkingApp.API.Models
{
    public class Calendar
    {
        public Guid CalendarId { get; set; }
        public DateTime Date { get; set; }
        
        public Guid SpaceId { get; set; }
        public Space Space { get; set; }

        public ICollection<TimeSlot> Slots { get; set; }
    }
}