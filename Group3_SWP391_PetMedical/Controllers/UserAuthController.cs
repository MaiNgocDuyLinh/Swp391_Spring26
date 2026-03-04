using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Group3_SWP391_PetMedical.Models;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class UserAuthController : Controller
    {
        private readonly ILogger<UserAuthController> _logger;
        private readonly PetClinicContext _context;

        public UserAuthController(ILogger<UserAuthController> logger, PetClinicContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
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

            if (user == null || user.password != password)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            if (user.status != "Active" && user.status != "Unactive")
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa hoặc không hoạt động.";
                return View();
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.role_id == user.role_id);
            var roleName = role?.role_name ?? "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.full_name),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity));

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

