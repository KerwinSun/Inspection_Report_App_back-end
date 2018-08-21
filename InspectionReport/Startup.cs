using System;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InspectionReport.Models;

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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            /// TODO: Should put the connection string as an environment variable.

            // Use SQL Database if in Azure, otherwise, use SQLite
            // To change the environment, and test the published Database on Azure, change the the ASPNETCORE_ENVIRONMENT
            // on both profiles in the launchSettings.json
            if (_env.IsDevelopment())
            {
                var connection = @"Server=(localdb)\mssqllocaldb;Database=InspectionReportDB;Trusted_Connection=True;ConnectRetryCount=0";
                services.AddDbContext<ReportContext>(options => options.UseSqlServer(connection));
            } else
            {
                services.AddDbContext<ReportContext>(options =>
                   options.UseSqlServer(Configuration.GetConnectionString("ProductionConnection")));
            }
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

            app.UseMvc();
        }
    }
}
