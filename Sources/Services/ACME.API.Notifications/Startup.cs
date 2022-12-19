using ACME.API.Notifications.Data;
using ACME.API.Notifications.Models;
using ACME.API.Notifications.Repositories;
using ACME.API.Notifications.Repositories.Interfaces;
using ACME.API.Notifications.Sagas;
using ACME.API.Notifications.Services;
using ACME.API.Notifications.Services.Interfaces;
using ACME.API.Notifications.Subscribers;
using ACME.Library.Common.Helpers;
using ACME.Library.Common.Outbox;
using ACME.Library.Outbox.EntityFramework.Extensions;
using ACME.Library.Outbox.Extensions;
using ACME.Library.RabbitMq.Configuration;
using ACME.Library.RabbitMq.Extensions;
using ACME.Library.Saga.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ACME.API.Notifications
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
               
            services.AddHttpClient();
            services.AddOptions();

            services.AddControllers();
            services.AddHealthChecks();
            

            //Database context
            services.AddDbContext<NotificationsDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DbConnection")));

            services.AddScoped<INotificationRepository, NotificationRepository>();

            
            services.AddSignalR();
            services.AddRabbitMq(Configuration.GetConnectionString("RabbitMqConnection"), new DefaultDeadLetterConfiguration());
            services.AddScoped<IMessageService, MessageService>();
            services.AddHostedService<NotificationSubscriber>();
            
            
            // SAGA Registration
            services.RegisterSaga<NotificationSaga, NotificationModel, NotificationRepository>();
            services.RegisterSagaHandler();

            // Transactional Outbox Registration
            services.AddScoped<IOutboxRepository, NotificationOutboxRepository>();
            services.RegisterEntityFrameworkOutboxPublisher();
            services.RegisterOutboxPublishWorker();
        }

        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            
            // app.UseMiddleware<SignalRCorsMiddleware>();
            

            // Don't log all requests by default
            // app.UseMiddleware<RequestResponseLoggingMiddleware>();

            // if (Configuration.IsSwaggerEnabled())
            // {
            //     app.UseSwagger();
            //     app.UseSwaggerForOcelotUI(opt =>
            //     {
            //         opt.PathToSwaggerGenerator = "/swagger/docs";
            //     }, uiOpt => {
            //         uiOpt.DocumentTitle = "Gateway documentation";
            //         uiOpt.InjectStylesheet("/css/custom-swagger-ui.css");
            //         uiOpt.InjectJavascript("/js/custom-swagger-ui.js");
            //     });
            // }
            
            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
                endpoints.MapHub<SignalRService>("/notifications");
            });

            app.UseDefaultFiles();
            
            app.UseStaticFiles();

            
            app.UseWebSockets();
            
            RunMigration(app);
        }
        
        private void RunMigration(IApplicationBuilder app)
        {
            bool Migrate()
            {
                // Run Migrations in app
                using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var context = scope.ServiceProvider.GetService<NotificationsDbContext>();
                if (context?.Database == null) return false;
                context.Database.Migrate();
                return true;
            }

            RetryHelper.Do(Migrate, TimeSpan.FromSeconds(20));
        }
    }
}
