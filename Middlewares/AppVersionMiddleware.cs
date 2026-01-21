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
            var path = context.Request.Path.Value?.ToLower();

            //Console.WriteLine($"Request path: '{context.Request.Path.Value}'");

            // ✅ ALWAYS allow Swagger (Render-safe)
            if (!string.IsNullOrEmpty(path) && path.Contains("swagger"))
            {
                await _next(context);
                return;
            }

            // ✅ Allow root & favicon (Swagger NEEDS these)
            if (path == "/" || path == "/favicon.ico")
            {
                await _next(context);
                return;
            }  

            // ✅ Allow health/admin
            if (!string.IsNullOrEmpty(path) &&
                (path.Contains("health") || path.Contains("admin")))
            {
                await _next(context);
                return;
            }

            // 🔒 Require app version for real app only
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
