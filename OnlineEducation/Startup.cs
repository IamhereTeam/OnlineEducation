using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineEducation.Common;
using OnlineEducation.DAL;

namespace OnlineEducation
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
            services.AddCors();
            services.AddControllers();

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddDbContext<OnlineEducationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("OnlineEducationDatabase")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OnlineEducation API", Version = "v1" });
                //c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                //{
                //    Description = "JWT Authorization header using the Bearer scheme. \"Bearer {token}\"",
                //    Name = "Authorization",
                //    In = "header",
                //    Type = "apiKey"
                //});
                //c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                //{
                //    { "Bearer", new string[] { } }
                //});
            });

            services.ConfigureServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {

#if DEBUG
                // For Debug in Kestrel
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnlineEducation API V1");
#else
   // To deploy on IIS
   c.SwaggerEndpoint("/webapi/swagger/v1/swagger.json", "Web API V1");
#endif
                c.RoutePrefix = string.Empty;
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            serviceScope.ServiceProvider.GetService<OnlineEducationDbContext>().Database.EnsureCreated();
        }
    }
}
