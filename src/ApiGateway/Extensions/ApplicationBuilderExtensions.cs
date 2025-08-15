// <copyright file="ApplicationBuilderExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Extensions;

using ApiGateway.Middleware;
using Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for IApplicationBuilder.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds DPoP validation middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseDPoPValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DPoPValidationMiddleware>();
    }
}
