using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IUserService _userService;

        public LoginController(ILogger<LoginController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var (success, user, errorMessage) = await _userService.AuthenticateAsync(username, password);
            if (!success || user == null)
            {
                ViewBag.Error = errorMessage ?? "Sai tài khoản hoặc mật khẩu!";
                return View();
            }
            var roleName = user.role?.role_name ?? "User";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.full_name ?? ""),
                new Claim(ClaimTypes.Email, user.email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity));

            if (user.status == "Unactive")
                TempData["InfoMessage"] = "Vui lòng cập nhật thông tin cá nhân để sử dụng đầy đủ các dịch vụ.";

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

        public IActionResult Contact()
        {
            return View();
        }
    }
}
