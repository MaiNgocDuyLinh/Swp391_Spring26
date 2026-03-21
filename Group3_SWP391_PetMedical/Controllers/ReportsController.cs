using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Staff,Manager")]
    public class ReportsController : Controller
    {
        private readonly PetClinicContext _context;

        public ReportsController(PetClinicContext context)
        {
            _context = context;
        }

        // Trang cấu hình & xem trước dữ liệu export
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? exportType)
        {
            // Nếu không chọn khoảng thời gian thì mặc định 7 ngày gần nhất
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                toDate = DateTime.Today;
                fromDate = DateTime.Today.AddDays(-7);
            }

            var model = new ReportExportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                ExportType = string.IsNullOrWhiteSpace(exportType) ? "Invoices" : exportType
            };

            switch (model.ExportType)
            {
                case "Prescriptions":
                    {
                        var query = _context.Prescriptions
                            .Include(p => p.medicine)
                            .Include(p => p.record)
                                .ThenInclude(r => r.appointment)
                                    .ThenInclude(a => a.customer)
                            .Include(p => p.record)
                                .ThenInclude(r => r.appointment)
                                    .ThenInclude(a => a.pet)
                            .AsQueryable();

                        if (fromDate.HasValue)
                        {
                            query = query.Where(p => p.record.created_at >= fromDate.Value);
                        }

                        if (toDate.HasValue)
                        {
                            query = query.Where(p => p.record.created_at <= toDate.Value);
                        }

                        model.Prescriptions = await query
                            .OrderByDescending(p => p.record.created_at)
                            .ToListAsync();
                        break;
                    }

                case "Statistics":
                    {
                        var invoiceQuery = _context.Invoices.AsQueryable();
                        var appointmentQuery = _context.Appointments.AsQueryable();

                        if (fromDate.HasValue)
                        {
                            invoiceQuery = invoiceQuery.Where(i => i.created_at >= fromDate.Value);
                            appointmentQuery = appointmentQuery.Where(a => a.appointment_date >= fromDate.Value);
                        }

                        if (toDate.HasValue)
                        {
                            invoiceQuery = invoiceQuery.Where(i => i.created_at <= toDate.Value);
                            appointmentQuery = appointmentQuery.Where(a => a.appointment_date <= toDate.Value);
                        }

                        var invoices = await invoiceQuery.ToListAsync();
                        var appointments = await appointmentQuery.ToListAsync();

                        var revenueByDate = invoices
                            .Where(i => i.created_at.HasValue)
                            .GroupBy(i => i.created_at!.Value.Date)
                            .Select(g => new
                            {
                                Date = g.Key,
                                TotalRevenue = g.Sum(x => x.total_amount)
                            })
                            .ToDictionary(x => x.Date, x => x.TotalRevenue);

                        var appointmentsByDate = appointments
                            .GroupBy(a => a.appointment_date.Date)
                            .Select(g => new
                            {
                                Date = g.Key,
                                Count = g.Count()
                            })
                            .ToDictionary(x => x.Date, x => x.Count);

                        var allDates = revenueByDate.Keys
                            .Union(appointmentsByDate.Keys)
                            .OrderBy(d => d)
                            .ToList();

                        foreach (var date in allDates)
                        {
                            revenueByDate.TryGetValue(date, out var revenue);
                            appointmentsByDate.TryGetValue(date, out var appointmentCount);

                            model.Statistics.Add(new ReportStatisticsRow
                            {
                                Date = date,
                                TotalRevenue = revenue,
                                AppointmentCount = appointmentCount
                            });
                        }

                        break;
                    }

                default:
                    {
                        var query = _context.Invoices
                            .Include(i => i.appointment)
                                .ThenInclude(a => a.customer)
                            .Include(i => i.appointment)
                                .ThenInclude(a => a.pet)
                            .AsQueryable();

                        if (fromDate.HasValue)
                        {
                            query = query.Where(i => i.created_at >= fromDate.Value);
                        }

                        if (toDate.HasValue)
                        {
                            query = query.Where(i => i.created_at <= toDate.Value);
                        }

                        model.Invoices = await query
                            .OrderByDescending(i => i.created_at)
                            .ToListAsync();
                        break;
                    }
            }

            return View(model);
        }

        // ========== 1. EXPORT HÓA ĐƠN (CSV) ==========
        [HttpGet]
        public async Task<IActionResult> ExportInvoices(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Invoices
                .Include(i => i.appointment)
                    .ThenInclude(a => a.customer)
                .Include(i => i.appointment)
                    .ThenInclude(a => a.pet)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.created_at >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.created_at <= toDate.Value);
            }

            var invoices = await query
                .OrderByDescending(i => i.created_at)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("InvoiceId,AppointmentId,CustomerName,PetName,TotalAmount,PaymentStatus,PaymentMethod,CreatedAt");

            foreach (var invoice in invoices)
            {
                var appointment = invoice.appointment;
                var customerName = appointment?.customer?.full_name;
                var petName = appointment?.pet?.name;
                var createdAt = invoice.created_at?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;

                var line = string.Join(",", new[]
                {
                    invoice.invoice_id.ToString(CultureInfo.InvariantCulture),
                    invoice.appointment_id.ToString(CultureInfo.InvariantCulture),
                    EscapeCsv(customerName),
                    EscapeCsv(petName),
                    invoice.total_amount.ToString(CultureInfo.InvariantCulture),
                    EscapeCsv(invoice.payment_status),
                    EscapeCsv(invoice.payment_method),
                    EscapeCsv(createdAt)
                });

                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Invoices_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // ========== 2. EXPORT ĐƠN THUỐC (CSV) ==========
        [HttpGet]
        public async Task<IActionResult> ExportPrescriptions(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Prescriptions
                .Include(p => p.medicine)
                .Include(p => p.record)
                    .ThenInclude(r => r.appointment)
                        .ThenInclude(a => a.customer)
                .Include(p => p.record)
                    .ThenInclude(r => r.appointment)
                        .ThenInclude(a => a.pet)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.record.created_at >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.record.created_at <= toDate.Value);
            }

            var prescriptions = await query
                .OrderByDescending(p => p.record.created_at)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("PrescriptionId,RecordId,AppointmentDate,CustomerName,PetName,MedicineName,Dosage,Quantity,RecordCreatedAt");

            foreach (var prescription in prescriptions)
            {
                var record = prescription.record;
                var appointment = record.appointment;
                var customerName = appointment?.customer?.full_name;
                var petName = appointment?.pet?.name;
                var appointmentDate = appointment?.appointment_date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
                var recordCreatedAt = record.created_at?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;

                var line = string.Join(",", new[]
                {
                    prescription.prescription_id.ToString(CultureInfo.InvariantCulture),
                    prescription.record_id.ToString(CultureInfo.InvariantCulture),
                    EscapeCsv(appointmentDate),
                    EscapeCsv(customerName),
                    EscapeCsv(petName),
                    EscapeCsv(prescription.medicine.name),
                    EscapeCsv(prescription.dosage),
                    prescription.quantity.ToString(CultureInfo.InvariantCulture),
                    EscapeCsv(recordCreatedAt)
                });

                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Prescriptions_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // ========== 3. EXPORT THỐNG KÊ (CSV) ==========
        [HttpGet]
        public async Task<IActionResult> ExportStatistics(DateTime? fromDate, DateTime? toDate)
        {
            var invoiceQuery = _context.Invoices.AsQueryable();
            var appointmentQuery = _context.Appointments.AsQueryable();

            if (fromDate.HasValue)
            {
                invoiceQuery = invoiceQuery.Where(i => i.created_at >= fromDate.Value);
                appointmentQuery = appointmentQuery.Where(a => a.appointment_date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                invoiceQuery = invoiceQuery.Where(i => i.created_at <= toDate.Value);
                appointmentQuery = appointmentQuery.Where(a => a.appointment_date <= toDate.Value);
            }

            var invoices = await invoiceQuery.ToListAsync();
            var appointments = await appointmentQuery.ToListAsync();

            var revenueByDate = invoices
                .Where(i => i.created_at.HasValue)
                .GroupBy(i => i.created_at!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(x => x.total_amount)
                })
                .ToDictionary(x => x.Date, x => x.TotalRevenue);

            var appointmentsByDate = appointments
                .GroupBy(a => a.appointment_date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.Date, x => x.Count);

            var allDates = revenueByDate.Keys
                .Union(appointmentsByDate.Keys)
                .OrderBy(d => d)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Date,TotalRevenue,AppointmentCount");

            foreach (var date in allDates)
            {
                revenueByDate.TryGetValue(date, out var revenue);
                appointmentsByDate.TryGetValue(date, out var appointmentCount);

                var line = string.Join(",", new[]
                {
                    date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    revenue.ToString(CultureInfo.InvariantCulture),
                    appointmentCount.ToString(CultureInfo.InvariantCulture)
                });

                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Statistics_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // ========== CSV HELPER ==========
        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');

            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }

            return needsQuotes ? $"\"{value}\"" : value;
        }
    }
}