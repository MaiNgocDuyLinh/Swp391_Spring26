using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Appointment;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface ICusAppointmentRepository
    {
        Task<PagedResult<CusAppointmentHistoryItemVM>>
            GetCusAppointmentHistoryAsync(int customerId, CusAppointmentHistoryQuery query);

        Task<PagedResult<CusBookedAppointmentItemVM>>
            GetCusBookedAppointmentsAsync(int customerId, CusBookedAppointmentQuery query);

        Task<List<(int PetId, string PetName)>> GetCustomerPetsAsync(int customerId);

        Task<int> CreateAppointmentAsync(int customerId, CusCreateAppointmentCommand cmd);

        Task<List<(int DoctorId, string DoctorName)>> GetDoctorsAsync();

        Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime day);

        Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime from, DateTime to);
        Task<List<DoctorAppointmentEventVM>>GetDoctorAppointmentsAsync(int doctorId, DateTime from, DateTime to);
        Task<bool> IsDoctorWorkingAtAsync(int doctorId, DateTime appointmentDateTime);

        Task<CusAppointmentDetailVM?> GetCusAppointmentDetailAsync(int customerId, int appointmentId);

        Task<CusEditAppointmentVM?> GetCusEditAppointmentAsync(int customerId, int appointmentId);

        Task<bool> UpdateCusAppointmentAsync(int customerId, CusEditAppointmentVM vm);

        //  GET Cancel popup data (tóm tắt: ngày giờ, thú cưng, dịch vụ, mô tả)
        Task<CusCancelAppointmentVM?> GetCusCancelAppointmentAsync(int customerId, int appointmentId);

        Task<bool> CancelCusAppointmentAsync(int customerId, int appointmentId, string reason);
    }
}