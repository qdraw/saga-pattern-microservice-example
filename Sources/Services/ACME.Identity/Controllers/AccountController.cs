using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ACME.Identity.Models;
using ACME.Identity.Services.Interfaces;
using System.Security.Claims;
using ACME.Library.Common.Models.Api;
using ACME.Library.Domain.Core;

namespace ACME.Identity.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    private readonly IUserService _userService;

    public AccountController(SignInManager<ApplicationUser> signInManager, IUserService userService)
    {
        _signInManager = signInManager;
        _userService = userService;
    }
    
    /// <summary>
    /// Login the current HttpContext in
    /// </summary>
    /// <param name="credential">Email, password and remember me bool</param>
    /// <returns>Login status</returns>
    /// <response code="200">successful login</response>
    /// <response code="401">login failed</response>
    /// <response code="405">ValidateAntiForgeryToken</response>
    /// <response code="423">login failed due lock</response>
    /// <response code="500">login failed due signIn errors</response>
    [HttpPost("/api/account/login")]
    [ProducesResponseType(typeof(string),200)]
    [ProducesResponseType(typeof(string),401)]
    [ProducesResponseType(typeof(string),405)]
    //[ValidateAntiForgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> LoginPost(LoginModel credential)
    {
        var loginResult = await _userService.Login(credential, HttpContext);
        if (loginResult.StatusCode >= 200)
        {
            Response.StatusCode = loginResult.StatusCode;
            return Json(loginResult.Message);
        }

        if (loginResult.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = credential.ReturnUrl, RememberMe = credential.RememberMe });
        }

        if (!string.IsNullOrEmpty(loginResult.LocalRedirectUrl))
        {
            return LocalRedirect(loginResult.LocalRedirectUrl);
        }
        
        return Json("Login Ok but Missing ReturnUrl");
    }
    
    /// <summary>
    /// Logout the current HttpContext out
    /// </summary>
    /// <returns></returns>
    /// <response code="200">successful logout</response>
    [HttpPost("/api/account/logout")]
    [ProducesResponseType(200)]
    public IActionResult LogoutJson()
    {
        _signInManager.SignOutAsync();

        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
        return Json("your logged out");
    }
    
    /// <summary>
    /// Register new user
    /// </summary>
    /// <returns></returns>
    /// <response code="200">successful register</response>
    [HttpPost("/api/account/register")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Register(RegisterUserModel model)
    {
        var registerResult = await _userService.Register(model);

        if (registerResult.StatusCode < 200)
        {
            return BadRequest();
        }

        Response.StatusCode = registerResult.StatusCode;
        return Json(registerResult.Message);
    }

    /// <summary>
    /// Get user info
    /// </summary>
    /// <returns></returns>
    /// <response code="200"></response>
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("/api/account/user-info"), Produces("application/json")]
    public async Task<ActionResult<ApiResponse<User>>> Userinfo()
    {
        Guid userId = Guid.Empty;
        ApiResponse response;
        if (Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out userId))
        {
            var user = await _userService.GetAsync(userId);
            response = ApiResponse<User>.Success(user);
            return Ok(response);
        }

        response = ApiResponse.Fail("No userId found.");
        return BadRequest(response);
    }

    /// <summary>
    /// Update user
    /// </summary>
    /// <returns></returns>
    /// <response code="200">successful logout</response>
    [HttpPost("/api/account/update")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Update(User model)
    {
        await _userService.Update(model);
        var response = ApiResponse.Success();
        return Ok(response);
    }
}