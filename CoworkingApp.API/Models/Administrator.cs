using System.Collections.Generic;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Models
{
    public class Administrator : User
    {
        public AdminLevel AdminLevel { get; set; }
        // Permissions handled via Role/Claim logic, simplified here as string list if needed
        public List<string> Permissions { get; set; } = new List<string>();
    }
}