using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Appointment;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class CusAppointmentService : ICusAppointmentService
    {
        private readonly ICusAppointmentRepository _repo;

        public CusAppointmentService(ICusAppointmentRepository repo)
        {
            _repo = repo;
        }

        public Task<PagedResult<CusAppointmentHistoryItemVM>>
            GetCusAppointmentHistoryAsync(int customerId, CusAppointmentHistoryQuery query)
            => _repo.GetCusAppointmentHistoryAsync(customerId, query);

        public Task<PagedResult<CusBookedAppointmentItemVM>>
            GetCusBookedAppointmentsAsync(int customerId, CusBookedAppointmentQuery query)
            => _repo.GetCusBookedAppointmentsAsync(customerId, query);

        public Task<List<(int PetId, string PetName)>> GetCustomerPetsAsync(int customerId)
            => _repo.GetCustomerPetsAsync(customerId);

        //validate đặt lịch (không sửa logic cũ, chỉ thêm trước khi gọi repo)
        public async Task<int> CreateAppointmentAsync(int customerId, CusCreateAppointmentCommand cmd)
        {
            if (cmd == null) throw new Exception("Dữ liệu đặt lịch không hợp lệ.");

            if (cmd.AppointmentDate == default)
                throw new Exception("Vui lòng chọn ngày khám.");

            if (string.IsNullOrWhiteSpace(cmd.Shift))
                throw new Exception("Vui lòng chọn ca khám.");

            if (cmd.PetId <= 0)
                throw new Exception("Vui lòng chọn thú cưng.");

            if (cmd.ServiceIds == null || cmd.ServiceIds.Count == 0)
                throw new Exception("Vui lòng chọn ít nhất 1 dịch vụ.");

            var appointmentDateTime = BuildAppointmentDateTime(cmd.AppointmentDate, cmd.Shift);

            var now = DateTime.Now;
            var nowTrim = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            if (appointmentDateTime < nowTrim)
                throw new Exception("Không được chọn ca khám ở quá khứ.");

            // map lại về datetime đầu ca trước khi xuống repo
            cmd.AppointmentDate = appointmentDateTime;
            cmd.Shift = NormalizeShiftKey(cmd.Shift);

            return await _repo.CreateAppointmentAsync(customerId, cmd);
        }

        private static string NormalizeShiftKey(string? shift)
        {
            var key = (shift ?? "").Trim().ToLowerInvariant();

            return key switch
            {
                "sáng" or "sang" => "sáng",
                "chiều" or "chieu" => "chiều",
                _ => ""
            };
        }

        private static DateTime BuildAppointmentDateTime(DateTime appointmentDate, string? shift)
        {
            var date = appointmentDate.Date;

            return NormalizeShiftKey(shift) switch
            {
                "sáng" => date.AddHours(8),
                "chiều" => date.AddHours(13),
                _ => throw new Exception("Vui lòng chọn ca khám hợp lệ.")
            };
        }

        public Task<List<(int DoctorId, string DoctorName)>> GetDoctorsAsync()
            => _repo.GetDoctorsAsync();

        public Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime day)
            => _repo.GetDoctorShiftsAsync(doctorId, day.Date);

        public Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime from, DateTime to)
            => _repo.GetDoctorShiftsAsync(doctorId, from.Date, to.Date);

        public Task<List<DoctorAppointmentEventVM>> GetDoctorAppointmentsAsync(int doctorId, DateTime from, DateTime to)
            => _repo.GetDoctorAppointmentsAsync(doctorId, from, to);

        public Task<bool> IsDoctorWorkingAtAsync(int doctorId, DateTime appointmentDateTime)
            => _repo.IsDoctorWorkingAtAsync(doctorId, appointmentDateTime);

        public Task<CusAppointmentDetailVM?> GetCusAppointmentDetailAsync(int customerId, int appointmentId)
            => _repo.GetCusAppointmentDetailAsync(customerId, appointmentId);

        public Task<CusEditAppointmentVM?> GetCusEditAppointmentAsync(int customerId, int appointmentId)
            => _repo.GetCusEditAppointmentAsync(customerId, appointmentId);

        public Task<bool> UpdateCusAppointmentAsync(int customerId, CusEditAppointmentVM vm)
            => _repo.UpdateCusAppointmentAsync(customerId, vm);

        //  Cancel popup data
        public Task<CusCancelAppointmentVM?> GetCusCancelAppointmentAsync(int customerId, int appointmentId)
            => _repo.GetCusCancelAppointmentAsync(customerId, appointmentId);

        public Task<bool> CancelCusAppointmentAsync(int customerId, int appointmentId, string reason)
            => _repo.CancelCusAppointmentAsync(customerId, appointmentId, reason);


        
    }
}