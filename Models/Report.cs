using System;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Models
{
    public class Report
    {
        public Guid ReportId { get; set; }
        public ReportType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Data { get; set; } // JSON content
        public DateTime GeneratedAt { get; set; }
        
        public Guid GeneratedByAdminId { get; set; }
        public Administrator GeneratedBy { get; set; }
    }
}