using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Microsoft.AspNetCore.Http;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IUserService
    {
        Task<(bool success, User? user, string? errorMessage)> AuthenticateAsync(string username, string password);
        Task<(bool success, string? errorMessage)> RegisterAsync(RegisterViewModel model);
        Task<User?> GetProfileAsync(int userId);
        Task<(bool success, string? errorMessage)> UpdateProfileAsync(int userId, User model);
        /// <summary>Cập nhật ảnh đại diện: validate, xóa ảnh cũ, lưu ảnh mới (tối đa 2MB).</summary>
        Task<(bool success, string? errorMessage)> UpdateAvatarAsync(int userId, IFormFile avatarFile);
        /// <summary>Gửi mã OTP reset mật khẩu qua email và lưu token/expiry vào Users.</summary>
        Task<(bool success, string? errorMessage)> SendResetPasswordOtpAsync(string username, string email);
        Task<ChangePasswordViewModel?> GetChangePasswordModelAsync(int userId);
        Task<(bool success, string? errorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
        /// <summary>Xác thực OTP và đặt lại mật khẩu mới.</summary>
        Task<(bool success, string? errorMessage)> ResetPasswordWithOtpAsync(string username, string email, string otp, string newPassword);
    }
}
