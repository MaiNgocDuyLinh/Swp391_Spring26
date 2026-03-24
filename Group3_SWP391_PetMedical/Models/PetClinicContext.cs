using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using System.Text.Json;
using Group3_SWP391_PetMedical.Models.TempShopModels;

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
      public virtual DbSet<ScheduleChangeRequest> ScheduleChangeRequests { get; set; }
      public virtual DbSet<Service> Services { get; set; }
      public virtual DbSet<User> Users { get; set; }

      // ✅ NEW TABLE
      public virtual DbSet<AuditLog> AuditLogs { get; set; }
      public virtual DbSet<ServiceDiscount> ServiceDiscounts { get; set; }


      public virtual DbSet<RetailOrder> RetailOrders { get; set; }
      public virtual DbSet<OrderDetail> OrderDetails { get; set; }
      public virtual DbSet<CartMedicin> CartsMedicin { get; set; }
      public virtual DbSet<CartItemMedicin> CartItemsMedicin { get; set; }

      // ✅ NEW SHOP TABLES
      public virtual DbSet<ProductCategory> ProductCategories { get; set; }
      public virtual DbSet<Product> Products { get; set; }
      public virtual DbSet<ProductVariant> ProductVariants { get; set; }
      public virtual DbSet<Cart> Carts { get; set; }
      public virtual DbSet<CartItem> CartItems { get; set; }
      public virtual DbSet<ProductOrder> ProductOrders { get; set; }
      public virtual DbSet<ProductOrderItem> ProductOrderItems { get; set; }

      // ================= AUTO AUDIT =================

      public override async Task<int> SaveChangesAsync(
          CancellationToken cancellationToken = default)
      {
            var httpContext = _httpContextAccessor?.HttpContext;
            var currentUser = httpContext?.User;

            var userIdClaim = currentUser?.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? currentUser?.FindFirstValue("user_id");
            var userEmail = currentUser?.FindFirstValue(ClaimTypes.Email)
                            ?? currentUser?.Identity?.Name;
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
            modelBuilder.Entity<ScheduleChangeRequest>().HasKey(r => r.request_id);
            modelBuilder.Entity<Service>().HasKey(s => s.service_id);
            modelBuilder.Entity<ServiceDiscount>().HasKey(sd => sd.discount_id);
            modelBuilder.Entity<User>().HasKey(u => u.user_id);
            modelBuilder.Entity<AuditLog>().HasKey(a => a.AuditLogId);

            // RetailOrder ↔ OrderDetail / User / Medication
            modelBuilder.Entity<RetailOrder>(entity =>
            {
                  entity.HasKey(ro => ro.id);
                  entity.ToTable("RetailOrders");

                  entity.HasOne(ro => ro.user)
                    .WithMany(u => u.RetailOrders)
                    .HasForeignKey(ro => ro.user_id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                  entity.Property(ro => ro.status_order)
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                  // Composite primary key (order_id, medicine_id)
                  entity.HasKey(od => new { od.order_id, od.medicine_id });
                  entity.ToTable("OrderDetails");

                  entity.HasOne(od => od.order)
                    .WithMany(ro => ro.OrderDetails)
                    .HasForeignKey(od => od.order_id)
                    .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(od => od.medicine)
                    .WithMany() // no navigation collection on Medication for order details
                    .HasForeignKey(od => od.medicine_id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // Cart ↔ User / CartItemsMedicin / Medication
            modelBuilder.Entity<CartMedicin>(entity =>
            {
                  entity.HasKey(c => c.id);
                  entity.ToTable("CartsMedicin");

                  entity.HasOne(c => c.user)
                    .WithMany(u => u.CartsMedicin)
                    .HasForeignKey(c => c.user_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItemMedicin>(entity =>
            {
                  entity.HasKey(ci => new { ci.cart_id, ci.medicine_id });
                  entity.ToTable("CartItemsMedicin");

                  entity.HasOne(ci => ci.cart)
                    .WithMany(c => c.CartItemsMedicin)
                    .HasForeignKey(ci => ci.cart_id)
                    .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(ci => ci.medicine)
                    .WithMany() // no navigation collection on Medication for cart items
                    .HasForeignKey(ci => ci.medicine_id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

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

            // ScheduleChangeRequest ↔ Schedule, User (doctor), User (decided_by)
            modelBuilder.Entity<ScheduleChangeRequest>(entity =>
            {
                  entity.HasOne(r => r.schedule)
                    .WithMany()
                    .HasForeignKey(r => r.schedule_id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                  entity.HasOne(r => r.doctor)
                    .WithMany()
                    .HasForeignKey(r => r.doctor_id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                  entity.HasOne(r => r.decidedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.decided_by)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
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

            // ================= SHOP MODULE CONFIG =================

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                  entity.ToTable("ProductCategories");

                  entity.HasKey(e => e.CategoryId);

                  entity.HasIndex(e => e.CategoryName)
                    .IsUnique()
                    .HasDatabaseName("UQ_ProductCategories_CategoryName");

                  entity.Property(e => e.CategoryId).HasColumnName("category_id");
                  entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .HasColumnName("category_name");
                  entity.Property(e => e.Description).HasColumnName("description");
                  entity.Property(e => e.Status).HasColumnName("status");
                  entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_at");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                  entity.ToTable("Products");

                  entity.HasKey(e => e.ProductId);

                  entity.HasIndex(e => e.Sku)
                    .IsUnique()
                    .HasDatabaseName("UQ_Products_SKU");

                  entity.Property(e => e.ProductId).HasColumnName("product_id");
                  entity.Property(e => e.CategoryId).HasColumnName("category_id");
                  entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .HasColumnName("name");
                  entity.Property(e => e.Sku)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("sku");
                  entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("price");
                  entity.Property(e => e.StockQuantity)
                    .HasDefaultValue(0)
                    .HasColumnName("stock_quantity");
                  entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Đang bán")
                    .HasColumnName("status");
                  entity.Property(e => e.Description).HasColumnName("description");
                  entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("image_url");
                  entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_at");
                  entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("updated_at");

                  entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_ProductCategories");
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                  entity.ToTable("ProductVariants");

                  entity.HasKey(e => e.VariantId);

                  entity.Property(e => e.VariantId).HasColumnName("variant_id");
                  entity.Property(e => e.ProductId).HasColumnName("product_id");
                  entity.Property(e => e.VariantName)
                    .HasMaxLength(255)
                    .HasColumnName("variant_name");
                  entity.Property(e => e.Color)
                    .HasMaxLength(50)
                    .HasColumnName("color");
                  entity.Property(e => e.Size)
                    .HasMaxLength(50)
                    .HasColumnName("size");
                  entity.Property(e => e.Material)
                    .HasMaxLength(100)
                    .HasColumnName("material");
                  entity.Property(e => e.Sku)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("sku");
                  entity.Property(e => e.PriceOverride)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("price_override");
                  entity.Property(e => e.StockQuantity)
                    .HasDefaultValue(0)
                    .HasColumnName("stock_quantity");
                  entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Đang bán")
                    .HasColumnName("status");
                  entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("image_url");
                  entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_at");
                  entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("updated_at");

                  entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductVariants_Products");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                  entity.ToTable("Carts");

                  entity.HasKey(e => e.CartId);

                  entity.Property(e => e.CartId).HasColumnName("cart_id");
                  entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                  entity.Property(e => e.Status)
                    .HasMaxLength(30)
                    .HasDefaultValue("Đang hoạt động")
                    .HasColumnName("status");
                  entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_at");
                  entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("updated_at");

                  entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Carts_Users");
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                  entity.ToTable("CartItems");

                  entity.HasKey(e => e.CartItemId);

                  entity.HasIndex(e => new { e.CartId, e.ProductId, e.VariantId })
                    .IsUnique()
                    .HasDatabaseName("UQ_CartItems_Cart_Product_Variant");

                  entity.Property(e => e.CartItemId).HasColumnName("cart_item_id");
                  entity.Property(e => e.CartId).HasColumnName("cart_id");
                  entity.Property(e => e.ProductId).HasColumnName("product_id");
                  entity.Property(e => e.VariantId).HasColumnName("variant_id");
                  entity.Property(e => e.Quantity)
                    .HasDefaultValue(1)
                    .HasColumnName("quantity");
                  entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("unit_price");
                  entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_at");

                  entity.HasOne<Cart>()
                    .WithMany()
                    .HasForeignKey(e => e.CartId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CartItems_Carts");

                  entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CartItems_Products");

                  entity.HasOne<ProductVariant>()
                    .WithMany()
                    .HasForeignKey(e => e.VariantId)
                    .HasConstraintName("FK_CartItems_ProductVariants");
            });

            modelBuilder.Entity<ProductOrder>(entity =>
            {
                  entity.ToTable("ProductOrders");

                  entity.HasKey(e => e.OrderId);

                  entity.HasIndex(e => e.OrderCode)
                    .IsUnique()
                    .HasDatabaseName("UQ_ProductOrders_OrderCode");

                  entity.Property(e => e.OrderId).HasColumnName("order_id");
                  entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                  entity.Property(e => e.OrderCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("order_code");
                  entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0m)
                    .HasColumnName("total_amount");
                  entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(50)
                    .HasColumnName("payment_method");
                  entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(30)
                    .HasDefaultValue("Chưa thanh toán")
                    .HasColumnName("payment_status");
                  entity.Property(e => e.OrderStatus)
                    .HasMaxLength(30)
                    .HasDefaultValue("Chờ xác nhận")
                    .HasColumnName("order_status");
                  entity.Property(e => e.PickupMethod)
                    .HasMaxLength(50)
                    .HasDefaultValue("Nhận tại phòng khám")
                    .HasColumnName("pickup_method");
                  entity.Property(e => e.PickupNote)
                    .HasMaxLength(500)
                    .HasColumnName("pickup_note");
                  entity.Property(e => e.PickupDate)
                    .HasColumnType("datetime")
                    .HasColumnName("pickup_date");
                  entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_at");

                  entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrders_Users");
            });

            modelBuilder.Entity<ProductOrderItem>(entity =>
            {
                  entity.ToTable("ProductOrderItems");

                  entity.HasKey(e => e.OrderItemId);

                  entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
                  entity.Property(e => e.OrderId).HasColumnName("order_id");
                  entity.Property(e => e.ProductId).HasColumnName("product_id");
                  entity.Property(e => e.VariantId).HasColumnName("variant_id");
                  entity.Property(e => e.ProductName)
                    .HasMaxLength(200)
                    .HasColumnName("product_name");
                  entity.Property(e => e.VariantName)
                    .HasMaxLength(255)
                    .HasColumnName("variant_name");
                  entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("unit_price");
                  entity.Property(e => e.Quantity)
                    .HasColumnName("quantity");
                  entity.Property(e => e.LineTotal)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("line_total");
                  entity.HasOne(d => d.Order)
                    .WithMany(p => p.ProductOrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrderItems_ProductOrders");

                  entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductOrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrderItems_Products");

                  entity.HasOne(d => d.Variant)
                    .WithMany(p => p.ProductOrderItems)
                    .HasForeignKey(d => d.VariantId)
                    .HasConstraintName("FK_ProductOrderItems_ProductVariants"); entity.HasOne(d => d.Order)
                    .WithMany(p => p.ProductOrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrderItems_ProductOrders");

                  entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductOrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrderItems_Products");

                  entity.HasOne(d => d.Variant)
                    .WithMany(p => p.ProductOrderItems)
                    .HasForeignKey(d => d.VariantId)
                    .HasConstraintName("FK_ProductOrderItems_ProductVariants");
            });
            modelBuilder.Entity<ServiceDiscount>(entity =>
            {
                  entity.ToTable("ServiceDiscount");
                  entity.HasOne(sd => sd.service)
                    .WithMany(s => s.ServiceDiscounts)
                    .HasForeignKey(sd => sd.service_id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

            });
      }

      partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}