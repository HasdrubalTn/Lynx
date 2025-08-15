// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using IdentityService.Configuration;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Add Entity Framework
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? "Host=localhost;Port=5432;Database=lynx_identity;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add IdentityServer EF stores
var identityServerConnectionString = builder.Configuration["ConnectionStrings:IdentityServerConnection"]
    ?? connectionString;

builder.Services.AddDbContext<Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext>(options =>
    options.UseNpgsql(identityServerConnectionString));

builder.Services.AddDbContext<Duende.IdentityServer.EntityFramework.DbContexts.PersistedGrantDbContext>(options =>
    options.UseNpgsql(identityServerConnectionString));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings for development
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add IdentityServer
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
})
.AddConfigurationStore(options =>
{
    options.ConfigureDbContext = b => b.UseNpgsql(
        identityServerConnectionString,
        sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));
})
.AddOperationalStore(options =>
{
    options.ConfigureDbContext = b => b.UseNpgsql(
        identityServerConnectionString,
        sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));
    options.EnableTokenCleanup = true;
})
.AddAspNetIdentity<ApplicationUser>()
.AddProfileService<ProfileService>()
.AddDeveloperSigningCredential(); // Only for development!

// Add configuration services
builder.Services.AddScoped<IClientConfigurationService, ClientConfigurationService>();
builder.Services.AddScoped<IScopeConfigurationService, ScopeConfigurationService>();

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("admin");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddAuthentication()
    .AddLocalApi(options =>
    {
        options.ExpectedScope = "identity_admin";
    });

// Add CORS for SPA clients
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "https://app.lynx.com", "https://admin.lynx.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Lynx IdentityService (Duende) - OIDC Provider Ready");

// Seed data in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configContext = scope.ServiceProvider.GetRequiredService<Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext>();
        var persistedGrantContext = scope.ServiceProvider.GetRequiredService<Duende.IdentityServer.EntityFramework.DbContexts.PersistedGrantDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Ensure databases are created
        await context.Database.EnsureCreatedAsync();
        await configContext.Database.EnsureCreatedAsync();
        await persistedGrantContext.Database.EnsureCreatedAsync();

        // Seed initial IdentityServer configuration
        await SeedData.SeedIdentityServerDataAsync(configContext, logger);

        // Seed admin user
        await SeedData.SeedAdminUserAsync(userManager, roleManager, logger);
    }
}

app.Run();
