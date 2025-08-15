var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
// NOTE: Configure Duende IdentityServer here per license requirements

var app = builder.Build();

// Configure pipeline
app.MapControllers();
app.MapGet("/", () => "Lynx IdentityService (Duende)");

app.Run();
