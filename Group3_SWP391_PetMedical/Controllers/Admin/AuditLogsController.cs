using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;

namespace Group3_SWP391_PetMedical.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly PetClinicContext _context;

        public AuditLogsController(PetClinicContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? searchUser,
            string? entity,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.AuditLogs.AsQueryable();

            // 🔎 Filter by UserEmail
            if (!string.IsNullOrEmpty(searchUser))
            {
                query = query.Where(x => x.UserEmail != null &&
                                         x.UserEmail.Contains(searchUser));
            }

            // 🔎 Filter by EntityName
            if (!string.IsNullOrEmpty(entity))
            {
                query = query.Where(x => x.EntityName != null &&
                                         x.EntityName.Contains(entity));
            }

            // 📅 From Date
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= fromDate.Value);
            }

            // 📅 To Date
            if (toDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= toDate.Value);
            }

            var logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(500)
                .ToListAsync();

            return View(logs);
        }
    }
}