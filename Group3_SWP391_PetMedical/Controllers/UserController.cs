using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Group3_SWP391_PetMedical.Services.Interfaces;
using System.Security.Claims;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
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
