﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InspectionReport.Models;
using Microsoft.AspNetCore.Identity;
using InspectionReport.Services;
using InspectionReport.Services.Interfaces;
using InspectionReport.Utility;
using System.IO;
using PdfSharp.Fonts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;

namespace InspectionReport
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                /*options.AddPolicy("localhost", builder => builder
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                );

                options.AddPolicy("deployment", builder => builder
                    .WithOrigins("https://inspection-report-app.azurewebsites.net")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                );*/

                options.AddPolicy("localhost", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                );

            });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(
                    options => options.SerializerSettings.ReferenceLoopHandling =
                        Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddTransient<IEmailService, EmailService>();

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = false;
                options.Cookie.SameSite = SameSiteMode.None;
                options.ExpireTimeSpan = TimeSpan.FromDays(5);
                options.SlidingExpiration = true;
            });
            
            /// TODO: Should put the connection string as an environment variable.

            // Use SQL Database if in Azure, otherwise, use SQLite
            // To change the environment, and test the published Database on Azure, change the the ASPNETCORE_ENVIRONMENT
            // on both profiles in the launchSettings.json

            if (_env.IsDevelopment())
            {
                var connection =
                    @"Server=(localdb)\mssqllocaldb;Database=InspectionReportDB;Trusted_Connection=True;ConnectRetryCount=0";
                services.AddDbContext<ReportContext>(options => options.UseSqlServer(connection));
            }
            else
            {
                services.AddDbContext<ReportContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("ProductionConnection")));
            }

			GlobalFontSettings.FontResolver = new FontResolver();

			//services.AddTransient<IAuthorizeService, AuthorizeService>(); // Injecting the Authorize Service
			services.AddTransient<IAuthorizeService, MockAuthorizeService>(); //USE THIS ONLY TO BYPASS AUTHORIZATION.
            services.AddTransient<IImageService, ImageService>(); //Inject image service.

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ReportContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            // Automatically perform database migration
            services.BuildServiceProvider().GetService<ReportContext>().Database.Migrate();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseDeveloperExceptionPage(); // TODO: Remove once finished debugging.
            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseCors("localhost");
            app.UseCors("deployment");
            app.UseMvc();

        }
    }
}
