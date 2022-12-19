using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ACME.Library.Common.Middlewares
{
    public class SwaggerHomeRedirectMiddleware
    {
        private readonly Microsoft.AspNetCore.Http.RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public SwaggerHomeRedirectMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }
        
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value == "/")
            {
                context.Response.Redirect("/swagger/index.html");
                return;
            }

            await _next.Invoke(context);
        }
    }
}
