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

        public async Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize)
        {
            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.customer)
                .Include(a => a.pet)
                .Include(a => a.doctor)
                .Include(a => a.AppointmentDetails)
                    .ThenInclude(ad => ad.service)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a =>
                    (a.customer != null && a.customer.full_name.ToLower().Contains(term)) ||
                    (a.pet != null && a.pet.name.ToLower().Contains(term)) ||
                    (a.doctor != null && a.doctor.full_name.ToLower().Contains(term)) ||
                    (a.notes ?? "").ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(a => a.status == statusFilter);
            }

            query = query.OrderByDescending(a => a.appointment_date);

            return await query.ToPagedResultAsync(page, pageSize);
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.customer)
                .Include(a => a.pet)
                .Include(a => a.doctor)
                .Include(a => a.AppointmentDetails)
                    .ThenInclude(ad => ad.service)
                .FirstOrDefaultAsync(a => a.appointment_id == id);
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return false;
            appt.status = "Approved";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id, string? reason)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return false;
            appt.status = "Rejected";
            if (!string.IsNullOrWhiteSpace(reason))
            {
                appt.notes = (appt.notes ?? "") + " [Từ chối: " + reason + "]";
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignDoctorAsync(int appointmentId, int doctorId)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt == null) return false;
            appt.doctor_id = doctorId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetDoctorsAsync()
        {
            return await _context.Users
                .Where(u => u.role_id == 3)
                .OrderBy(u => u.full_name)
                .ToListAsync();
        }
    }
}
