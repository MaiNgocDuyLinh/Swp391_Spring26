using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Models;

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


            if (user != null && user.password == password && user.status == "Active")
            {
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

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View();
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
