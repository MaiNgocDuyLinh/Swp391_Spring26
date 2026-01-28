using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Data;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly PetClinicContext _context;
        public AppointmentRepository(PetClinicContext context) => _context = context;

        public async Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize)
        {
            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.customer)
                .Include(a => a.pet)
                .Include(a => a.doctor)
                .Include(a => a.AppointmentDetails)
                    .ThenInclude(ad => ad.service)
                .Where(a => a.appointment_date.Date == date.Date);

            // Tìm kiếm theo tên khách hàng, tên thú cưng, tên bác sĩ
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(a => 
                    (a.customer != null && a.customer.full_name.ToLower().Contains(search)) ||
                    (a.pet != null && a.pet.name.ToLower().Contains(search)) ||
                    (a.doctor != null && a.doctor.full_name.ToLower().Contains(search)) ||
                    (a.notes ?? "").ToLower().Contains(search));
            }

            query = query.OrderBy(a => a.appointment_date);

            return await query.ToPagedResultAsync(page, pageSize);
        }
    }
}
