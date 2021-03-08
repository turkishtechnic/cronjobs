using TT.Cronjobs;
using TT.Cronjobs.AspNetCore;
using HangfireDemo.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace HangfireDemo
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
            services.Configure<CronjobsOptions>(Configuration.GetSection(CronjobsOptions.Key));
            services.AddCronJobs();

            services.AddTransient<CreateReport>();
            services.AddTransient<ReallyLongCronjob>();

            services.AddDbContext<DemoDbContext>(builder => builder.UseInMemoryDatabase(nameof(DemoDbContext)));

            services.AddRouting();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "HangfireDemo", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DemoDbContext db)
        {
            if (env.IsDevelopment())
            {
                db.Database.EnsureCreated();
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = "";
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HangfireDemo v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapCronJobWebhook("/-/cronjobs");
                endpoints.MapControllers();
            });
        }
    }
}