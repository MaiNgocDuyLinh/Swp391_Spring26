using Group3_SWP391_PetMedical.Data;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Feedback;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly PetClinicContext _context;

        public FeedbackRepository(PetClinicContext context)
        {
            _context = context;
        }

        public async Task<CusCreateFeedbackVM?> GetCusCreateFeedbackAsync(int customerId, int appointmentId)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId && a.appointment_id == appointmentId)
                .Select(a => new CusCreateFeedbackVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    PetName = a.pet.name,
                    DoctorName = (a.doctor_id != null && a.doctor != null && a.doctor.role_id == 3)
                        ? a.doctor.full_name
                        : "Chưa phân công",
                    ServiceNames = string.Join(", ", a.AppointmentDetails.Select(d => d.service.service_name))
                });

            return await q.FirstOrDefaultAsync();
        }

        public async Task<bool> HasFeedbackAsync(int customerId, int appointmentId)
        {
            return await _context.Feedbacks
                .AsNoTracking()
                .AnyAsync(f => f.customer_id == customerId && f.appointment_id == appointmentId);
        }

        public async Task<int> CreateFeedbackAsync(int customerId, CusCreateFeedbackVM vm)
        {
            var appointment = await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.customer_id == customerId && a.appointment_id == vm.AppointmentId);

            if (appointment == null)
                throw new Exception("Không tìm thấy lịch hẹn.");

            if ((appointment.status ?? "").Trim().ToLower() != "đã thanh toán")
                throw new Exception("Chỉ được feedback lịch hẹn có trạng thái 'Đã thanh toán'.");

            var existed = await _context.Feedbacks
                .AsNoTracking()
                .AnyAsync(f => f.customer_id == customerId && f.appointment_id == vm.AppointmentId);

            if (existed)
                throw new Exception("Bạn đã feedback lịch hẹn này rồi.");

            var feedback = new Feedback
            {
                appointment_id = vm.AppointmentId,
                customer_id = customerId,
                rating = vm.Rating,
                comment = vm.Comment,
                created_at = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return feedback.feedback_id;
        }
    }
}