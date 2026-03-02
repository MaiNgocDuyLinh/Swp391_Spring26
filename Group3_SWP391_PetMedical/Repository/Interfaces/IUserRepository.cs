using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<PagedResult<User>> GetCustomersPagedAsync(string? search, int page, int pageSize);

        /// <summary>Lấy user theo id, có Include role.</summary>
        Task<User?> GetByIdWithRoleAsync(int userId);

        /// <summary>Cập nhật entity User (đã load và sửa), gọi SaveChanges.</summary>
        Task UpdateAsync(User user);

        /// <summary>Kiểm tra email đã tồn tại bởi user khác (loại trừ excludeUserId).</summary>
        Task<bool> ExistsEmailByOtherUserAsync(string email, int excludeUserId);

        /// <summary>Kiểm tra số điện thoại đã tồn tại bởi user khác (loại trừ excludeUserId).</summary>
        Task<bool> ExistsPhoneByOtherUserAsync(string? phone, int excludeUserId);

        /// <summary>Cập nhật profile: full_name, email, phone. Trả về true nếu thành công.</summary>
        Task<bool> UpdateProfileAsync(int userId, string fullName, string email, string? phone);

        /// <summary>Đổi mật khẩu: kiểm tra currentPassword, gán newPassword và lưu. Trả về (success, errorMessage).</summary>
        Task<(bool success, string? errorMessage)> UpdatePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
