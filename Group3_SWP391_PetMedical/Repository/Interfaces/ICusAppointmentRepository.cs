using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Appointment;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface ICusAppointmentRepository
    {
        Task<PagedResult<CusAppointmentHistoryItemVM>>
            GetCusAppointmentHistoryAsync(int customerId, CusAppointmentHistoryQuery query);

        // Lịch đã đặt 
        Task<PagedResult<CusBookedAppointmentItemVM>>
            GetCusBookedAppointmentsAsync(int customerId, CusBookedAppointmentQuery query);

        // lấy pets của customer để đổ dropdown
        Task<List<(int PetId, string PetName)>> GetCustomerPetsAsync(int customerId);

        // tạo lịch hẹn
        Task<int> CreateAppointmentAsync(int customerId, CusCreateAppointmentCommand cmd);

        // doctors
        Task<List<(int DoctorId, string DoctorName)>> GetDoctorsAsync();

        // ✅ shifts: giữ bản 1 ngày (đang có)
        Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime day);

        // ✅ shifts: thêm overload (from,to) để controller/service dùng
        Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime from, DateTime to);

        Task<bool> IsDoctorWorkingAtAsync(int doctorId, DateTime appointmentDateTime);
    }
}