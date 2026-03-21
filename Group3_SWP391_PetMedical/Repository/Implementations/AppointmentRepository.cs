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

        // ========== Helper: Include navigation properties ==========
        private IQueryable<Appointment> BaseQuery() =>
            _context.Appointments
                .AsNoTracking()
                .Include(a => a.customer)
                .Include(a => a.pet)
                .Include(a => a.doctor)
                .Include(a => a.AppointmentDetails)
                    .ThenInclude(ad => ad.service);

        // ========== Staff: Xem lich theo ngay ==========
        public async Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(
            DateTime date, string? search, int page, int pageSize)
        {
            var query = BaseQuery().Where(a => a.appointment_date.Date == date.Date);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(a =>
                    (a.customer != null && EF.Functions.Collate(a.customer.full_name, "Vietnamese_CI_AI").Contains(search)) ||
                    (a.pet != null && EF.Functions.Collate(a.pet.name, "Vietnamese_CI_AI").Contains(search)) ||
                    (a.doctor != null && EF.Functions.Collate(a.doctor.full_name, "Vietnamese_CI_AI").Contains(search)) ||
                    EF.Functions.Collate(a.notes ?? "", "Vietnamese_CI_AI").Contains(search));
            }

            query = query.OrderBy(a => a.appointment_date);
            return await query.ToPagedResultAsync(page, pageSize);
        }

        // ========== Manager + Staff: Xem tat ca lich (filter status) ==========
        public async Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(
            string? search, string? statusFilter, int page, int pageSize)
        {
            var query = BaseQuery();

            // Filter by Vietnamese status directly
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                var vietMap = new Dictionary<string, string> {
                    {"Pending", "Chờ xác nhận"}, {"Deposited", "Đã đặt cọc"},
                    {"Confirmed", "Đã xác nhận"}, {"Arrived", "Đã đến"},
                    {"Completed", "Hoàn thành"}, {"Cancelled", "Đã hủy"},
                    {"NoShow", "Không đến"}
                };
                var vietStatus = vietMap.ContainsKey(statusFilter) ? vietMap[statusFilter] : statusFilter;
                query = query.Where(a => a.status == statusFilter || a.status == vietStatus);
                query = query.Where(a => a.status == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(a =>
                    (a.customer != null && EF.Functions.Collate(a.customer.full_name, "Vietnamese_CI_AI").Contains(search)) ||
                    (a.pet != null && EF.Functions.Collate(a.pet.name, "Vietnamese_CI_AI").Contains(search)) ||
                    (a.doctor != null && EF.Functions.Collate(a.doctor.full_name, "Vietnamese_CI_AI").Contains(search)) ||
                    EF.Functions.Collate(a.notes ?? "", "Vietnamese_CI_AI").Contains(search));
            }

            query = query.OrderByDescending(a => a.appointment_date);
            return await query.ToPagedResultAsync(page, pageSize);
        }

        // ========== Cancelled history ==========
        public async Task<PagedResult<Appointment>> GetCancelledAppointmentsPagedAsync(
            string? search, int page, int pageSize)
        {
            var query = BaseQuery().Where(a => a.status == "Đã Hủy");

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(a =>
                    (a.customer != null && EF.Functions.Collate(a.customer.full_name, "Vietnamese_CI_AI").Contains(search)) ||
                    (a.pet != null && EF.Functions.Collate(a.pet.name, "Vietnamese_CI_AI").Contains(search)) ||
                    (a.doctor != null && EF.Functions.Collate(a.doctor.full_name, "Vietnamese_CI_AI").Contains(search)) ||
                    EF.Functions.Collate(a.notes ?? "", "Vietnamese_CI_AI").Contains(search));
            }

            query = query.OrderByDescending(a => a.appointment_date);
            return await query.ToPagedResultAsync(page, pageSize);
        }

        // ========== Get chi tiet 1 lich ==========
        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await BaseQuery()
                .Include(a => a.MedicalRecord)
                    .ThenInclude(mr => mr.Prescriptions)
                        .ThenInclude(p => p.medicine)
                .FirstOrDefaultAsync(a => a.appointment_id == id);
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return false;
            appt.status = "Confirmed";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id, string? reason)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return false;
            appt.status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }
        // ========== Staff: Cancel appointment ==========
        public async Task<bool> CancelAsync(int id, string? reason)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return false;
            appt.status = "Đã Hủy";
            if (!string.IsNullOrWhiteSpace(reason))
                appt.notes = (appt.notes ?? "") + "\n[Staff hủy]: " + reason;
            await _context.SaveChangesAsync();
            return true;
        }

        // ========== Staff: Assign Doctor ==========
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
            var doctorRole = await _context.Roles.FirstOrDefaultAsync(r => r.role_name == "Doctor");
            if (doctorRole == null) return new List<User>();

            return await _context.Users
                .AsNoTracking()
                .Where(u => u.role_id == doctorRole.role_id && u.status == "Active")
                .OrderBy(u => u.full_name)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return false;
            appt.status = newStatus;
            
            // Sync related invoice if newly paid
            if (newStatus == "Đã thanh toán")
            {
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.appointment_id == id);
                if (invoice != null)
                {
                    invoice.payment_status = "Paid";
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.appointment)
                    .ThenInclude(a => a.AppointmentDetails)
                        .ThenInclude(ad => ad.service)
                .Include(i => i.appointment)
                    .ThenInclude(a => a.customer)
                .Include(i => i.appointment)
                    .ThenInclude(a => a.pet)
                .Include(i => i.appointment)
                    .ThenInclude(a => a.MedicalRecord)
                        .ThenInclude(mr => mr.Prescriptions)
                            .ThenInclude(p => p.medicine)
                .FirstOrDefaultAsync(i => i.appointment_id == appointmentId);
        }

        public async Task<bool> CreateInvoiceAsync(int appointmentId)
        {
            var appt = await _context.Appointments
                .Include(a => a.AppointmentDetails)
                    .ThenInclude(ad => ad.service)
                .FirstOrDefaultAsync(a => a.appointment_id == appointmentId);

            if (appt == null) return false;

            var exists = await _context.Invoices.AnyAsync(i => i.appointment_id == appointmentId);
            if (exists) return true;

            var total = appt.AppointmentDetails.Sum(ad => ad.actual_price ?? ad.service?.base_price ?? 0);

            var invoice = new Invoice
            {
                appointment_id = appointmentId,
                total_amount = total,
                payment_status = "Unpaid",
                payment_method = "Cash",
                created_at = DateTime.Now
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
