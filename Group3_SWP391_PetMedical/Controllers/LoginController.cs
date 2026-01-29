using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Controllers
{


    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly PetClinicContext _context;

        public LoginController(ILogger<LoginController> logger, PetClinicContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Login()
        {

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            var user = _context.Users
                .FirstOrDefault(u => u.email == username || u.full_name == username);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            // Kiểm tra mật khẩu
            if (user.password != password)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            // Kiểm tra trạng thái tài khoản - chỉ chặn nếu bị khóa hoặc không phải Active/Unactive
            if (user.status != "Active" && user.status != "Unactive")
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa hoặc không hoạt động.";
                return View();
            }

            // Đăng nhập thành công (cho phép cả Active và Unactive)
            var role = _context.Roles.FirstOrDefault(r => r.role_id == user.role_id);
            var roleName = role?.role_name ?? "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.full_name),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

            // (Ghi Cookie vào trình duyệt)
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity));

            // Nếu user Unactive, có thể hiển thị thông báo nhắc cập nhật thông tin
            if (user.status == "Unactive")
            {
                TempData["InfoMessage"] = "Vui lòng cập nhật thông tin cá nhân để sử dụng đầy đủ các dịch vụ.";
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> LogoutGet()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra validation
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Vui lòng kiểm tra lại thông tin đã nhập.";
                return View(model);
            }

            // Kiểm tra email đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.email == model.Email.Trim());
            
            if (existingUser != null)
            {
                ViewBag.Error = "Email này đã được sử dụng. Vui lòng chọn email khác.";
                return View(model);
            }

            // Kiểm tra password và re_password khớp nhau
            if (model.Password != model.RePassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View(model);
            }

            // Kiểm tra đồng ý điều khoản
            if (!model.AgreeTerm)
            {
                ViewBag.Error = "Vui lòng đồng ý với điều khoản dịch vụ.";
                return View(model);
            }

            try
            {
                // Tìm role mặc định cho user đăng ký (thường là "User" hoặc "Customer")
                var defaultRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.role_name == "User" || r.role_name == "Customer");
                
                // Nếu không có role "User" hoặc "Customer", lấy role đầu tiên (hoặc role_id = 1)
                if (defaultRole == null)
                {
                    defaultRole = await _context.Roles
                        .OrderBy(r => r.role_id)
                        .FirstOrDefaultAsync();
                }

                if (defaultRole == null)
                {
                    ViewBag.Error = "Hệ thống chưa được cấu hình đúng. Vui lòng liên hệ quản trị viên.";
                    return View(model);
                }

                // Tạo user mới
                var newUser = new User
                {
                    email = model.Email.Trim(),
                    password = model.Password,
                    full_name = model.Name.Trim(),
                    role_id = defaultRole.role_id,
                    status = "Unactive",  // Chưa xác thực, cần kích hoạt sau
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
                return View(model);
            }
        }

    }
}
