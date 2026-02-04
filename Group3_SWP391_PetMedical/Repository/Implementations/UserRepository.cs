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
    }
}
