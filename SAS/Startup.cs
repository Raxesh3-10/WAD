using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SAS.Models;
using SAS.Services;
using System;
using SAS.Repositories;
using CloudinaryDotNet;
using SAS.Config;
using AutoMapper;
using SAS.Mappers;

namespace SAS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Bind Cloudinary settings from appsettings.json
            var cloudinarySettings = Configuration.GetSection("Cloudinary").Get<CloudinarySettings>();

            var account = new Account(
                cloudinarySettings.CloudName,
                cloudinarySettings.ApiKey,
                cloudinarySettings.ApiSecret
            );

            var cloudinary = new Cloudinary(account);

            // Register as singleton
            services.AddSingleton(cloudinary);

            services.AddAutoMapper(typeof(Helper));

            // Enable MVC
            services.AddControllersWithViews();

            // Enable SQL Server DB context
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("UniCon")));

            // Register Repositories
            services.AddScoped<IRepository<Student>, SQLStudentRepository>();
            services.AddScoped<IRepository<User>, SQLUserRepository>();
            services.AddScoped<IRepository<Notice>, SQLNoticeRepository>();
            services.AddScoped<IUserDetailsRepository,SQLUserDetailsRepository>();

            // Email service
            services.AddTransient<MailService>();

            // Enable HttpContextAccessor
            services.AddHttpContextAccessor();

            // Enable Session with optional settings
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // If using authentication, add it here
            // services.AddAuthentication(...);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession(); // Should come after UseRouting

            // If using authentication
            // app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
