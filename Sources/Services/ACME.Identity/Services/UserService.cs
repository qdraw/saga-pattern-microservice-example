using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using ACME.Identity.Controllers;
using ACME.Identity.Models;
using ACME.Identity.Repositories.Interfaces;
using ACME.Identity.Services.Interfaces;
using System.Security.Claims;
using ACME.Library.Domain.Core;
using ACME.Library.Saga.Abstractions;

namespace ACME.Identity.Services
{
    public class UserService : IUserService, ISagaStateRepository<RegistrationUsers>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly string _gatewayClientId;

        public UserService(SignInManager<ApplicationUser> signInManager,
                           IMapper mapper,
                           UserManager<ApplicationUser> userManager,
                           ILogger<AccountController> logger,
                           IOpenIddictApplicationManager applicationManager,
                           IOpenIddictAuthorizationManager authorizationManager,
                            IConfiguration configuration,
                            IUserRepository userRepository)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _gatewayClientId = configuration["GatewayClientId"];
            _userRepository = userRepository;
        }

        public async Task Delete(Guid id)
        {
            //TODO validatie en checks?
            var obj = await _userManager.FindByIdAsync(id.ToString());
            if (obj != null)
            {
                await _userManager.DeleteAsync(obj);
            }
        }


        public async Task<IEnumerable<User>> GetAllAsync()
        {
            //TODO add pagination/sorting etc.
            var objs = _userManager.Users.ToList();
            var users = new List<User>();
            //Add claims and Roles
            foreach (var obj in objs)
            {
                var user = _mapper.Map<ApplicationUser, User>(obj);
                users.Add(await PopulateClaimsAndRoles(user, obj));
            }
            return users;
        }

        public async Task<User> GetAsync(Guid id)
        {
            var obj = await _userManager.FindByIdAsync(id.ToString());
            if (obj != null)
            {
                var user = _mapper.Map<ApplicationUser, User>(obj);
                if (user != null)
                {
                    return await PopulateClaimsAndRoles(user, obj);
                }
            }
            return new User();
        }

        public async Task<User> GetAsync(string email)
        {
            var obj = await _userManager.FindByEmailAsync(email);
            if (obj != null)
            {
                var user = _mapper.Map<ApplicationUser, User>(obj);
                if (user != null)
                {
                    return await PopulateClaimsAndRoles(user, obj);
                }
            }
            return new User();
        }

        public async Task Update(User user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (existingUser != null)
            {
                var updatedUser = _mapper.Map<User, ApplicationUser>(user);
                //Update user
                await _userManager.UpdateAsync(updatedUser);
                
                //update roles
                var existingRoles = await _userManager.GetRolesAsync(existingUser);
                await UpdateRolesAsync(existingRoles.ToList(), user.Roles.ToList(), updatedUser);
            }
        }
   
        private async Task UpdateRolesAsync(List<string> existingRoles, List<string> newRoles, ApplicationUser user)
        {
            var rolesToAdd = newRoles.Where(x => !existingRoles.Contains(x));
            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            var rolesToRemove = existingRoles.Where(x => !newRoles.Contains(x));
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }
        }

        private async Task<User> PopulateClaimsAndRoles(User user, ApplicationUser obj)
        {
            user.Roles = await _userManager.GetRolesAsync(obj);
            return user;
        }


        public string GenerateCode()
        {
            var lastRecord = _userRepository.GetAll().Select(x => x.Code).OrderByDescending(x => x).FirstOrDefault();
            var count = !string.IsNullOrWhiteSpace(lastRecord) ? int.Parse(lastRecord.Substring(2)) + 1 : 1;
            return $"CO{count:D8}";
        }

        public RegistrationUsers? GetByCorrelationId(Guid correlationId)
        {
            return _userRepository.GetByCorrelationId(correlationId).Result;
        }

        public async Task CreateAsync(RegistrationUsers data)
        {
            await Register(data);

            Console.WriteLine("CreateAsync done");
        }

        public Task UpdateAsync(RegistrationUsers data)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<AccountResultModel> Login(LoginModel credential, HttpContext httpContext)
        {
            var checkIfUserExists = await _userManager.FindByEmailAsync(credential.Email ??= string.Empty);
            if (checkIfUserExists?.Email == null || string.IsNullOrEmpty(credential.Password) || string.IsNullOrEmpty(credential.Email))
            {
                _logger.LogWarning($"User not found {credential.Email} ");
                return new AccountResultModel(401, "Login failed");
            }

            var validateResult = await _signInManager.CheckPasswordSignInAsync(checkIfUserExists,
                credential.Password, true);

            if (validateResult.RequiresTwoFactor)
            {
                _logger.LogWarning("MFA is enabled");
                return new AccountResultModel() { RequiresTwoFactor = true };
            }

            if (validateResult.IsLockedOut)
            {
                return new AccountResultModel(423, "To many wrong passwords/emails try again later");
            }

            if (validateResult.IsNotAllowed)
            {
                return new AccountResultModel(409, "Should confirm email first");
            }

            if (!validateResult.Succeeded)
            {
                return new AccountResultModel(401, "Login failed");
            }

            var claims = await _userManager.GetClaimsAsync(checkIfUserExists);

            var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = false,

                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(10),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                IsPersistent = credential.RememberMe ??= true,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            await httpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (credential.ReturnUrl != null)
            {
                var url = $"{credential.ReturnUrl}&";
                url += $"redirect_uri={credential.redirect_uri}&";
                url += $"response_type={credential.response_type}&";
                url += $"state={credential.state}&";
                url += $"scope={credential.scope}&";
                url += $"code_challenge={credential.code_challenge}&";
                url += $"code_challenge_method={credential.code_challenge_method}&";
                url += $"response_mode={credential.response_mode}&";
                url += $"nonce={credential.nonce}";

                return new AccountResultModel()
                {
                    LocalRedirectUrl = url
                };
            }

            return new AccountResultModel(200, "Login Ok but Missing ReturnUrl");
        }

        public async Task<AccountResultModel> Register(RegisterUserModel model)
        {
            var checkIfUserExists = await _userManager.FindByEmailAsync(model.EmailAddress);
            if (checkIfUserExists?.Email != null)
            {
                _logger.LogInformation("user already exists");
                return new AccountResultModel(400, "User already exists");
            }

            if (string.IsNullOrEmpty(model.FirstName))
            {
                model.FirstName = model.EmailAddress;
            } 
            if (string.IsNullOrEmpty(model.LastName))
            {
                model.LastName = model.EmailAddress;
            }
            
            if (string.IsNullOrEmpty(model.State))
            {
                model.State = "Created";
            }

            if (string.IsNullOrEmpty(model.Code))
            {
                model.Code = GenerateCode();
            }

            // Create the user 
            var user = _mapper.Map<RegisterUserModel, ApplicationUser>(model);
            user.CorrelationId = model.CorrelationId;

            // DEBUG CODE!
            if (model.EmailAddress.Contains("@fail.com"))
            {
                throw new Exception("Failed to create user"); 
            }
            // END DEBUG CODE!
            
            var result = !string.IsNullOrEmpty(model.Password) ? await _userManager.CreateAsync(user, model.Password) : await _userManager.CreateAsync(user);
            var addedUser = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (!result.Succeeded || addedUser == null)
            {
                _logger.LogError($"user creation failed");
                var message = "User creation failed ";
                foreach (var identityError in result.Errors)
                {
                    _logger.LogError(identityError.Code + identityError.Description);
                    message += identityError.Description;
                }
                return new AccountResultModel(400, message);
            }

            //TODO For now only one role in registration
            // await _userManager.AddToRoleAsync(addedUser, model.Role);
            //foreach(var userRole in model.Roles)
            //{
            //    await _userManager.AddToRoleAsync(addedUser, userRole.Name);
            //}

            var defaultClaims = new List<Claim>()
            {
                //TODO change to openIdDict claims?
                new Claim(ClaimTypes.NameIdentifier, user.Email), // NameIdentifier is used to check
                new Claim(ClaimTypes.Email, user.Email),
            };

            await _userManager.AddClaimsAsync(addedUser, defaultClaims);

            await ApproveDefaultClient(user);

            _logger.LogInformation("registration done for user {0}", user.Email);
            Guid.TryParse(addedUser.Id, out var userId);

            return new AccountResultModel(200, "registration done", userId);
        }

        private async Task ApproveDefaultClient(ApplicationUser user)
        {
            if (string.IsNullOrEmpty(_gatewayClientId))
            {
                _logger.LogInformation("gatewayClientId is null, so ignore approving for newly added user");
                return;
            }

            var application = await _applicationManager.FindByClientIdAsync(_gatewayClientId);
            if (application is OpenIddictEntityFrameworkCoreApplication coreApplication)
            {
                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                var clientId = coreApplication.Id;
                if (string.IsNullOrEmpty(clientId))
                {
                    _logger.LogInformation("clientId is null");
                }
                else
                {
                    // Add claim for default gateway client for the current roles
                    var authorization = await _authorizationManager.CreateAsync(
                        principal: principal,
                        subject: await _userManager.GetUserIdAsync(user),
                        client: clientId,
                        type: OpenIddictConstants.AuthorizationTypes.Permanent,
                        scopes: principal.GetScopes());

                    principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                    foreach (var claim in principal.Claims)
                    {
                        claim.SetDestinations(AuthorizationController.GetDestinations(claim, principal));
                    }

                    await _authorizationManager.UpdateAsync(authorization);
                }
            }
        }
    }
}
