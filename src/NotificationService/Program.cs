var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app = builder.Build();
app.MapGet("/", () => "Lynx NotificationService");
app.MapHealthChecks("/health");
app.Run();
