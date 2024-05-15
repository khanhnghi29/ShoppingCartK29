using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.Extensions;

namespace ShoppingCart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<KHShop29Context>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });
            // Config session
			builder.Services.AddDistributedMemoryCache();
			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(10);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});
            // Use Service automapper
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));


            //Config Xac thuc khi login
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
			{
                options.LoginPath = "/KhachHang/DangNhap";
                options.AccessDeniedPath = "/AccessDenied";
			});

			//Config PaypalClient dạng Singleton() - chỉ có 1 instance duy nhất trong toàn ứng dụng
            // Aplly to all -> Yes
			builder.Services.AddSingleton(options => new PaypalClient(
					builder.Configuration["PaypalOptions:AppId"],
					builder.Configuration["PaypalOptions:AppSecret"],
					builder.Configuration["PaypalOptions:Mode"]
			));

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
            app.UseAuthentication();
            app.UseSession();
            app.UseAuthorization();
			app.MapControllerRoute(
	        name: "MyArea",
	        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
			app.MapControllerRoute(
                name: "default",
                pattern: "{controller=HangHoa}/{action=Index}/{id?}");
            

            app.Run();
        }
    }
}