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

                // Tính thời gian chờ đến 00:05 ngày mai rồi mới quét lại
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1).AddMinutes(5); // 00:05 ngày mai
                var delay = nextRun - now;
                _logger.LogInformation($"Next auto-cancel scan scheduled at {nextRun:dd/MM/yyyy HH:mm}. Waiting {delay.TotalHours:F1} hours.");
                await Task.Delay(delay, stoppingToken);
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
