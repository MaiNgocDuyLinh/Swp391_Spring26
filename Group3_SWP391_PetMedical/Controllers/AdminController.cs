using Group3_SWP391_PetMedical.Attributes;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(PetClinicContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? roleFilter, string? statusFilter, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Users.Include(u => u.role).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    query = query.Where(u =>
                        u.email.Contains(term) ||
                        u.full_name.Contains(term) ||
                        (u.phone != null && u.phone.Contains(term)));
                }

                if (!string.IsNullOrWhiteSpace(roleFilter) && int.TryParse(roleFilter, out var roleId))
                    query = query.Where(u => u.role_id == roleId);

                if (!string.IsNullOrWhiteSpace(statusFilter))
                    query = query.Where(u => u.status == statusFilter);

                var totalRecords = await query.CountAsync();
                var totalPages = Math.Max(1, (int)Math.Ceiling(totalRecords / (double)pageSize));
                page = Math.Clamp(page, 1, totalPages);

                var users = await query
                    .OrderByDescending(u => u.created_at)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var model = new AccountListViewModel
                {
                    Accounts = users.Select(u => new AccountViewModel
                    {
                        UserId = u.user_id,
                        Email = u.email,
                        FullName = u.full_name,
                        Phone = u.phone,
                        RoleName = u.role.role_name,
                        RoleId = u.role_id,
                        Status = u.status ?? "Active",
                        CreatedAt = u.created_at
                    }).ToList(),
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    PageSize = pageSize,
                    SearchTerm = searchTerm?.Trim(),
                    RoleFilter = roleFilter,
                    StatusFilter = statusFilter,
                    AvailableRoles = await _context.Roles
                        .Select(r => new RoleFilterOption { RoleId = r.role_id, RoleName = r.role_name })
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accounts");
                ViewBag.Error = "Có lỗi khi tải danh sách tài khoản.";
                return View(new AccountListViewModel { AvailableRoles = new List<RoleFilterOption>() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateAccountViewModel
            {
                AvailableRoles = await _context.Roles
                    .Where(r => r.role_name == "Staff" || r.role_name == "Doctor")
                    .Select(r => new RoleOption { RoleId = r.role_id, RoleName = r.role_name })
                    .ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            try
            {
                model.AvailableRoles = await _context.Roles
                    .Where(r => r.role_name == "Staff" || r.role_name == "Doctor")
                    .Select(r => new RoleOption { RoleId = r.role_id, RoleName = r.role_name })
                    .ToListAsync();

                if (await _context.Users.AnyAsync(u => u.email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.role_id == model.RoleId && (r.role_name == "Staff" || r.role_name == "Doctor"));
                if (role == null)
                {
                    ModelState.AddModelError("RoleId", "Vai trò không hợp lệ.");
                    return View(model);
                }

                if (!ModelState.IsValid)
                    return View(model);

                var user = new User
                {
                    email = model.Email.Trim(),
                    password = model.Password,
                    full_name = model.FullName.Trim(),
                    phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                    role_id = model.RoleId,
                    status = "Active",
                    created_at = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã tạo tài khoản {user.email} thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                ViewBag.Error = "Có lỗi khi tạo tài khoản.";
                model.AvailableRoles = await _context.Roles
                    .Where(r => r.role_name == "Staff" || r.role_name == "Doctor")
                    .Select(r => new RoleOption { RoleId = r.role_id, RoleName = r.role_name })
                    .ToListAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.Include(u => u.role).FirstOrDefaultAsync(u => u.user_id == id);
            if (user == null) return NotFound();
            if (user.role.role_name == "Admin")
            {
                TempData["ErrorMessage"] = "Không thể sửa tài khoản Admin.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditAccountViewModel
            {
                UserId = user.user_id,
                Email = user.email,
                FullName = user.full_name,
                Phone = user.phone,
                RoleId = user.role_id,
                Status = user.status ?? "Active",
                AvailableRoles = await _context.Roles
                    .Select(r => new RoleOption { RoleId = r.role_id, RoleName = r.role_name })
                    .ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            try
            {
                model.AvailableRoles = await _context.Roles
                    .Select(r => new RoleOption { RoleId = r.role_id, RoleName = r.role_name })
                    .ToListAsync();

                var user = await _context.Users.Include(u => u.role).FirstOrDefaultAsync(u => u.user_id == model.UserId);
                if (user == null) return NotFound();
                if (user.role.role_name == "Admin")
                {
                    TempData["ErrorMessage"] = "Không thể sửa tài khoản Admin.";
                    return RedirectToAction(nameof(Index));
                }

                var role = await _context.Roles.FindAsync(model.RoleId);
                if (role == null)
                {
                    ModelState.AddModelError("RoleId", "Vai trò không hợp lệ.");
                    model.AvailableRoles = await _context.Roles
                        .Select(r => new RoleOption { RoleId = r.role_id, RoleName = r.role_name })
                        .ToListAsync();
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    model.AvailableRoles = await _context.Roles
                        .Select(r => new RoleOption { RoleId = r.role_id, RoleName = r.role_name })
                        .ToListAsync();
                    return View(model);
                }

                user.full_name = model.FullName.Trim();
                user.phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
                user.role_id = model.RoleId;
                user.status = model.Status;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã cập nhật tài khoản {user.email}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account");
                ViewBag.Error = "Có lỗi khi cập nhật tài khoản.";
                model.Email = model.Email ?? (await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.user_id == model.UserId))?.email;
                model.AvailableRoles = await _context.Roles
                    .Select(r => new RoleOption { RoleId = r.role_id, RoleName = r.role_name })
                    .ToListAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users.Include(u => u.role).FirstOrDefaultAsync(u => u.user_id == id);
            if (user == null) return NotFound();

            var vm = new AccountViewModel
            {
                UserId = user.user_id,
                Email = user.email,
                FullName = user.full_name,
                Phone = user.phone,
                RoleName = user.role.role_name,
                RoleId = user.role_id,
                Status = user.status ?? "Active",
                CreatedAt = user.created_at
            };
            return View(vm);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ToggleLock(int id)
        {
            try
            {
                var user = await _context.Users.Include(u => u.role).FirstOrDefaultAsync(u => u.user_id == id);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản." });
                if (user.role.role_name == "Admin")
                    return Json(new { success = false, message = "Không thể khóa tài khoản Admin." });

                user.status = user.status == "Active" ? "Inactive" : "Active";
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = user.status == "Active" ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.",
                    newStatus = user.status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleLock error for user {UserId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra." });
            }
        }
    }
}
