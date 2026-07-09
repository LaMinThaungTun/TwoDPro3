using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Middlewares;
using TwoDPro3.Services;

var builder = WebApplication.CreateBuilder(args);


// ----------------------
// Connection string (Neon)
// ----------------------

var connectionString =
    Environment.GetEnvironmentVariable("NEON_CONN_STRING")
    ?? builder.Configuration.GetConnectionString("CalendarContext");


// ----------------------
// Database
// ----------------------

builder.Services.AddDbContext<CalendarContext>(options =>
    options.UseNpgsql(connectionString));


// ----------------------
// Services
// ----------------------

builder.Services.AddHostedService<MembershipExpiryService>();

builder.Services.AddScoped<MembershipService>();

builder.Services.AddScoped<TelegramOtpService>();

builder.Services.AddScoped<AgentService>();

builder.Services.AddHttpClient<TwilioVerifyService>();


// ----------------------
// MVC / Swagger
// ----------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();



var app = builder.Build();


// ----------------------
// Swagger
// ----------------------

app.UseSwagger();

app.UseSwaggerUI();


// ----------------------
// Middleware
// ----------------------

app.UseHttpsRedirection();

//app.UseMiddleware<AppVersionMiddleware>();

app.UseAuthorization();

app.MapControllers();


app.Run();