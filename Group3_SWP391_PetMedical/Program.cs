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
            builder.Services.AddScoped<IPetRepository, PetRepository>();
            builder.Services.AddScoped<IPetService, PetService>();
            // User (Profile, ChangePassword)
            builder.Services.AddScoped<Group3_SWP391_PetMedical.Repository.Interfaces.IUserRepository,
                                       Group3_SWP391_PetMedical.Repository.Implementations.UserRepository>();
            builder.Services.AddScoped<Group3_SWP391_PetMedical.Services.Interfaces.IUserService,
                                       Group3_SWP391_PetMedical.Services.Implementations.UserService>();

            // Staff Module DI
            builder.Services.AddScoped<Group3_SWP391_PetMedical.Repository.Interfaces.IAppointmentRepository,
                                       Group3_SWP391_PetMedical.Repository.Implementations.AppointmentRepository>();
            builder.Services.AddScoped<Group3_SWP391_PetMedical.Services.Interfaces.IStaffService,
                                       Group3_SWP391_PetMedical.Services.Implementations.StaffService>();
            // Manager Module DI (uses shared IServiceRepository)
            builder.Services.AddScoped<Group3_SWP391_PetMedical.Services.Interfaces.IManagerService,
                                       Group3_SWP391_PetMedical.Services.Implementations.ManagerService>();

            builder.Services.AddAuthentication("MyCookieAuth")
            .AddCookie("MyCookieAuth", options =>
            {
            options.Cookie.Name = "MyLoginCookie";
            options.LoginPath = "/Login/Login"; // Đường dẫn trả về nếu chưa đăng nhập
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // 30p hết hạn
            });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Phải đặt UseAuthentication() trước UseAuthorization() .
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

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
