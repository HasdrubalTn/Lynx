// <copyright file="DPoPValidationMiddlewareExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Middleware;

using Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for registering DPoP validation middleware.
/// </summary>
public static class DPoPValidationMiddlewareExtensions
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
