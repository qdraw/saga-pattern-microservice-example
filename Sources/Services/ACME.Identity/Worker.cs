using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using ACME.Identity.Controllers;
using ACME.Identity.Data;
using ACME.Identity.Models;
using ACME.Identity.Repositories.Interfaces;
using ACME.Identity.Services;
using ACME.Identity.Services.Interfaces;
using System.Globalization;
using ACME.Library.Common.Helpers;
using ACME.Library.Domain.Enums;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ACME.Identity
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            async Task<bool> MigrateAsync()
            {
                // Run Migrations in app
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (context.Database == null!) return false;
                await context.Database.MigrateAsync(cancellationToken);
                return true;
            }
            
            await RetryHelper.DoAsync(MigrateAsync, TimeSpan.FromSeconds(20),3);

            using var scope = _serviceProvider.CreateScope();

            await RegisterApplicationsAsync(scope.ServiceProvider);
            await RegisterScopesAsync(scope.ServiceProvider, cancellationToken);

            static async Task RegisterApplicationsAsync(IServiceProvider provider)
            {
                var logger = provider.GetRequiredService<ILogger<Worker>>();
                var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                var gatewayClientSecret = configuration.GetValue<string>("GatewayClientSecret");
                
                // BFF
                var testGatewayClient = new OpenIddictApplicationDescriptor
                {
                    ClientId = "testGatewayClient",
                    ConsentType = ConsentTypes.Implicit,
                    DisplayName = "Test Gateway Client",
                    DisplayNames = { [CultureInfo.GetCultureInfo("nl-NL")] = "Test Gateway Client" },
                    PostLogoutRedirectUris =
                    {
                        new Uri("http://localhost:5025/signout-callback-oidc"),
                        new Uri("http://localhost:9006/signout-callback-oidc"),
                        new Uri("https://localhost:9025/signout-callback-oidc"),
                    },
                    RedirectUris =
                    {
                        new Uri("http://localhost:5025/signin-oidc"),
                        new Uri("http://localhost:9006/signin-oidc"),
                        new Uri("https://localhost:9025/signin-oidc"),
                    },
                    ClientSecret = gatewayClientSecret,
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Revocation,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "dataEventRecords"
                    },
                    Type = ClientTypes.Confidential,
                    Requirements = { Requirements.Features.ProofKeyForCodeExchange }
                };

                var application = await manager.FindByClientIdAsync("testGatewayClient");
                if (application == null)
                {
                    logger.LogInformation("set: gatewayClientSecret: {gatewayClientSecret}", gatewayClientSecret);
                    await manager.CreateAsync(testGatewayClient);
                }
                else if (application is OpenIddictEntityFrameworkCoreApplication coreApplication 
                         && (coreApplication.RedirectUris?.Split(",").Length != testGatewayClient.RedirectUris.Count || 
                             coreApplication.PostLogoutRedirectUris?.Split(",").Length != testGatewayClient.PostLogoutRedirectUris.Count ||
                             coreApplication.ConsentType != testGatewayClient.ConsentType))
                {
                    logger.LogInformation("updated OpenIddictApplicationDescriptor");
                    await manager.UpdateAsync(application, testGatewayClient);
                }

                var roleService = provider.GetRequiredService<IRoleService>();


                var gatewayAdminTestUserName = configuration.GetValue<string>("GatewayAdminTestUserName");
                var gatewayAdminTestPassword = configuration.GetValue<string>("GatewayAdminTestPassword");
                var account = new RegisterUserModel
                {
                    EmailAddress = gatewayAdminTestUserName,
                    Password = gatewayAdminTestPassword,
                    Code = "CO00000001",
                    FirstName = "Admin",
                    LastName = "Backoffice",
                    Locale = LocaleType.nl,
                    CorrelationId = Guid.NewGuid(),
                    State = "seedData"
                };
                await AddNewUserIfNotExist(provider, manager, configuration, roleService, account);

                var gatewayContactTestUserName = configuration.GetValue<string>("GatewayContactTestUserName");
                var gatewayContactTestPassword = configuration.GetValue<string>("GatewayContactTestPassword");
                
                var contactUser = new RegisterUserModel
                {
                    EmailAddress = gatewayContactTestUserName,
                    Password = gatewayContactTestPassword,
                    Code = "CO00000002",
                    FirstName = "Contact",
                    LastName = "Test company",
                    Locale = LocaleType.nl,
                    CompanyCode = "COMPANY00000001",
                    CorrelationId = Guid.NewGuid(),
                    State = "seedData"
                };
                await AddNewUserIfNotExist(provider, manager, configuration, roleService, contactUser);
            }

            static async Task RegisterScopesAsync(IServiceProvider provider, CancellationToken token)
            {
                var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("dataEventRecords", token) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        DisplayName = "dataEventRecords API access",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("nl-NL")] = "API access"
                        },
                        Name = "dataEventRecords",
                        Resources =
                        {
                            "rs_dataEventRecordsApi"
                        }
                    }, token);
                }
            }
        }

        private static async Task AddNewUserIfNotExist(IServiceProvider provider, IOpenIddictApplicationManager manager, IConfiguration configuration, 
            IRoleService roleService, RegisterUserModel model)
        {
            if (!string.IsNullOrEmpty(model.EmailAddress) && !string.IsNullOrEmpty(model.Password))
            {
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                var checkIfUserExists = await userManager.FindByEmailAsync(model.EmailAddress);
                if (checkIfUserExists?.Email == null)
                {
                    var authorizationManager = provider.GetRequiredService<IOpenIddictAuthorizationManager>();
                    var loggerAccount = provider.GetRequiredService<ILogger<AccountController>>();
                    var signInManager = provider.GetRequiredService<SignInManager<ApplicationUser>>();
                    var userRepo = provider.GetRequiredService<IUserRepository>();
                    var mapper = provider.GetRequiredService<IMapper>();
                        
                    var userService = new UserService(signInManager, mapper, userManager, loggerAccount, manager, authorizationManager, configuration, userRepo);
                    await userService.Register(model);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}