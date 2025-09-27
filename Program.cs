using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Read connection string
// Try environment variable first (for deployment), fallback to appsettings.json
var connectionString = Environment.GetEnvironmentVariable("NEON_CONN_STRING")
                       ?? builder.Configuration.GetConnectionString("CalendarContext");

// ----------------------
// Register DbContext with Neon
builder.Services.AddDbContext<CalendarContext>(options =>
    options.UseNpgsql(connectionString));

// ----------------------
// Standard API setup
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
