var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

var app = builder.Build();

// Configure pipeline
app.MapControllers();
app.MapGet("/", () => "Lynx NotificationService");

app.Run();
