using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.ForgetPassword;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class ForgetPasswordController : Controller
    {
        private readonly IUserService _userService;

        public ForgetPasswordController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new SendResetOtpVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetCode(SendResetOtpVm model) // gửi code
        {
            if (!ModelState.IsValid)
                return View("ResetPassword", model);

            var (success, errorMessage) = await _userService.SendResetPasswordOtpAsync(model.Username, model.Email);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage ?? "Gửi mã OTP thất bại.";
                return View("ResetPassword", model);
            }

            TempData["SuccessMessage"] = "Đã gửi mã OTP tới email của bạn. Vui lòng kiểm tra hộp thư (kể cả Spam/Quảng cáo).";
            return View("ResetPassword", new SendResetOtpVm { Username = model.Username, Email = model.Email });
        }

        [HttpGet]
        public IActionResult ConfirmReset()
        {
            return View(new ConfirmResetPasswordVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReset(ConfirmResetPasswordVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage) = await _userService.ResetPasswordWithOtpAsync(
                model.Username, model.Email, model.Otp, model.NewPassword);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage ?? "Đặt lại mật khẩu thất bại.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới.";
            return RedirectToAction("Login", "Login");
        }
    }
}

