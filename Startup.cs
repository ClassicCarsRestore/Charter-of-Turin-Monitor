using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Text;
using tasklist.Models;
using tasklist.Services;

namespace tasklist
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
                if (string.IsNullOrEmpty(corsOrigins))
                {
                    throw new InvalidOperationException("CORS_ORIGINS environment variable is not set. Please configure it in .env file or system environment variables.");
                }

                var origins = corsOrigins.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.Trim())
                    .ToArray();

                options.AddPolicy(name: MyAllowSpecificOrigins,
                    builder => builder
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .AllowAnyHeader());
            });

            // requires using Microsoft.Extensions.Options
            services.Configure<ProjectsDatabaseSettings>(
                Configuration.GetSection(nameof(ProjectsDatabaseSettings)));

            services.AddSingleton<IProjectsDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<ProjectsDatabaseSettings>>().Value);

            services.Configure<TasksDatabaseSettings>(
                Configuration.GetSection(nameof(TasksDatabaseSettings)));

            services.AddSingleton<ITasksDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<TasksDatabaseSettings>>().Value);

            services.Configure<SensorTasksDatabaseSettings>(
                Configuration.GetSection(nameof(SensorTasksDatabaseSettings)));

            services.AddSingleton<ISensorTasksDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<SensorTasksDatabaseSettings>>().Value);

            services.Configure<CredentialsDatabaseSettings>(
                Configuration.GetSection(nameof(CredentialsDatabaseSettings)));

            services.AddSingleton<ICredentialsDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CredentialsDatabaseSettings>>().Value);

            services.Configure<ActivityMapsDatabaseSettings>(
                Configuration.GetSection(nameof(ActivityMapsDatabaseSettings)));

            services.AddSingleton<IActivityMapsDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<ActivityMapsDatabaseSettings>>().Value);

            services.Configure<LoginCredentialsDatabaseSettings>(
                Configuration.GetSection(nameof(LoginCredentialsDatabaseSettings)));

            services.AddSingleton<ILoginCredentialsDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<LoginCredentialsDatabaseSettings>>().Value);

            services.Configure<PinterestDatabaseSettings>(
                Configuration.GetSection(nameof(PinterestDatabaseSettings)));

            services.AddSingleton<IPinterestDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<PinterestDatabaseSettings>>().Value);

            services.Configure<CameraHubDatabaseSettings>(
                Configuration.GetSection(nameof(CameraHubDatabaseSettings)));

            services.AddSingleton<ICameraHubDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CameraHubDatabaseSettings>>().Value);

            services.Configure<VirtualMapLocationsDatabaseSettings>(
                Configuration.GetSection(nameof(VirtualMapLocationsDatabaseSettings)));

            services.AddSingleton<IVirtualMapLocationsDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<VirtualMapLocationsDatabaseSettings>>().Value);

            services.Configure<ActivityAndLocationHistoryDatabaseSettings>(
                Configuration.GetSection(nameof(ActivityAndLocationHistoryDatabaseSettings)));

            services.AddSingleton<IActivityAndLocationHistoryDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<ActivityAndLocationHistoryDatabaseSettings>>().Value);


            services.AddSingleton<ProjectService>();

            services.AddSingleton<TaskService>();

            services.AddSingleton<SensorTaskService>();

            services.AddSingleton<CamundaService>();

            services.AddSingleton<AmazonS3Service>();

            services.AddSingleton<ActivityMapService>();

            services.AddSingleton<LoginCredentialsService>();

            services.AddSingleton<PinterestService>();

            services.AddSingleton<CameraHubService>();

            services.AddSingleton<VirtualMapLocationService>();

            services.AddSingleton<ActivityAndLocationHistoryService>();



            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist/ClientApp";
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "tasklist_api", Version = "v1" });
            });

            var key = Convert.FromBase64String(Settings.Secret);
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "tasklist_api v1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "tasklist_api v1"));

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
