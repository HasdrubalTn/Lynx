var builder = WebApplication.CreateBuilder(args);
// NOTE: Minimal placeholder. Configure Duende IdentityServer here per license.
builder.Services.AddHealthChecks();
var app = builder.Build();
app.MapGet("/", () => "Lynx IdentityService (Duende)");
app.MapHealthChecks("/health");
app.Run();
