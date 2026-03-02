using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Models;
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

            var loginName = username?.Trim();
            if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.username == loginName);

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
    }
}
