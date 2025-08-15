var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app = builder.Build();
app.MapGet("/", () => "Lynx ApiGateway");
app.MapHealthChecks("/health");
app.Run();
