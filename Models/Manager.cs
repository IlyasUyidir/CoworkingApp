using System;
using System.Collections.Generic;

namespace CoworkingApp.API.Models
{
    public class Manager : User
    {
        public Guid? ManagedLocationId { get; set; } // ADD THIS LINE
        public Location ManagedLocation { get; set; }
        public ICollection<Space> ManagedSpaces { get; set; }
    }
}