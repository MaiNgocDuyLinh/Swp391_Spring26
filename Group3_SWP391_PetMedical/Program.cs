using Group3_SWP391_PetMedical.Models;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Repository.Implementations;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.Services.Implementations;

namespace Group3_SWP391_PetMedical
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký DbContext với SQL Server
            builder.Services.AddDbContext<PetClinicContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddScoped<IServiceService, ServiceService>();

            //list pet
            builder.Services.AddScoped<IPetRepository, PetRepository>();
            builder.Services.AddScoped<IPetService, PetService>();

            //edit pet
            // (Lưu ý: Bạn đang khai báo trùng PetRepository/Service ở đây, 
            // nhưng mình giữ nguyên theo ý bạn để không ảnh hưởng code cũ)
            builder.Services.AddScoped<IPetRepository, PetRepository>();
            builder.Services.AddScoped<IPetService, PetService>();

            // Staff Module DI
            builder.Services.AddScoped<Group3_SWP391_PetMedical.Repository.Interfaces.IUserRepository,
                                       Group3_SWP391_PetMedical.Repository.Implementations.UserRepository>();
            builder.Services.AddScoped<Group3_SWP391_PetMedical.Repository.Interfaces.IAppointmentRepository,
                                       Group3_SWP391_PetMedical.Repository.Implementations.AppointmentRepository>();
            builder.Services.AddScoped<Group3_SWP391_PetMedical.Services.Interfaces.IStaffService,
                                       Group3_SWP391_PetMedical.Services.Implementations.StaffService>();

            builder.Services.AddAuthentication("MyCookieAuth")
            .AddCookie("MyCookieAuth", options =>
            {
                options.Cookie.Name = "MyLoginCookie";
                options.LoginPath = "/Login/Login"; // Đường dẫn trả về nếu chưa đăng nhập
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Hết hạn sau 30 phút
            });

            // ============================================================
            // 1. THÊM MỚI: Đăng ký dịch vụ Session (Bắt buộc để sửa lỗi)
            // ============================================================
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            // ============================================================

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ============================================================
            // 2. THÊM MỚI: Kích hoạt Session Middleware (Phải đặt sau UseRouting)
            // ============================================================
            app.UseSession();
            // ============================================================

            // Phải đặt UseAuthentication() trước UseAuthorization()
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles(); // (Dòng này bị lặp lại trong code gốc, nhưng mình giữ nguyên theo yêu cầu)

            app.MapGet("/", context =>
            {
                context.Response.Redirect("/Home");
                return Task.CompletedTask;
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}