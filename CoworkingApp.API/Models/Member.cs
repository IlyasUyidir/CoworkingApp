using System;
using System.Collections.Generic;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Models
{
    public class Member : User
    {
        public MembershipType MembershipType { get; set; }
        public DateTime MembershipStartDate { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public int TotalBookings { get; set; }
        
        public ICollection<Reservation> Reservations { get; set; }
    }
}