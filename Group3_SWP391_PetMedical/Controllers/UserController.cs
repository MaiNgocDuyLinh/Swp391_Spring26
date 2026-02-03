using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using System.Security.Claims;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class UserController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(PetClinicContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Login");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Login");
            }

            var user = await _context.Users
                .Include(u => u.role)
                .FirstOrDefaultAsync(u => u.user_id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            idStr ??= User.FindFirstValue("user_id");

            if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out int id))
                throw new Exception("Không lấy được user_id từ Claims. Hãy kiểm tra Login tạo Claims.");

            return id;
        }
    }
}
