using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using System.Linq;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _env;

        public UserController(IUserService userService, ILogger<UserController> logger, IWebHostEnvironment env)
        {
            _userService = userService;
            _logger = logger;
            _env = env;
        }

        // lay user_id tu Claims
        private int? GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated != true)
                return null;

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return null;

            return userId;
        }

        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Login");

            var user = await _userService.GetProfileAsync(userId.Value);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvatar(IFormFile? avatarFile)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Login");

            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một ảnh để tải lên.";
                return RedirectToAction(nameof(Profile));
            }

            const long maxFileSize = 20 * 1024 * 1024;
            if (avatarFile.Length > maxFileSize)
            {
                TempData["ErrorMessage"] = "Ảnh quá lớn. Vui lòng chọn ảnh nhỏ hơn 20 MB.";
                return RedirectToAction(nameof(Profile));
            }

            var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (string.IsNullOrEmpty(extension) || !allowedExts.Contains(extension))
            {
                TempData["ErrorMessage"] = "Định dạng ảnh không hợp lệ. Chỉ hỗ trợ jpg, jpeg, png, gif, webp.";
                return RedirectToAction(nameof(Profile));
            }

            var user = await _userService.GetProfileAsync(userId.Value);
            if (user == null)
                return NotFound();

            var safeUsername = new string((user.username ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .ToArray());
            if (string.IsNullOrWhiteSpace(safeUsername))
                safeUsername = "user";
            var fileName = $"{safeUsername}_{user.user_id}{extension}";

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "userAvatar");
            Directory.CreateDirectory(uploadsDir);
            var filePath = Path.Combine(uploadsDir, fileName);

            if (!string.IsNullOrWhiteSpace(user.avatar) &&
                user.avatar.StartsWith("/uploads/userAvatar/", StringComparison.OrdinalIgnoreCase))
            {
                var oldPhysical = Path.Combine(_env.WebRootPath,
                    user.avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                try
                {
                    if (System.IO.File.Exists(oldPhysical) &&
                        !string.Equals(oldPhysical, filePath, StringComparison.OrdinalIgnoreCase))
                    {
                        System.IO.File.Delete(oldPhysical);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể xóa ảnh avatar cũ cho user {UserId}", user.user_id);
                }
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var virtualPath = $"/uploads/userAvatar/{fileName}";
            var (success, errorMessage) = await _userService.UpdateAvatarAsync(user.user_id, virtualPath);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage ?? "Cập nhật ảnh đại diện thất bại.";
                return RedirectToAction(nameof(Profile));
            }

            TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([Bind(Prefix = "")] User model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Login");

            if (model == null)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Profile));
            }

            if (string.IsNullOrWhiteSpace(model.full_name) || string.IsNullOrWhiteSpace(model.email))
            {
                TempData["ErrorMessage"] = "Họ và tên và Email không được để trống.";
                var user = await _userService.GetProfileAsync(userId.Value);
                if (user == null) return NotFound();
                user.full_name = model.full_name;
                user.email = model.email;
                user.phone = model.phone;
                return View(user);
            }

            var (success, errorMessage) = await _userService.UpdateProfileAsync(userId.Value, model);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage ?? "Cập nhật thất bại.";
                var user = await _userService.GetProfileAsync(userId.Value);
                if (user == null) return NotFound();
                user.full_name = model.full_name;
                user.email = model.email;
                user.phone = model.phone;
                return View(user);
            }   

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Login");

            var model = await _userService.GetChangePasswordModelAsync(userId.Value);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Login");

            var vm = await _userService.GetChangePasswordModelAsync(userId.Value);
            if (vm == null)
                return NotFound();

            model.User = vm.User;

            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage) = await _userService.ChangePasswordAsync(userId.Value, model);
            if (!success)
            {
                ModelState.AddModelError("CurrentPassword", errorMessage ?? "Đổi mật khẩu thất bại.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));

        }
    }
}
