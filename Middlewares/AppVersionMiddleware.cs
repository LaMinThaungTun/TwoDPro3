using System.Globalization;

namespace TwoDPro3.Middlewares
{
    public class AppVersionMiddleware
    {
        private readonly RequestDelegate _next;

        private const string MIN_SUPPORTED_VERSION = "1.0.5";

        public AppVersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            Console.WriteLine($"[MIDDLEWARE] PATH: {path}");

            // --------------------------------------------------
            // 🔓 PUBLIC ROUTES (NO VERSION CHECK)
            // --------------------------------------------------

            if (IsPublicRoute(path))
            {
                await _next(context);
                return;
            }

            // --------------------------------------------------
            // 🔒 VERSION HEADER REQUIRED FOR APP REQUESTS
            // --------------------------------------------------

            if (!context.Request.Headers.TryGetValue("X-App-Version", out var clientVersion))
            {
                context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                await context.Response.WriteAsync("App version required. Please update your app.");
                return;
            }

            if (!IsVersionValid(clientVersion!))
            {
                context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                await context.Response.WriteAsync("Your app version is outdated. Please update.");
                return;
            }

            await _next(context);
        }

        // --------------------------------------------------
        // PUBLIC ROUTES (SAFE FOR TELEGRAM + SWAGGER + ETC)
        // --------------------------------------------------

        private bool IsPublicRoute(string path)
        {
            return
                path.StartsWith("/swagger") ||
                path.StartsWith("/favicon.ico") ||
                path.StartsWith("/health") ||
                path.StartsWith("/admin") ||
                path.StartsWith("/api/telegram/webhook");
        }

        // --------------------------------------------------
        // VERSION CHECK
        // --------------------------------------------------

        private bool IsVersionValid(string clientVersion)
        {
            try
            {
                var client = Version.Parse(clientVersion);
                var min = Version.Parse(MIN_SUPPORTED_VERSION);

                return client >= min;
            }
            catch
            {
                return false;
            }
        }
    }
}