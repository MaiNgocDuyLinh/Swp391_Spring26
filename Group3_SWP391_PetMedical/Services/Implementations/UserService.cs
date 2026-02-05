using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Account;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<User?> GetProfileAsync(int userId)
        {
            return await _userRepo.GetByIdWithRoleAsync(userId);
        }

        public async Task<(bool success, string? errorMessage)> UpdateProfileAsync(int userId, User model)
        {
            var fullName = (model.full_name ?? "").Trim();
            var email = (model.email ?? "").Trim();
            var phone = string.IsNullOrWhiteSpace(model.phone) ? null : model.phone.Trim();

            if (await _userRepo.ExistsEmailByOtherUserAsync(email, userId))
                return (false, "Email này đã được sử dụng bởi tài khoản khác.");

            if (await _userRepo.ExistsPhoneByOtherUserAsync(phone, userId))
                return (false, "Số điện thoại này đã được sử dụng bởi tài khoản khác.");

            var ok = await _userRepo.UpdateProfileAsync(userId, fullName, email, phone);
            return ok ? (true, null) : (false, "Không tìm thấy tài khoản.");
        }

        public async Task<ChangePasswordViewModel?> GetChangePasswordModelAsync(int userId)
        {
            var user = await _userRepo.GetByIdWithRoleAsync(userId);
            if (user == null)
                return null;

            return new ChangePasswordViewModel
            {
                User = user
            };
        }

        public async Task<(bool success, string? errorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
        {
            return await _userRepo.UpdatePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
        }
    }
}
