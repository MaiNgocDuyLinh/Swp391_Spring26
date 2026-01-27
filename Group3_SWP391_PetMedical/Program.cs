using Group3_SWP391_PetMedical.Models;
using Microsoft.EntityFrameworkCore;

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

            builder.Services.AddAuthentication("MyCookieAuth")
            .AddCookie("MyCookieAuth", options =>
            {
            options.Cookie.Name = "MyLoginCookie";
            options.LoginPath = "/Login/Login"; // Đường dẫn trả về nếu chưa đăng nhập
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Hết hạn sau 30 phút
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

            // Phải đặt UseAuthentication() trước UseAuthorization()
            app.UseAuthentication();
            app.UseAuthorization();

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
