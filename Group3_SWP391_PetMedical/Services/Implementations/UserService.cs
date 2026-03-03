using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Account;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepo, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<(bool success, User? user, string? errorMessage)> AuthenticateAsync(string username, string password)
        {
            var loginName = username?.Trim();
            if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(password))
                return (false, null, "Vui lòng nhập đầy đủ thông tin!");

            var user = await _userRepo.GetByUsernameWithRoleAsync(loginName);
            if (user == null)
                return (false, null, "Sai tài khoản hoặc mật khẩu!");

            if (user.password != password)
                return (false, null, "Sai tài khoản hoặc mật khẩu!");

            if (user.status != "Active" && user.status != "Unactive")
                return (false, null, "Tài khoản của bạn đã bị khóa hoặc không hoạt động.");

            return (true, user, null);
        }

        public async Task<(bool success, string? errorMessage)> RegisterAsync(RegisterViewModel model)
        {
            var username = model.Username?.Trim();
            var email = model.Email?.Trim();

            if (string.IsNullOrEmpty(username))
                return (false, "Vui lòng nhập tên đăng nhập.");

            if (string.IsNullOrWhiteSpace(email))
                return (false, "Vui lòng nhập email.");

            if (await _userRepo.ExistsUsernameAsync(username))
                return (false, "Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên khác.");

            if (await _userRepo.ExistsEmailAsync(email!))
                return (false, "Email này đã được sử dụng. Vui lòng chọn email khác.");

            if (model.Password != model.RePassword)
                return (false, "Mật khẩu xác nhận không khớp.");

            if (!model.AgreeTerm)
                return (false, "Vui lòng đồng ý với điều khoản dịch vụ.");

            var defaultRole = await _userRepo.GetDefaultRoleAsync();
            if (defaultRole == null)
                return (false, "Hệ thống chưa được cấu hình đúng. Vui lòng liên hệ quản trị viên.");

            var newUser = new User
            {
                username = username,
                email = email,
                password = model.Password,
                full_name = model.Name.Trim(),
                role_id = defaultRole.role_id,
                status = "Unactive",
                created_at = DateTime.Now
            };

            try
            {
                await _userRepo.AddAsync(newUser);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return (false, "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại sau.");
            }
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

        public async Task<(bool success, string? errorMessage)> UpdateAvatarAsync(int userId, string avatarPath)
        {
            var ok = await _userRepo.UpdateAvatarAsync(userId, avatarPath);
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
