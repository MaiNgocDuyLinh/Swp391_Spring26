using Group3_SWP391_PetMedical.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class AutoCancelAppointmentService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoCancelAppointmentService> _logger;

        public AutoCancelAppointmentService(IServiceProvider serviceProvider, ILogger<AutoCancelAppointmentService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutoCancelAppointmentService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CancelOverdueAppointmentsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing AutoCancelAppointmentService.");
                }

                // Chờ 1 giờ trước khi kiểm tra lại (có thể chỉnh thành 1 phút để test)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("AutoCancelAppointmentService is stopping.");
        }

        private async Task CancelOverdueAppointmentsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PetClinicContext>();
                var today = DateTime.Today;

                var overdueAppointments = await context.Appointments
                    .Where(a => a.appointment_date.Date < today && a.status == "Đặt lịch thành công")
                    .ToListAsync();

                if (overdueAppointments.Any())
                {
                    foreach (var appointment in overdueAppointments)
                    {
                        appointment.status = "Đã hủy";
                        appointment.notes = (appointment.notes ?? "") + "\n[Hệ thống]: Tự động hủy do quá hạn ngày đặt khám nhưng khách không đến.";
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Auto canceled {overdueAppointments.Count} overdue appointments.");
                }
            }
        }
    }
}
