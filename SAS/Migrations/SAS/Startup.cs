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

        public void ConfigureServices(IServiceCollection services)
        {
            var cloudinarySettings = Configuration.GetSection("Cloudinary").Get<CloudinarySettings>();

            var account = new Account(
                cloudinarySettings.CloudName,
                cloudinarySettings.ApiKey,
                cloudinarySettings.ApiSecret
            );

            var cloudinary = new Cloudinary(account);

            services.AddSingleton(cloudinary);

            services.AddAutoMapper(typeof(Helper));

            services.AddControllersWithViews();

            services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("UniCon")));

            services.AddScoped<IStudentRepository, SQLStudentRepository>();
            services.AddScoped<IRepository<User>, SQLUserRepository>();
            services.AddScoped<INoticeRepository, SQLNoticeRepository>();
            services.AddScoped<IUserDetailsRepository,SQLUserDetailsRepository>();
            services.AddScoped<IPreviousStudentRepository, SQLPreviousStudentRepository>();
            services.AddScoped<IBillRepository, SQLBillRepository>();
            services.AddScoped<CloudinaryService>();
            services.AddHttpClient<IdCardService>();
            services.AddTransient<MailService>();

            services.AddHttpContextAccessor();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }
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

            app.UseSession(); 

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