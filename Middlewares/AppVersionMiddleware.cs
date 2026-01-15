using System.Net;

namespace TwoDPro3.Middlewares
{
    public class AppVersionMiddleware
    {
        private readonly RequestDelegate _next;

        // 🔒 Minimum allowed app version
        private const string MIN_SUPPORTED_VERSION = "1.0.5";

        public AppVersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // ✅ Allow Swagger & static files
            if (path != null &&
                (path.StartsWith("/swagger") ||
                 path.StartsWith("/favicon") ||
                 path.StartsWith("/index") ||
                 path == "/"))
            {
                await _next(context);
                return;
            }

            // ✅ Allow health & admin routes (optional)
            if (path != null && (path.Contains("/health") || path.Contains("/admin")))
            {
                await _next(context);
                return;
            }

            // 🔍 Require X-App-Version header
            if (!context.Request.Headers.TryGetValue("X-App-Version", out var clientVersion))
            {
                context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                await context.Response.WriteAsync(
                    "App version required. Please update your app.");
                return;
            }

            // 🔢 Check version validity
            if (!IsVersionAllowed(clientVersion!))
            {
                context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                await context.Response.WriteAsync(
                    "Your app version is outdated. Please update.");
                return;
            }

            // ✅ Version OK → continue
            await _next(context);
        }



        private bool IsVersionAllowed(string clientVersion)
        {
            try
            {
                var client = new Version(clientVersion);
                var minimum = new Version(MIN_SUPPORTED_VERSION);

                return client >= minimum;
            }
            catch
            {
                // ❌ Invalid version format
                return false;
            }
        }
    }
}
