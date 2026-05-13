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

            Console.WriteLine($"PATH: {path}");

            // --------------------------------------------------
            // PUBLIC ROUTES (NO VERSION CHECK)
            // --------------------------------------------------

            var publicRoutes = new[]
            {
                "/swagger",
                "/favicon.ico",
                "/health",
                "/admin",
                "/api/telegram/webhook"
            };

            // allow root
            if (path == "/")
            {
                await _next(context);
                return;
            }

            // allow public routes
            if (publicRoutes.Any(route => path.StartsWith(route)))
            {
                await _next(context);
                return;
            }

            // --------------------------------------------------
            // VERSION HEADER REQUIRED
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
            // VERSION VALIDATION
            // --------------------------------------------------

            if (!IsVersionAllowed(clientVersion!))
            {
                context.Response.StatusCode =
                    StatusCodes.Status426UpgradeRequired;

                await context.Response.WriteAsync(
                    "Your app version is outdated. Please update.");

                return;
            }

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