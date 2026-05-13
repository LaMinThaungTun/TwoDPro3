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

            // --------------------------------------------------
            // 🔓 ALWAYS ALLOW PUBLIC / EXTERNAL ENDPOINTS
            // --------------------------------------------------

            // Swagger UI + swagger.json + swagger assets
            if (path.StartsWith("/swagger"))
            {
                await _next(context);
                return;
            }

            // Root + favicon
            if (path == "/" || path == "/favicon.ico")
            {
                await _next(context);
                return;
            }

            // Health/Admin endpoints
            if (path.StartsWith("/health") ||
                path.StartsWith("/admin"))
            {
                await _next(context);
                return;
            }

            // Telegram webhook
            if (path.StartsWith("/api/telegram/webhook"))
            {
                await _next(context);
                return;
            }

            // OPTIONAL:
            // Allow Twilio callbacks if needed later
            if (path.StartsWith("/api/twilio"))
            {
                await _next(context);
                return;
            }

            // --------------------------------------------------
            // 🔒 REQUIRE APP VERSION FOR REAL APP REQUESTS
            // --------------------------------------------------

            if (!context.Request.Headers.TryGetValue(
                    "X-App-Version",
                    out var clientVersion))
            {
                context.Response.StatusCode =
                    StatusCodes.Status426UpgradeRequired;

                await context.Response.WriteAsync(
                    "App version required. Please update your app.");

                return;
            }

            // --------------------------------------------------
            // 🔒 VERSION VALIDATION
            // --------------------------------------------------

            if (!IsVersionAllowed(clientVersion!))
            {
                context.Response.StatusCode =
                    StatusCodes.Status426UpgradeRequired;

                await context.Response.WriteAsync(
                    "Your app version is outdated. Please update.");

                return;
            }

            // --------------------------------------------------
            // NEXT
            // --------------------------------------------------

            await _next(context);
        }

        private bool IsVersionAllowed(string clientVersion)
        {
            try
            {
                return new Version(clientVersion)
                    >= new Version(MIN_SUPPORTED_VERSION);
            }
            catch
            {
                return false;
            }
        }
    }
}