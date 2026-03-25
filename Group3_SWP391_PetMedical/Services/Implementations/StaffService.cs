using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Appointment;
using Group3_SWP391_PetMedical.ViewModels.Staff;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly IServiceRepository _serviceRepo;
        private readonly IUserRepository _userRepo;
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IPetRepository _petRepo;
        private readonly ICusAppointmentRepository _cusAppointmentRepo;

        public StaffService(
            IServiceRepository serviceRepo,
            IUserRepository userRepo,
            IAppointmentRepository appointmentRepo,
            IPetRepository petRepo,
            ICusAppointmentRepository cusAppointmentRepo)
        {
            _serviceRepo = serviceRepo;
            _userRepo = userRepo;
            _appointmentRepo = appointmentRepo;
            _petRepo = petRepo;
            _cusAppointmentRepo = cusAppointmentRepo;
        }

        // ========== Services (view only) ==========
        public Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize)
            => _serviceRepo.GetPagedAsync(search, page, pageSize);

        // ========== Customers ==========
        public Task<PagedResult<User>> GetCustomersPagedAsync(string? search, int page, int pageSize)
            => _userRepo.GetCustomersPagedAsync(search, page, pageSize);

        public Task<User?> GetCustomerDetailAsync(int customerId)
            => _userRepo.GetCustomerDetailAsync(customerId);

        // ========== Appointments ==========
        public Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize)
            => _appointmentRepo.GetAppointmentsByDatePagedAsync(date, search, page, pageSize);

        public Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize)
            => _appointmentRepo.GetAllAppointmentsPagedAsync(search, statusFilter, page, pageSize);

        public Task<PagedResult<Appointment>> GetCancelledAppointmentsPagedAsync(string? search, int page, int pageSize)
            => _appointmentRepo.GetCancelledAppointmentsPagedAsync(search, page, pageSize);

        public Task<Appointment?> GetAppointmentByIdAsync(int id)
            => _appointmentRepo.GetByIdAsync(id);

        public Task<bool> CancelAppointmentAsync(int id, string? reason)
            => _appointmentRepo.CancelAsync(id, reason);

        public Task<bool> AssignDoctorAsync(int appointmentId, int doctorId)
            => _appointmentRepo.AssignDoctorAsync(appointmentId, doctorId);

        public Task<List<User>> GetDoctorsAsync()
            => _appointmentRepo.GetDoctorsAsync();

        public async Task<(bool Success, bool IsEarlyArrival)> UpdateAppointmentStatusAsync(int id, string newStatus)
        {
            var appt = await _appointmentRepo.GetByIdAsync(id);
            if (appt == null) return (false, false);

            bool isEarly = (newStatus == "Đã đến" && appt.appointment_date.Date > DateTime.Today);
            bool success = await _appointmentRepo.UpdateStatusAsync(id, newStatus, isEarly);
            return (success, isEarly);
        }

        // ========== Invoice ==========
        public Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId)
            => _appointmentRepo.GetInvoiceByAppointmentIdAsync(appointmentId);

        public Task<bool> CreateInvoiceAsync(int appointmentId)
            => _appointmentRepo.CreateInvoiceAsync(appointmentId);

        public async Task<int> CreateGuestBookingAsync(Group3_SWP391_PetMedical.ViewModels.Staff.StaffCreateGuestBookingVM model)
        {
            // 1. Get or Create Customer
            var customer = await _userRepo.GetByPhoneAsync(model.CustomerPhone);
            if (customer == null)
            {
                // Kiểm tra username trước khi tạo mới để tránh lỗi Unique Constraint
                var existsUsername = await _userRepo.ExistsUsernameAsync(model.CustomerPhone);
                if (existsUsername)
                {
                    customer = await _userRepo.GetByUsernameWithRoleAsync(model.CustomerPhone);
                }
                else
                {
                    var customerRole = await _userRepo.GetDefaultRoleAsync();
                    customer = new User
                    {
                        // Phương án 2: Tự tạo mã định danh ẩn cho Username & Password
                        username = "GUEST_" + model.CustomerPhone,
                        phone = model.CustomerPhone,
                        full_name = model.CustomerName,
                        email = model.CustomerEmail ?? "", 
                        password = Guid.NewGuid().ToString("N"), // Password ngẫu nhiên, không dùng để login
                        status = "Unactive",
                        role_id = customerRole?.role_id ?? 1,
                        created_at = DateTime.Now
                    };
                    await _userRepo.AddAsync(customer);
                }
            }
            else
            {
                // Cập nhật thông tin nếu có thay đổi từ form
                bool changed = false;
                
                // Cập nhật email nếu có nhập và khác cũ, và chưa ai dùng
                if (!string.IsNullOrWhiteSpace(model.CustomerEmail) && customer.email != model.CustomerEmail)
                {
                    bool isEmailTaken = await _userRepo.ExistsEmailByOtherUserAsync(model.CustomerEmail.Trim(), customer.user_id);
                    if (!isEmailTaken)
                    {
                        customer.email = model.CustomerEmail.Trim();
                        changed = true;
                    }
                }
                
                // Cập nhật tên nếu khác cũ
                if (!string.IsNullOrWhiteSpace(model.CustomerName) && customer.full_name != model.CustomerName)
                {
                    customer.full_name = model.CustomerName.Trim();
                    changed = true;
                }

                if (changed) await _userRepo.UpdateAsync(customer);
            }

            if (model.PetBirthdate.HasValue && model.PetBirthdate.Value > DateTime.Now)
                throw new Exception("Ngày sinh thú cưng không được lớn hơn hiện tại.");

            // 2. Get or Create Pet
            var pet = await _petRepo.GetByDetailsAndOwnerAsync(
                model.PetName, 
                model.PetSpecies, 
                model.PetBreed ?? "", 
                model.PetGender, 
                model.PetBirthdate, 
                customer.user_id);
            if (pet == null)
            {
                pet = new Pet
                {
                    owner_id = customer.user_id,
                    name = model.PetName,
                    species = model.PetSpecies,
                    breed = model.PetBreed,
                    pet_gender = model.PetGender,
                    weight = model.PetWeight,
                    pet_birthdate = model.PetBirthdate,
                    created_at = DateTime.Now
                };
                await _petRepo.AddAsync(pet);
            }

            // 3. Create Appointment using the same repository as Customer (to ensure consistency)
            var cmd = new CusCreateAppointmentCommand
            {
                PetId = pet.pet_id,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                Shift = model.Shift,
                ServiceIds = model.ServiceIds, // Bổ sung mapping danh sách dịch vụ
                Notes = model.Notes,
                IgnoreDoctorShiftCheck = true 
            };

            var appointmentId = await _cusAppointmentRepo.CreateAppointmentAsync(customer.user_id, cmd);
            
            // Tự động chuyển trạng thái thành "Đã đến" vì đây là khách vãng lai đặt tại chỗ
            await _appointmentRepo.UpdateStatusAsync(appointmentId, "Đã đến");

            return appointmentId;
        }

        public Task<User?> GetCustomerByPhoneAsync(string phone)
            => _userRepo.GetByPhoneAsync(phone);
    }
}
