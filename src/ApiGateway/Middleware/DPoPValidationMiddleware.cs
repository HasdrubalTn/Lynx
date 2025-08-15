// <copyright file="DPoPValidationMiddleware.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Middleware;

using System;
using System.Threading.Tasks;
using ApiGateway.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Middleware for validating DPoP (Demonstration of Proof-of-Possession) tokens.
/// Ensures that access tokens are properly bound to the client's public key.
/// </summary>
public class DPoPValidationMiddleware
{
    private readonly RequestDelegate next;
    private readonly IDPoPTokenValidator dpopValidator;
    private readonly ILogger<DPoPValidationMiddleware> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DPoPValidationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="dpopValidator">The DPoP token validator service.</param>
    /// <param name="logger">The logger instance.</param>
    public DPoPValidationMiddleware(
        RequestDelegate next,
        IDPoPTokenValidator dpopValidator,
        ILogger<DPoPValidationMiddleware> logger)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
        this.dpopValidator = dpopValidator ?? throw new ArgumentNullException(nameof(dpopValidator));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to validate DPoP tokens.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate DPoP for requests with Authorization header
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            await this.next(context);
            return;
        }

        var authorization = context.Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await this.next(context);
            return;
        }

        var accessToken = authorization["Bearer ".Length..].Trim();

        // Check if DPoP header is present
        if (!context.Request.Headers.TryGetValue("DPoP", out var dpopHeader) || string.IsNullOrEmpty(dpopHeader))
        {
            // For DPoP-bound tokens, DPoP proof is required
            if (accessToken.Contains("dpop", StringComparison.OrdinalIgnoreCase))
            {
                this.logger.LogWarning("DPoP proof required for DPoP-bound access token");
                context.Response.StatusCode = 401;
                context.Response.Headers.Append("WWW-Authenticate", "DPoP error=\"invalid_request\" error_description=\"DPoP proof required\"");
                await context.Response.WriteAsync("DPoP proof required");
                return;
            }

            // For regular Bearer tokens, DPoP is optional
            this.logger.LogDebug("No DPoP header found, skipping DPoP validation");
            await this.next(context);
            return;
        }

        var dpopProof = dpopHeader.ToString();
        var httpMethod = context.Request.Method;
        var uri = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";

        using var scope = this.logger.BeginScope("Method:{Method}, Uri:{Uri}", httpMethod, uri);

        try
        {
            this.logger.LogDebug("Validating DPoP proof token");

            var validationResult = await this.dpopValidator.ValidateAsync(
                dpopProof,
                accessToken,
                httpMethod,
                uri,
                context.RequestAborted);

            if (!validationResult.IsValid)
            {
                this.logger.LogWarning("DPoP validation failed: {Error}", validationResult.ErrorMessage);

                context.Response.StatusCode = 401;
                context.Response.Headers.Append("WWW-Authenticate", $"DPoP error=\"invalid_dpop_proof\" error_description=\"{validationResult.ErrorMessage}\"");
                await context.Response.WriteAsync($"DPoP validation failed: {validationResult.ErrorMessage}");
                return;
            }

            this.logger.LogDebug("DPoP validation successful");

            // Set the authenticated user principal on the context
            if (validationResult.ClaimsPrincipal != null)
            {
                context.User = validationResult.ClaimsPrincipal;
            }

            // Add validation result to context for potential use by downstream middleware/controllers
            context.Items["DPoPValidationResult"] = validationResult;

            await this.next(context);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during DPoP validation");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error during DPoP validation");
        }
    }
}
