using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<UserService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;

        private const long MaxAvatarSizeBytes = 2 * 1024 * 1024; // 2 MB
        private static readonly string[] AllowedAvatarExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public UserService(IUserRepository userRepo, ILogger<UserService> logger, IWebHostEnvironment env, IEmailSender emailSender)
        {
            _userRepo = userRepo;
            _logger = logger;
            _env = env;
            _emailSender = emailSender;
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

        public async Task<(bool success, string? errorMessage)> UpdateAvatarAsync(int userId, IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
                return (false, "Vui lòng chọn một ảnh để tải lên.");

            var error = ValidateAvatarFile(avatarFile);
            if (error != null)
                return (false, error);

            var user = await _userRepo.GetByIdWithRoleAsync(userId);
            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            var safeUsername = new string((user.username ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrWhiteSpace(safeUsername))
                safeUsername = "user";
            var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            var fileName = $"{safeUsername}_{user.user_id}{extension}";

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "userAvatar");
            Directory.CreateDirectory(uploadsDir);
            var filePath = Path.Combine(uploadsDir, fileName);

            DeleteOldAvatarIfAny(user.avatar, filePath);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var virtualPath = $"/uploads/userAvatar/{fileName}";
            var ok = await _userRepo.UpdateAvatarAsync(userId, virtualPath);
            return ok ? (true, null) : (false, "Cập nhật ảnh đại diện thất bại.");
        }

        private static string? ValidateAvatarFile(IFormFile file)
        {
            if (file.Length > MaxAvatarSizeBytes)
                return "Ảnh quá lớn. Vui lòng chọn ảnh nhỏ hơn 2 MB.";
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedAvatarExtensions.Contains(ext))
                return "Định dạng ảnh không hợp lệ. Chỉ hỗ trợ jpg, jpeg, png, gif, webp.";
            return null;
        }

        private void DeleteOldAvatarIfAny(string? avatarVirtualPath, string newFilePath)
        {
            if (string.IsNullOrWhiteSpace(avatarVirtualPath) ||
                !avatarVirtualPath.StartsWith("/uploads/userAvatar/", StringComparison.OrdinalIgnoreCase))
                return;
            var oldPhysical = Path.Combine(_env.WebRootPath,
                avatarVirtualPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            try
            {
                if (File.Exists(oldPhysical) &&
                    !string.Equals(oldPhysical, newFilePath, StringComparison.OrdinalIgnoreCase))
                    File.Delete(oldPhysical);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể xóa ảnh avatar cũ.");
            }
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

        public async Task<(bool success, string? errorMessage)> SendResetPasswordOtpAsync(string username, string email)
        {
            username = username?.Trim() ?? "";
            email = email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
                return (false, "Vui lòng nhập đầy đủ tài khoản và email.");

            var user = await _userRepo.GetByUsernameAndEmailAsync(username, email);
            if (user == null)
                return (false, "Tài khoản hoặc email không đúng.");

            var otp = GenerateOtp6Digits();
            user.reset_password_token = otp;
            user.reset_password_expiry = DateTime.UtcNow.AddMinutes(15);

            await _userRepo.UpdateAsync(user);

            try
            {
                var subject = "Mã OTP đặt lại mật khẩu";
                var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;line-height:1.5'>
  <h2 style='margin:0 0 8px;color:#0b1c39'>PetMedical - Quên mật khẩu</h2>
  <p>Mã OTP của bạn là:</p>
  <div style='font-size:28px;font-weight:800;letter-spacing:6px;color:#ff3d1c'>{otp}</div>
  <p style='color:#5d6b82'>Mã có hiệu lực trong 15 phút. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
</div>";

                await _emailSender.SendAsync(user.email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send reset OTP email failed for user {UserId}", user.user_id);
                return (false, "Không thể gửi email OTP lúc này. Vui lòng thử lại sau.");
            }

            return (true, null);
        }

        public async Task<(bool success, string? errorMessage)> ResetPasswordWithOtpAsync(string username, string email, string otp, string newPassword)
        {
            username = username?.Trim() ?? "";
            email = email?.Trim() ?? "";
            otp = otp?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(otp) ||
                string.IsNullOrWhiteSpace(newPassword))
            {
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            }

            var user = await _userRepo.GetByUsernameAndEmailAsync(username, email);
            if (user == null)
                return (false, "Tài khoản hoặc email không đúng.");

            if (string.IsNullOrEmpty(user.reset_password_token) ||
                !string.Equals(user.reset_password_token, otp, StringComparison.Ordinal))
            {
                return (false, "Mã OTP không đúng.");
            }

            if (user.reset_password_expiry == null || user.reset_password_expiry < DateTime.UtcNow)
            {
                return (false, "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.");
            }

            user.password = newPassword;
            user.reset_password_token = null;
            user.reset_password_expiry = null;

            await _userRepo.UpdateAsync(user);
            return (true, null);
        }

        private static string GenerateOtp6Digits() // tạo opt 6 số
        {
            // secure random 000000-999999
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            var n = BitConverter.ToUInt32(bytes) % 1_000_000;
            return n.ToString("D6");
        }
    }
}
