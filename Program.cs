using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Services;
using TwoDPro3.Middlewares;

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

builder.Services.AddHostedService<MembershipExpiryService>();
builder.Services.AddScoped<MembershipService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ----------------------
// Dev tools
app.UseSwagger();
app.UseSwaggerUI();


// ----------------------
// Middleware pipeline (ORDER MATTERS)
app.UseHttpsRedirection();

// 🔐 VERSION PROTECTION — RUNS BEFORE ALL CONTROLLERS
app.UseMiddleware<AppVersionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
