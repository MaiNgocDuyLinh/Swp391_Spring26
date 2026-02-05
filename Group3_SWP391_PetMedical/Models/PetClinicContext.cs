using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Models;

public partial class PetClinicContext : DbContext
{
    public PetClinicContext(DbContextOptions<PetClinicContext> options)
        : base(options)
    {

    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentDetail> AppointmentDetails { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.appointment_id).HasName("PK__Appointm__A50828FCFCE42674");

            entity.Property(e => e.appointment_date).HasColumnType("datetime");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.customer).WithMany(p => p.Appointmentcustomers)
                .HasForeignKey(d => d.customer_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appt_Customer");

            entity.HasOne(d => d.doctor).WithMany(p => p.Appointmentdoctors)
                .HasForeignKey(d => d.doctor_id)
                .HasConstraintName("FK_Appt_Doctor");

            entity.HasOne(d => d.pet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.pet_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appt_Pet");
        });

        modelBuilder.Entity<AppointmentDetail>(entity =>
        {
            entity.HasKey(e => new { e.appointment_id, e.service_id }).HasName("PK__Appointm__46E8F376979B5332");

            entity.Property(e => e.actual_price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.appointment).WithMany(p => p.AppointmentDetails)
                .HasForeignKey(d => d.appointment_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detail_Appt");

            entity.HasOne(d => d.service).WithMany(p => p.AppointmentDetails)
                .HasForeignKey(d => d.service_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detail_Service");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.feedback_id).HasName("PK__Feedback__7A6B2B8CA8196EC0");

            entity.ToTable("Feedback");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.appointment).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.appointment_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Feed_Appt");

            entity.HasOne(d => d.customer).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.customer_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Feed_Customer");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.invoice_id).HasName("PK__Invoices__F58DFD49BFEC8652");

            entity.HasIndex(e => e.appointment_id, "UQ__Invoices__A50828FD2F004936").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.payment_method).HasMaxLength(50);
            entity.Property(e => e.payment_status)
                .HasMaxLength(20)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.total_amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.vnpay_transaction_id)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.appointment).WithOne(p => p.Invoice)
                .HasForeignKey<Invoice>(d => d.appointment_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoice_Appt");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.record_id).HasName("PK__MedicalR__BFCFB4DD3E2494E1");

            entity.HasIndex(e => e.appointment_id, "UQ__MedicalR__A50828FDE35248DD").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.appointment).WithOne(p => p.MedicalRecord)
                .HasForeignKey<MedicalRecord>(d => d.appointment_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Record_Appt");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.medicine_id).HasName("PK__Medicati__E7148EBB0287C5BC");

            entity.Property(e => e.name).HasMaxLength(100);
            entity.Property(e => e.stock_quantity).HasDefaultValue(0);
            entity.Property(e => e.unit_price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.pet_id).HasName("PK__Pets__390CC5FEAA98A7C5");

            entity.Property(e => e.breed).HasMaxLength(50);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.name).HasMaxLength(100);
            entity.Property(e => e.species).HasMaxLength(50);

            // ảnh 
            entity.Property(e => e.PetImg).HasMaxLength(255).IsUnicode(false);

            // giới tính , tuổi tự tăng
            entity.Property(e => e.pet_gender).HasMaxLength(10);
            entity.Property(e => e.pet_birthdate).HasColumnType("date");


            entity.HasOne(d => d.owner).WithMany(p => p.Pets)
                .HasForeignKey(d => d.owner_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pet_Owner");


        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.prescription_id).HasName("PK__Prescrip__3EE444F8CDD7BFD2");

            entity.Property(e => e.dosage).HasMaxLength(255);

            entity.HasOne(d => d.medicine).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.medicine_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Presc_Med");

            entity.HasOne(d => d.record).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.record_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Presc_Record");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.role_id).HasName("PK__Roles__760965CC73AAC333");

            entity.HasIndex(e => e.role_name, "UQ__Roles__783254B181645AE4").IsUnique();

            entity.Property(e => e.role_name).HasMaxLength(50);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.schedule_id).HasName("PK__Schedule__C46A8A6F980197DF");

            entity.Property(e => e.shift).HasMaxLength(20);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.doctor).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.doctor_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Doctor");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.service_id).HasName("PK__Services__3E0DB8AFC4D0DAA3");

            entity.Property(e => e.base_price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.is_home_service).HasDefaultValue(false);
            entity.Property(e => e.service_name).HasMaxLength(100);
            entity.Property(e => e.status).HasDefaultValue(true);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PK__Users__B9BE370F776CFD5F");

            entity.HasIndex(e => e.email, "UQ__Users__AB6E616472465850").IsUnique();

            entity.Property(e => e.avatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.full_name).HasMaxLength(100);
            entity.Property(e => e.password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.verification_token)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.role).WithMany(p => p.Users)
                .HasForeignKey(d => d.role_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
