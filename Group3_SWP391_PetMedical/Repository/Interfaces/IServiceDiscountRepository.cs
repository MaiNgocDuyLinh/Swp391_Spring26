using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IServiceDiscountRepository
    {
        /// <summary>Lấy danh sách dịch vụ kèm thông tin giảm giá đang active (có phân trang)</summary>
        Task<PagedResult<Service>> GetServicesWithDiscountPagedAsync(string? search, int page, int pageSize);

        /// <summary>Lấy thông tin giảm giá đang active của 1 dịch vụ</summary>
        Task<ServiceDiscount?> GetActiveDiscountByServiceIdAsync(int serviceId);

        /// <summary>Tạo hoặc cập nhật giảm giá cho dịch vụ</summary>
        Task<bool> ApplyDiscountAsync(int serviceId, int discountPercent, DateTime startDate, DateTime endDate);

        /// <summary>Hủy giảm giá (set is_active = false)</summary>
        Task<bool> RemoveDiscountAsync(int discountId);

        /// <summary>Lấy lịch sử giảm giá (tất cả, có phân trang)</summary>
        Task<PagedResult<ServiceDiscount>> GetDiscountHistoryPagedAsync(string? search, int page, int pageSize);
    }
}
