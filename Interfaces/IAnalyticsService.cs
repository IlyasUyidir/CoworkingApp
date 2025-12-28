using System.Threading.Tasks;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Interfaces
{
    public interface IAnalyticsService
    {
        Task<Analytics> GetDashboardMetricsAsync();
    }
}