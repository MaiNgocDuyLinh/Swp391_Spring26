using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Appointment;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface ICusAppointmentService
    {
        Task<PagedResult<CusAppointmentHistoryItemVM>>
            GetCusAppointmentHistoryAsync(int customerId, CusAppointmentHistoryQuery query);

        // lịch đã đặt
        Task<PagedResult<CusBookedAppointmentItemVM>>
            GetCusBookedAppointmentsAsync(int customerId, CusBookedAppointmentQuery query);

        // Lấy pet để tạo lịch hẹn 
        Task<List<(int PetId, string PetName)>> GetCustomerPetsAsync(int customerId);

        // tạo lịch hẹn 
        Task<int> CreateAppointmentAsync(int customerId, CusCreateAppointmentCommand cmd);

        // doctors
        Task<List<(int DoctorId, string DoctorName)>> GetDoctorsAsync();

        // ✅ shifts: overload 1 ngày (mới thêm)
        Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime day);

        // ✅ shifts: overload from-to (bạn đang dùng ở controller)
        Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime from, DateTime to);

        Task<bool> IsDoctorWorkingAtAsync(int doctorId, DateTime appointmentDateTime);



        Task<CusAppointmentDetailVM?> GetCusAppointmentDetailAsync(int customerId, int appointmentId);
        Task<CusEditAppointmentVM?> GetCusEditAppointmentAsync(int customerId, int appointmentId);
        Task<bool> UpdateCusAppointmentAsync(int customerId, CusEditAppointmentVM vm);
        Task<bool> CancelCusAppointmentAsync(int customerId, int appointmentId, string reason);
    }
}