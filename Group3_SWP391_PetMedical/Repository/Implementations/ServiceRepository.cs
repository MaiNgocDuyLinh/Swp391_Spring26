using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Data;
using Microsoft.EntityFrameworkCore;

public class ServiceRepository : IServiceRepository
{
    private readonly PetClinicContext _context;
    public ServiceRepository(PetClinicContext context) => _context = context;

    public async Task<PagedResult<Service>> GetPagedAsync(string? q, int page, int pageSize)
    {
        var query = _context.Services.AsNoTracking().AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(s => s.service_name.Contains(q) ||
                                     (s.description ?? "").Contains(q));
        }

        // Option: chỉ lấy đang hoạt động
        query = query.Where(s => s.status == true);

        // Sort
        query = query.OrderBy(s => s.service_name);

        return await query.ToPagedResultAsync(page, pageSize);
    }
}
