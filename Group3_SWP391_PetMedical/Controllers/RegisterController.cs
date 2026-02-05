using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly PetClinicContext _context;

        public RegisterController(ILogger<RegisterController> logger, PetClinicContext context)
        {
            _logger = logger;
            _context = context;
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

            var username = model.Username?.Trim();
            var email = model.Email?.Trim();

            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập.";
                return View("~/Views/Login/Register.cshtml", model);
            }

            var existingByUsername = await _context.Users
                .FirstOrDefaultAsync(u => u.username == username);
            if (existingByUsername != null)
            {
                ViewBag.Error = "Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên khác.";
                return View("~/Views/Login/Register.cshtml", model);
            }

            var existingByEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.email == email);
            if (existingByEmail != null)
            {
                ViewBag.Error = "Email này đã được sử dụng. Vui lòng chọn email khác.";
                return View("~/Views/Login/Register.cshtml", model);
            }

            if (model.Password != model.RePassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View("~/Views/Login/Register.cshtml", model);
            }

            if (!model.AgreeTerm)
            {
                ViewBag.Error = "Vui lòng đồng ý với điều khoản dịch vụ.";
                return View("~/Views/Login/Register.cshtml", model);
            }

            try
            {
                var defaultRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.role_name == "User" || r.role_name == "Customer");

                if (defaultRole == null)
                    defaultRole = await _context.Roles.OrderBy(r => r.role_id).FirstOrDefaultAsync();

                if (defaultRole == null)
                {
                    ViewBag.Error = "Hệ thống chưa được cấu hình đúng. Vui lòng liên hệ quản trị viên.";
                    return View("~/Views/Login/Register.cshtml", model);
                }

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

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay. Vui lòng cập nhật thông tin cá nhân để sử dụng đầy đủ các dịch vụ.";
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                ViewBag.Error = "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại sau.";
                return View("~/Views/Login/Register.cshtml", model);
            }
        }
    }
}
