using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Data;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly PetClinicContext _context;
        public UserRepository(PetClinicContext context) => _context = context;

        public async Task<PagedResult<User>> GetCustomersPagedAsync(string? search, int page, int pageSize)
        {
            // Lấy role_id của Customer
            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.role_name == "Customer");

            if (customerRole == null)
            {
                return new PagedResult<User>
                {
                    Items = Array.Empty<User>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = 0
                };
            }

            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.role_id == customerRole.role_id);

            // Tìm kiếm theo tên, email, phone
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(u => 
                    u.full_name.ToLower().Contains(search) ||
                    u.email.ToLower().Contains(search) ||
                    (u.phone ?? "").Contains(search));
            }

            query = query.OrderByDescending(u => u.created_at);

            return await query.ToPagedResultAsync(page, pageSize);
        }

        public async Task<User?> GetByIdWithRoleAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.role)
                .FirstOrDefaultAsync(u => u.user_id == userId);
        }

        public async Task UpdateAsync(User user)
        {
            var entry = _context.Entry(user);
            if (entry.State == EntityState.Detached)
                _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsEmailByOtherUserAsync(string email, int excludeUserId)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return await _context.Users
                .AnyAsync(u => u.email.Trim().ToLower() == email.Trim().ToLower() && u.user_id != excludeUserId);
        }

        public async Task<bool> ExistsPhoneByOtherUserAsync(string? phone, int excludeUserId)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return await _context.Users
                .AnyAsync(u => u.phone != null && u.phone.Trim() == phone.Trim() && u.user_id != excludeUserId);
        }

        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string email, string? phone)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == userId);
            if (user == null)
                return false;

            user.full_name = fullName;
            user.email = email;
            user.phone = phone;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string? errorMessage)> UpdatePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == userId);
            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            if (user.password != currentPassword)
                return (false, "Mật khẩu hiện tại không đúng.");

            user.password = newPassword;
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
