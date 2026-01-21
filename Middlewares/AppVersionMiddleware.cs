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
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 🔓 ALWAYS allow Swagger UI + assets + json
            if (path.StartsWith("/swagger"))
            {
                await _next(context);
                return;
            }

            // 🔓 Allow root & favicon (Swagger NEEDS these)
            if (path == "/" || path == "/favicon.ico")
            {
                await _next(context);
                return;
            }

            // 🔓 Allow health/admin endpoints
            if (path.StartsWith("/health") || path.StartsWith("/admin"))
            {
                await _next(context);
                return;
            }

            // 🔒 Require app version for ALL real API calls
            if (!context.Request.Headers.TryGetValue("X-App-Version", out var clientVersion))
            {
                context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                await context.Response.WriteAsync("App version required. Please update your app.");
                return;
            }

            if (!IsVersionAllowed(clientVersion!))
            {
                context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                await context.Response.WriteAsync("Your app version is outdated. Please update.");
                return;
            }

            await _next(context);
        }

        private bool IsVersionAllowed(string clientVersion)
        {
            try
            {
                return new Version(clientVersion) >= new Version(MIN_SUPPORTED_VERSION);
            }
            catch
            {
                return false;
            }
        }
    }
}
