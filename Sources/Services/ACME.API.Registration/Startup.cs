using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ACME.API.Registration.Data;
using ACME.API.Registration.Mappers;
using ACME.API.Registration.Repositories;
using ACME.API.Registration.Repositories.Interfaces;
using ACME.API.Registration.Sagas;
using ACME.API.Registration.Services;
using ACME.API.Registration.Services.Interfaces;
using ACME.Library.Common.Helpers;
using ACME.Library.Common.Middlewares;
using ACME.Library.Common.Outbox;
using ACME.Library.Outbox.EntityFramework.Extensions;
using ACME.Library.Outbox.Extensions;
using ACME.Library.RabbitMq.Configuration;
using ACME.Library.RabbitMq.Extensions;
using ACME.Library.Saga.Extensions;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using ACME.Library.Domain.Registration;

namespace ACME.API.Registration
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
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            services.AddHealthChecks();
            services.AddDbContext<RegistrationDbContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("RegistrationDbConnection")));
            
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddHttpClient();
            services.AddHttpContextAccessor();

            // AutoMapper profiles
            services.AddAutoMapper(typeof(RegistrationMappingProfile));

            // Message Broker Registration
            services.AddRabbitMq(Configuration.GetConnectionString("RabbitMqConnection"), new DefaultDeadLetterConfiguration());
            
            // Transactional Outbox Registration
            services.AddScoped<IOutboxRepository, RegistrationOutboxRepository>();
            services.RegisterEntityFrameworkOutboxPublisher();
            services.RegisterOutboxPublishWorker();

            // SAGA Registration
            services.RegisterSaga<RegistrationSaga, RegistrationData, RegistrationService>();
            services.RegisterSagaHandler();
            
            //Here all services that need to be injected.
            //Take care of the way how: Scoped, Transient or Singleton
            services.AddScoped<IRegistrationRepository, RegistrationRepository>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<IQueueRegistrationService, QueueRegistrationService>();
            

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Registration", Version = "v1" });
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authorization",
                    Description = "registration service",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lower case
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme, new string[] { }}
                });
                AddSwaggerComments(c);
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            });
        }
        
        private void AddSwaggerComments(SwaggerGenOptions c)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly().GetName().Name}.xml");
            if (File.Exists(filePath))
            {
                // For example: AMCE.API.Notification.xml
                c.IncludeXmlComments(filePath);
                return;
            }
            Console.WriteLine($"swagger {filePath} not found");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<SwaggerHomeRedirectMiddleware>();


            app.UseRouting();

            app.UseCors();
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Registration"); 
            });
            
            RunMigration(app);
        }

        private void RunMigration(IApplicationBuilder app)
        {
            bool Migrate()
            {
                // Run Migrations in app
                using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var context = scope.ServiceProvider.GetService<RegistrationDbContext>();
                if (context?.Database == null) return false;
                context.Database.Migrate();
                return true;
            }
            
            RetryHelper.Do(Migrate, TimeSpan.FromSeconds(15),5);
        }
        
    }
}
