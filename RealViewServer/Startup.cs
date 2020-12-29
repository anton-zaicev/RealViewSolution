using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RealViewServer.Storage;

namespace RealViewServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            using (var client = new DatabaseContext())
                client.Database.EnsureCreated();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc();
            services.AddEntityFrameworkSqlite().AddDbContext<DatabaseContext>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}");
                endpoints.MapControllerRoute(name: "photos", pattern: "{controller=Admin}/projects/{projectName}/{action=Photos}");
                endpoints.MapControllerRoute(name: "admin", pattern: "{controller=Admin}/{action=Projects}");
            });
        }
    }
}
