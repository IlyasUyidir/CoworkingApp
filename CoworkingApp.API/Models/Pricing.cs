using System;

namespace CoworkingApp.API.Models
{
    public class Pricing
    {
        public Guid PricingId { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal WeeklyRate { get; set; }
        public decimal MonthlyRate { get; set; }
        
        public Guid SpaceId { get; set; }
        public Space Space { get; set; }
        
        // Discount logic usually stored as JSON or separate table, kept simple here
        public string DiscountRules { get; set; }
    }
}