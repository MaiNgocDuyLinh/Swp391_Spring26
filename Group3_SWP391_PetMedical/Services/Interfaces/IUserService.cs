using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetProfileAsync(int userId);

        Task<(bool success, string? errorMessage)> UpdateProfileAsync(int userId, User model);

        Task<ChangePasswordViewModel?> GetChangePasswordModelAsync(int userId);

        Task<(bool success, string? errorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    }
}
