// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// NOTE: Configure Duende IdentityServer here per license requirements
var app = builder.Build();

// Configure pipeline
app.MapControllers();
app.MapGet("/", () => "Lynx IdentityService (Duende)");

app.Run();
