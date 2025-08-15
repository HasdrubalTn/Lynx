using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure pipeline
app.MapControllers();
app.MapGet("/", () => "Lynx ApiGateway");

app.Run();
