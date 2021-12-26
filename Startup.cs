using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QiYeWeiXinNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QiYeWeiXinNotify
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
            services.Configure<HangFireModel>(HangfireSetup.Configuration.GetSection("HangFire"));
            services.AddSingleton<DapperContext>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddHangfireSetup();//任务调度c
            services.AddRazorPages();
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
                app.UseExceptionHandler("/Error");
            }
            ServiceLocator.Instance = app.ApplicationServices.CreateScope().ServiceProvider;
            app.UseHangfireMiddleware();//Job
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }

    public class HangFireModel
    {
        public string Connection { get; set; }
        public string PathMatch { get; set; }
        public string Login { get; set; }
        public string PaswordClear { get; set; }
    }
}
