using Microsoft.AspNetCore.Mvc;
using ACME.Identity.Security;

namespace ACME.Identity.Controllers;

public class HomeController : Controller
{
    private readonly string _clientApp;

    public HomeController()
    {
        var baseDirectory = Directory.GetCurrentDirectory();
        _clientApp = Path.Combine(baseDirectory,
            "ClientApp", "out", "index.html");
    }
    
    [HttpGet("/")]
    [HttpGet("/login")]
    [HttpGet("/account/login")]
    public IActionResult Index()
    {
        SecurityHeadersMiddleware.AppendSecurityHeaders(HttpContext);
        return PhysicalFile(_clientApp, "text/html");
    }
}