using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IUserService
    {

        Task<(bool success, User? user, string? errorMessage)> AuthenticateAsync(string username, string password);// xác thực đăng nhập


        Task<(bool success, string? errorMessage)> RegisterAsync(RegisterViewModel model); // đăng kí tài khoản mới

        Task<User?> GetProfileAsync(int userId); // lấy User bằng userid

        Task<(bool success, string? errorMessage)> UpdateProfileAsync(int userId, User model); // update profile

        
        Task<(bool success, string? errorMessage)> UpdateAvatarAsync(int userId, string avatarPath); // luu dường dẫn

        Task<ChangePasswordViewModel?> GetChangePasswordModelAsync(int userId); //đổi pass bằng id

        Task<(bool success, string? errorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    }
}
