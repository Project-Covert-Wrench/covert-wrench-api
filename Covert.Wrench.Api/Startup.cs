using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Covert.Wrench.Data;
using Microsoft.AspNetCore.Authorization;
using Covert.Wrench.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Covert.Wrench.Api
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
            string authority = Configuration["Auth0:Authority"] ??
                throw new ArgumentNullException("Auth0:Authority");
            
            string audience = Configuration["Auth0:Audience"] ??
                throw new ArgumentNullException("Auth0:Audience");

            services.AddControllers();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = audience;
                });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("delete:catalog", policy =>
                policy.Requirements.Add(
                    new HasScopeRequirements("delete:catalog", authority)
                ));
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddDbContext<StoreContext>(options =>
                options.UseSqlite("Data Source=../Registrar.sqlite",
                b => b.MigrationsAssembly("Covert.Wrench.Api")));
            services.AddCors(options =>
            {
               options.AddDefaultPolicy(policyBuilder =>
               {
                   policyBuilder.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
               });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "jet.piranha.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
