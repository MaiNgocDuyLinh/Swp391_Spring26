using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Manager;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IManagerModuleService
    {
        Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize);
        Task<Service?> GetServiceByIdAsync(int id);
        Task<bool> UpdateServiceAsync(int id, decimal basePrice, string? description);

        Task<List<ScheduleChangeRequestListVM>> GetScheduleChangeRequestsAsync(string? status = "Pending");
        Task<ScheduleChangeRequestDetailVM?> GetScheduleChangeRequestByIdAsync(int requestId);
        Task<bool> ApproveScheduleChangeRequestAsync(int requestId, int managerUserId, string? managerNote);
        Task<bool> RejectScheduleChangeRequestAsync(int requestId, int managerUserId, string? managerNote);

        Task<OverviewStatsVM> GetOverviewStatsAsync(DateTime? fromDate, DateTime? toDate, string? groupBy);
    }
}

