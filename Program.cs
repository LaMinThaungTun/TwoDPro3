using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Middlewares;
using TwoDPro3.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Connection string (Neon)
var connectionString =
    Environment.GetEnvironmentVariable("NEON_CONN_STRING")
    ?? builder.Configuration.GetConnectionString("CalendarContext");

// ----------------------
// Services

builder.Services.AddDbContext<CalendarContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHostedService<MembershipExpiryService>();

builder.Services.AddScoped<MembershipService>();

builder.Services.AddScoped<TelegramOtpService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<TwilioVerifyService>();

var app = builder.Build();

// ----------------------
// Dev tools

app.UseSwagger();

app.UseSwaggerUI();

// ----------------------
// Middleware pipeline

app.UseHttpsRedirection();

// 🔐 VERSION PROTECTION
// app.UseMiddleware<AppVersionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();