using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly IUserService _userService;

        public RegisterController(ILogger<RegisterController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home"); 
            return View("~/Views/Login/Register.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Vui lòng kiểm tra lại thông tin đã nhập.";
                return View("~/Views/Login/Register.cshtml", model);
            }

            var (success, errorMessage) = await _userService.RegisterAsync(model);
            if (!success)
            {
                ViewBag.Error = errorMessage ?? "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại sau.";
                return View("~/Views/Login/Register.cshtml", model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay. Vui lòng cập nhật thông tin cá nhân để sử dụng đầy đủ các dịch vụ.";
            return RedirectToAction("Login", "Login");
        }
    }
}
