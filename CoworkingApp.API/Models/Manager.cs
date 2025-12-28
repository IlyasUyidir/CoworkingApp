using System.Collections.Generic;

namespace CoworkingApp.API.Models
{
    public class Manager : User
    {
        public ICollection<Space> ManagedSpaces { get; set; }
        public Location ManagedLocation { get; set; }
    }
}