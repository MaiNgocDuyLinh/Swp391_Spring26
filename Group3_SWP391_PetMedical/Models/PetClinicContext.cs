using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using System.Text.Json;

namespace Group3_SWP391_PetMedical.Models;

public partial class PetClinicContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PetClinicContext(
        DbContextOptions<PetClinicContext> options,
        IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ================= DBSETS =================

    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<AppointmentDetail> AppointmentDetails { get; set; }
    public virtual DbSet<Feedback> Feedback { get; set; }
    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }
    public virtual DbSet<Medication> Medications { get; set; }
    public virtual DbSet<Pet> Pets { get; set; }
    public virtual DbSet<Prescription> Prescriptions { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Schedule> Schedules { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<User> Users { get; set; }

    // ✅ NEW TABLE
    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    // ================= AUTO AUDIT =================

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        var currentUser = httpContext?.User;

        var userIdClaim = currentUser?.FindFirst("user_id")?.Value;
        var userEmail = currentUser?.Identity?.Name;
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();

        var auditLogs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog ||
                entry.State == EntityState.Detached ||
                entry.State == EntityState.Unchanged)
                continue;

            var keyProperty = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey());

            if (keyProperty == null)
                continue;

            var audit = new AuditLog
            {
                EntityName = entry.Metadata.GetTableName(),
                EntityId = keyProperty.CurrentValue?.ToString(),
                Action = entry.State.ToString(),
                CreatedAt = DateTime.Now,
                UserEmail = userEmail,
                IpAddress = ipAddress
            };

            if (int.TryParse(userIdClaim, out int parsedUserId))
                audit.UserId = parsedUserId;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey()) continue;
                if (property.Metadata.IsShadowProperty()) continue;

                if (property.Metadata.ClrType.IsClass &&
                    property.Metadata.ClrType != typeof(string))
                    continue;

                if (property.Metadata.Name.ToLower().Contains("password"))
                    continue;

                if (entry.State == EntityState.Added)
                {
                    newValues[property.Metadata.Name] = property.CurrentValue;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (!Equals(property.OriginalValue, property.CurrentValue))
                    {
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                        newValues[property.Metadata.Name] = property.CurrentValue;
                    }
                }
            }

            if (oldValues.Any())
                audit.OldValues = JsonSerializer.Serialize(oldValues);

            if (newValues.Any())
                audit.NewValues = JsonSerializer.Serialize(newValues);

            if (audit.OldValues == null && audit.NewValues == null)
                continue;

            auditLogs.Add(audit);
        }

        if (auditLogs.Any())
            AuditLogs.AddRange(auditLogs);

        return await base.SaveChangesAsync(cancellationToken);
    }

    // ================= MODEL CONFIG =================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ⚠️ GIỮ NGUYÊN TOÀN BỘ SCAFFOLD CŨ TRONG FILE PARTIAL
        OnModelCreatingPartial(modelBuilder);

        // ================= PRIMARY KEYS =================

        modelBuilder.Entity<Appointment>().HasKey(a => a.appointment_id);
        modelBuilder.Entity<AppointmentDetail>().HasKey(ad => new { ad.appointment_id, ad.service_id });
        modelBuilder.Entity<Feedback>().HasKey(f => f.feedback_id);
        modelBuilder.Entity<Invoice>().HasKey(i => i.invoice_id);
        modelBuilder.Entity<MedicalRecord>().HasKey(m => m.record_id);
        modelBuilder.Entity<Medication>().HasKey(m => m.medicine_id);
        modelBuilder.Entity<Pet>().HasKey(p => p.pet_id);
        modelBuilder.Entity<Prescription>().HasKey(p => p.prescription_id);
        modelBuilder.Entity<Role>().HasKey(r => r.role_id);
        modelBuilder.Entity<Schedule>().HasKey(s => s.schedule_id);
        modelBuilder.Entity<Service>().HasKey(s => s.service_id);
        modelBuilder.Entity<User>().HasKey(u => u.user_id);
        modelBuilder.Entity<AuditLog>().HasKey(a => a.AuditLogId);

        // ================= AUDIT LOG CONFIG =================

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
        });

        // ================= RELATIONSHIPS CONFIG =================

        // Appointment ↔ User (customer / doctor)
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasOne(a => a.customer)
                  .WithMany(u => u.Appointmentcustomers)
                  .HasForeignKey(a => a.customer_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(a => a.doctor)
                  .WithMany(u => u.Appointmentdoctors)
                  .HasForeignKey(a => a.doctor_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            // Appointment ↔ Pet (many appointments per pet)
            entity.HasOne(a => a.pet)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.pet_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            // Appointment ↔ Invoice (1:1)
            entity.HasOne(a => a.Invoice)
                  .WithOne(i => i.appointment)
                  .HasForeignKey<Invoice>(i => i.appointment_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            // Appointment ↔ MedicalRecord (1:1)
            entity.HasOne(a => a.MedicalRecord)
                  .WithOne(m => m.appointment)
                  .HasForeignKey<MedicalRecord>(m => m.appointment_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // Pet ↔ User (owner)
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasOne(p => p.owner)
                  .WithMany(u => u.Pets)
                  .HasForeignKey(p => p.owner_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // User ↔ Role
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.role_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // Schedule ↔ User (doctor)
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasOne(s => s.doctor)
                  .WithMany(u => u.Schedules)
                  .HasForeignKey(s => s.doctor_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // AppointmentDetail ↔ Appointment / Service
        modelBuilder.Entity<AppointmentDetail>(entity =>
        {
            entity.HasOne(ad => ad.appointment)
                  .WithMany(a => a.AppointmentDetails)
                  .HasForeignKey(ad => ad.appointment_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(ad => ad.service)
                  .WithMany(s => s.AppointmentDetails)
                  .HasForeignKey(ad => ad.service_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // Feedback ↔ User / Appointment
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasOne(f => f.customer)
                  .WithMany(u => u.Feedbacks)
                  .HasForeignKey(f => f.customer_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(f => f.appointment)
                  .WithMany(a => a.Feedbacks)
                  .HasForeignKey(f => f.appointment_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // Prescription ↔ MedicalRecord / Medication
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasOne(p => p.record)
                  .WithMany(mr => mr.Prescriptions)
                  .HasForeignKey(p => p.record_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(p => p.medicine)
                  .WithMany(m => m.Prescriptions)
                  .HasForeignKey(p => p.medicine_id)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}