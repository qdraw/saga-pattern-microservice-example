using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Quartz;
using ACME.Identity;
using ACME.Identity.Data;
using ACME.Identity.Helpers;
using ACME.Identity.Mappers;
using ACME.Identity.Models;
using ACME.Identity.Repositories;
using ACME.Identity.Repositories.Interfaces;
using ACME.Identity.Sagas;
using ACME.Identity.Security;
using ACME.Identity.Services;
using ACME.Identity.Services.Interfaces;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Text;
using ACME.Identity.Extentions;
using ACME.Library.Common.Helpers;
using ACME.Library.Common.Outbox;
using ACME.Library.Outbox.EntityFramework.Extensions;
using ACME.Library.Outbox.Extensions;
using ACME.Library.RabbitMq.Configuration;
using ACME.Library.RabbitMq.Extensions;
using ACME.Library.Saga.Extensions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration.GetConnectionString("DefaultConnection") == null)
{
    throw new ConfigurationErrorsException("missing DefaultConnection connection string");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | 
        ForwardedHeaders.XForwardedHost | 
        ForwardedHeaders.XForwardedProto;

    options.ForwardLimit=2;  //Limit number of proxy hops trusted
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict();
});

void AddSwaggerComments(SwaggerGenOptions c)
{
    var filePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml");
    if (File.Exists(filePath))
    {
        c.IncludeXmlComments(filePath);
        return;
    }
    Console.WriteLine($"swagger {filePath} not found");
}

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity", Version = "v1" });
    AddSwaggerComments(c);
    c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
});

bool CheckIfDatabaseIsReady()
{
#pragma warning disable ASP0000
    using var serviceProvider = builder.Services.BuildServiceProvider();
#pragma warning restore ASP0000
    serviceProvider.GetRequiredService<ApplicationDbContext>();
    return true;
}

RetryHelper.Do(CheckIfDatabaseIsReady, TimeSpan.FromSeconds(20), 4);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IQueueService, QueueService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddRabbitMq(builder.Configuration.GetConnectionString("RabbitMqConnection"), new DefaultDeadLetterConfiguration());

// SAGA Registration
builder.Services.RegisterSaga<UserSaga, RegistrationUsers, UserService>();
builder.Services.RegisterSagaHandler();

// Transactional Outbox Registration
builder.Services.AddScoped<IOutboxRepository, UserOutboxRepository>();
builder.Services.RegisterEntityFrameworkOutboxPublisher();
builder.Services.RegisterOutboxPublishWorker();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserNameClaimType = Claims.Name;
    options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
    options.ClaimsIdentity.RoleClaimType = Claims.Role;
    options.ClaimsIdentity.EmailClaimType = Claims.Email;
    // todo: set to true
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
});

builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceReDirector(HttpStatusCode statusCode, 
    Func<RedirectContext<CookieAuthenticationOptions>, Task> existingReDirector) => context =>
{
    if ( !context.Request.Path.StartsWithSegments("/api") )
        return existingReDirector(context);
    context.Response.StatusCode = ( int ) statusCode;
    var jsonString = "{\"errors\": [{\"status\": \""+ (int) statusCode + "\" }]}";

    context.Response.ContentType = "application/json";
    var data = Encoding.UTF8.GetBytes(jsonString);
    return context.Response.Body.WriteAsync(data,0, data.Length);
};

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
var authConfig = builder.Configuration.GetAuthorizationConfiguration();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
}).AddJwtBearer("Bearer", options =>
    {
        options.Authority = authConfig.Authority;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();

        options.UseQuartz();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/connect/authorize")
            .SetLogoutEndpointUris("/connect/logout")
            .SetIntrospectionEndpointUris("/connect/introspect")
            .SetTokenEndpointUris("/connect/token")
            .SetUserinfoEndpointUris("/connect/userinfo")
            .SetVerificationEndpointUris("/connect/verify");

        options.AllowAuthorizationCodeFlow()
            .AllowHybridFlow()
            .AllowClientCredentialsFlow()
            .AllowRefreshTokenFlow();
            
        options
            .SetAccessTokenLifetime(TimeSpan.FromHours(8))
            .SetIdentityTokenLifetime(TimeSpan.FromHours(8));

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

        // todo: check if AddDevelopmentEncryptionCertificate is ok for production
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .DisableTransportSecurityRequirement()
            .EnableAuthorizationEndpointPassthrough()
            .EnableLogoutEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "__Host-identity";
    options.ExpireTimeSpan =  TimeSpan.FromHours(8);
    options.SlidingExpiration = false;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.AccessDeniedPath = "/forbidden";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.Events.OnRedirectToLogin = ReplaceReDirector(HttpStatusCode.Unauthorized, options.Events.OnRedirectToLogin);
});

builder.Services.AddHostedService<Worker>();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Fix for http instead of https urls in .well-known/openid-configuration
if (!app.Environment.IsDevelopment())
{
    app.Use((context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/.well-known"))
        {
            context.Request.Scheme = "https";
        }
        return next(context);
    });
}

IdentityModelEventSource.ShowPII = true;

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "k8s-test")
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

StaticServeHelper.Serve(app);

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");
    endpoints.MapHealthChecks("/health");
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.Run();
