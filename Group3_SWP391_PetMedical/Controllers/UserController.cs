using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Group3_SWP391_PetMedical.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvatar(IFormFile? avatarFile)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Login");

            if (avatarFile == null || avatarFile.Length <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh đại diện.";
                return RedirectToAction(nameof(Profile));
            }

            const long maxBytes = 20L * 1024 * 1024; // 20MB
            if (avatarFile.Length > maxBytes)
            {
                TempData["ErrorMessage"] = "Ảnh quá lớn (tối đa 20 MB).";
                return RedirectToAction(nameof(Profile));
            }

            var ext = Path.GetExtension(avatarFile.FileName)?.ToLowerInvariant();
            var allowedExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp"
            };
            if (string.IsNullOrEmpty(ext) || !allowedExts.Contains(ext))
            {
                TempData["ErrorMessage"] = "Định dạng ảnh không hợp lệ. Vui lòng chọn JPG/PNG/GIF/WEBP.";
                return RedirectToAction(nameof(Profile));
            }

            var user = await _userService.GetProfileAsync(userId.Value);
            if (user == null)
                return NotFound();

            var safeUsername = Regex.Replace(user.username ?? "user", @"[^a-zA-Z0-9_-]+", "_");
            var fileName = $"{safeUsername}_{user.user_id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";

            var avatarDir = Path.Combine(_env.WebRootPath, "uploads", "userAvatar");
            Directory.CreateDirectory(avatarDir);

            var physicalPath = Path.Combine(avatarDir, fileName);
            var virtualPath = $"/uploads/userAvatar/{fileName}";

            try
            {
                await using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                var oldAvatar = user.avatar;
                var (success, errorMessage) = await _userService.UpdateAvatarAsync(user.user_id, virtualPath);
                if (!success)
                {
                    if (System.IO.File.Exists(physicalPath))
                        System.IO.File.Delete(physicalPath);

                    TempData["ErrorMessage"] = errorMessage ?? "Cập nhật ảnh đại diện thất bại.";
                    return RedirectToAction(nameof(Profile));
                }

                // Optional: xóa ảnh cũ nếu là file trong uploads/userAvatar
                if (!string.IsNullOrWhiteSpace(oldAvatar) &&
                    oldAvatar.StartsWith("/uploads/userAvatar/", StringComparison.OrdinalIgnoreCase))
                {
                    var oldPhysical = Path.Combine(_env.WebRootPath, oldAvatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPhysical))
                        System.IO.File.Delete(oldPhysical);
                }

                TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công.";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating avatar for user {UserId}", user.user_id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải ảnh lên. Vui lòng thử lại.";
                return RedirectToAction(nameof(Profile));
            }
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
