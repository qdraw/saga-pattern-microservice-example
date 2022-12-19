using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using ACME.Identity.Security;
using System.Globalization;

namespace ACME.Identity.Helpers;

public static class StaticServeHelper
{
    public static void Serve(WebApplication app)
    {
        void PrepareResponse(StaticFileResponseContext ctx)
        {
            // Cache static files for 356 days
            ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddDays(365)
                .ToString("R", CultureInfo.InvariantCulture));
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");

            SecurityHeadersMiddleware.AppendSecurityHeaders(ctx.Context);
        }

        var staticFolder = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "out", "_next", "static");
        if (Directory.Exists(staticFolder) )
        {
            app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = PrepareResponse,
                    FileProvider = new PhysicalFileProvider(
                        staticFolder),
                    RequestPath = $"/_next/static",
                }
            );
        }
    }
}