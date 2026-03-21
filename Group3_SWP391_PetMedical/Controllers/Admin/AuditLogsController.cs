using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Admin;

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
            // Hide Login records from Admin system log view (still kept for statistics elsewhere).
            var query = _context.AuditLogs.AsNoTracking().Where(l => l.Action != "Login");

            // Optional server-side filters (kept for backward compatibility with query params).
            if (!string.IsNullOrEmpty(searchUser))
            {
                query = query.Where(l => l.UserEmail != null && l.UserEmail.Contains(searchUser));
            }

            if (!string.IsNullOrEmpty(entity))
            {
                query = query.Where(l => l.EntityName != null && l.EntityName.Contains(entity));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= toDate.Value);
            }

            var rawLogs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Take(500)
                .ToListAsync();

            // Enrich logs with User/Role info.
            // Many older logs may miss UserId and store full_name in UserEmail; resolve by (UserId OR email OR full_name).
            var userIds = rawLogs
                .Where(l => l.UserId.HasValue)
                .Select(l => l.UserId!.Value)
                .Distinct()
                .ToList();

            var emails = rawLogs
                .Select(l => l.UserEmail)
                .Where(x => !string.IsNullOrWhiteSpace(x) && x!.Contains("@"))
                .Select(x => x!.Trim().ToLower())
                .Distinct()
                .ToList();

            var names = rawLogs
                .Select(l => l.UserEmail)
                .Where(x => !string.IsNullOrWhiteSpace(x) && !x!.Contains("@"))
                .Select(x => x!.Trim().ToLower())
                .Distinct()
                .ToList();

            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.role)
                .Where(u =>
                    userIds.Contains(u.user_id) ||
                    emails.Contains(u.email.ToLower()) ||
                    names.Contains(u.full_name.ToLower()))
                .ToListAsync();

            var byId = users.ToDictionary(u => u.user_id, u => u);
            var byEmail = users
                .Where(u => !string.IsNullOrWhiteSpace(u.email))
                .GroupBy(u => u.email.Trim().ToLower())
                .ToDictionary(g => g.Key, g => g.First());
            var byName = users
                .Where(u => !string.IsNullOrWhiteSpace(u.full_name))
                .GroupBy(u => u.full_name.Trim().ToLower())
                .ToDictionary(g => g.Key, g => g.First());

            User? ResolveUser(AuditLog l)
            {
                if (l.UserId.HasValue && byId.TryGetValue(l.UserId.Value, out var u1))
                    return u1;

                if (!string.IsNullOrWhiteSpace(l.UserEmail))
                {
                    var key = l.UserEmail.Trim().ToLower();
                    if (key.Contains("@") && byEmail.TryGetValue(key, out var u2))
                        return u2;
                    if (!key.Contains("@") && byName.TryGetValue(key, out var u3))
                        return u3;
                }

                return null;
            }

            var logs = rawLogs.Select(l =>
            {
                var u = ResolveUser(l);

                var email = u?.email;
                var fullName = u?.full_name;

                // Fallbacks from legacy log format
                if (string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(l.UserEmail) && l.UserEmail.Contains("@"))
                    email = l.UserEmail;
                if (string.IsNullOrWhiteSpace(fullName) && !string.IsNullOrWhiteSpace(l.UserEmail) && !l.UserEmail.Contains("@"))
                    fullName = l.UserEmail;

                return new AuditLogListItemVM
                {
                    UserId = l.UserId ?? u?.user_id,
                    Email = email,
                    FullName = fullName,
                    RoleName = u?.role?.role_name,
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityId = l.EntityId,
                    OldValues = l.OldValues,
                    NewValues = l.NewValues,
                    CreatedAt = l.CreatedAt
                };
            }).ToList();

            return View(logs);
        }
    }
}