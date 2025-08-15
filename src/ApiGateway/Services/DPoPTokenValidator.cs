// <copyright file="DPoPTokenValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Services;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

/// <summary>
/// Service implementation for validating DPoP (Demonstration of Proof-of-Possession) tokens.
/// Provides comprehensive security validation including token binding and replay protection.
/// </summary>
public class DPoPTokenValidator : IDPoPTokenValidator
{
    private readonly IMemoryCache replayCache;
    private readonly ILogger<DPoPTokenValidator> logger;
    private readonly JwtSecurityTokenHandler tokenHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DPoPTokenValidator"/> class.
    /// </summary>
    /// <param name="replayCache">The memory cache for replay protection.</param>
    /// <param name="logger">The logger instance.</param>
    public DPoPTokenValidator(IMemoryCache replayCache, ILogger<DPoPTokenValidator> logger)
    {
        this.replayCache = replayCache ?? throw new ArgumentNullException(nameof(replayCache));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <summary>
    /// Calculates the JWK thumbprint for a given RSA key.
    /// </summary>
    /// <param name="rsa">The RSA key.</param>
    /// <returns>The JWK thumbprint as a base64url-encoded string.</returns>
    public static string CalculateJwkThumbprint(RSA rsa)
    {
        var parameters = rsa.ExportParameters(false);

        var jwk = new
        {
            kty = "RSA",
            n = Base64UrlEncoder.Encode(parameters.Modulus),
            e = Base64UrlEncoder.Encode(parameters.Exponent),
        };

        var json = JsonSerializer.Serialize(jwk, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Base64UrlEncoder.Encode(hash);
    }

    /// <inheritdoc/>
    public async Task<DPoPValidationResult> ValidateAsync(
        string dpopProof,
        string accessToken,
        string httpMethod,
        string uri,
        CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("HttpMethod:{HttpMethod}, Uri:{Uri}", httpMethod, uri);

        try
        {
            this.logger.LogDebug("Validating DPoP proof token");

            // Parse the DPoP proof token
            if (!this.tokenHandler.CanReadToken(dpopProof))
            {
                this.logger.LogWarning("Invalid DPoP proof token format");
                return DPoPValidationResult.Failure("Invalid DPoP proof token format");
            }

            var dpopToken = this.tokenHandler.ReadJwtToken(dpopProof);

            // Validate token structure and required claims
            var structureValidation = this.ValidateTokenStructure(dpopToken, httpMethod, uri);
            if (!structureValidation.IsValid)
            {
                return structureValidation;
            }

            // Check for replay attacks
            var jti = dpopToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                this.logger.LogWarning("Missing jti claim in DPoP proof");
                return DPoPValidationResult.Failure("Missing jti claim in DPoP proof");
            }

            if (await this.IsReplayedTokenAsync(jti, cancellationToken))
            {
                this.logger.LogWarning("DPoP proof token replay detected");
                return DPoPValidationResult.Failure("DPoP proof token has been replayed");
            }

            // Validate the token signature
            var publicKey = this.ExtractPublicKey(dpopToken);
            if (publicKey == null)
            {
                this.logger.LogWarning("Failed to extract public key from DPoP proof");
                return DPoPValidationResult.Failure("Invalid public key in DPoP proof");
            }

            var signatureValidation = await this.ValidateTokenSignatureAsync(dpopProof, publicKey, cancellationToken);
            if (!signatureValidation.IsValid)
            {
                return signatureValidation;
            }

            // Validate token binding with access token
            var bindingValidation = this.ValidateTokenBinding(dpopToken, accessToken, publicKey);
            if (!bindingValidation.IsValid)
            {
                return bindingValidation;
            }

            // Store the jti for replay protection
            var expiry = dpopToken.ValidTo;
            await this.StoreTokenAsync(jti, expiry, cancellationToken);

            // Calculate thumbprint for success result
            var thumbprint = CalculateJwkThumbprint(publicKey);

            this.logger.LogDebug("DPoP proof validation successful");
            return DPoPValidationResult.Success(new ClaimsPrincipal(), thumbprint);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error validating DPoP proof token");
            return DPoPValidationResult.Failure("Internal validation error");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsReplayedTokenAsync(string jti, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async compliance

        var cacheKey = $"dpop_jti_{jti}";
        return this.replayCache.TryGetValue(cacheKey, out _);
    }

    /// <inheritdoc/>
    public async Task StoreTokenAsync(string jti, DateTimeOffset expiry, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async compliance

        var cacheKey = $"dpop_jti_{jti}";
        var cacheExpiry = expiry.AddMinutes(5); // Add buffer to prevent race conditions

        this.replayCache.Set(cacheKey, true, cacheExpiry);
        this.logger.LogDebug("Stored DPoP jti for replay protection until {Expiry}", cacheExpiry);
    }

    private DPoPValidationResult ValidateTokenStructure(JwtSecurityToken dpopToken, string httpMethod, string uri)
    {
        // Validate required header claims
        if (dpopToken.Header.Typ != "dpop+jwt")
        {
            this.logger.LogWarning("Invalid DPoP token type. Expected: dpop+jwt, Actual: {Type}", dpopToken.Header.Typ);
            return DPoPValidationResult.Failure("Invalid DPoP token type");
        }

        if (dpopToken.Header.Alg != "RS256" && dpopToken.Header.Alg != "ES256")
        {
            this.logger.LogWarning("Unsupported DPoP algorithm: {Algorithm}", dpopToken.Header.Alg);
            return DPoPValidationResult.Failure("Unsupported DPoP algorithm");
        }

        // Validate required payload claims
        var requiredClaims = new[] { JwtRegisteredClaimNames.Jti, JwtRegisteredClaimNames.Iat, "htm", "htu" };
        var missingClaims = requiredClaims.Where(claim => !dpopToken.Claims.Any(c => c.Type == claim)).ToList();

        if (missingClaims.Any())
        {
            this.logger.LogWarning("Missing required claims: {Claims}", string.Join(", ", missingClaims));
            return DPoPValidationResult.Failure($"Missing required claims: {string.Join(", ", missingClaims)}");
        }

        // Validate HTTP method (htm)
        var htmClaim = dpopToken.Claims.FirstOrDefault(c => c.Type == "htm")?.Value;
        if (!string.Equals(htmClaim, httpMethod, StringComparison.OrdinalIgnoreCase))
        {
            this.logger.LogWarning("HTTP method mismatch. Expected: {Expected}, Actual: {Actual}", httpMethod, htmClaim);
            return DPoPValidationResult.Failure("HTTP method mismatch");
        }

        // Validate HTTP URI (htu)
        var htuClaim = dpopToken.Claims.FirstOrDefault(c => c.Type == "htu")?.Value;
        if (!string.Equals(htuClaim, uri, StringComparison.OrdinalIgnoreCase))
        {
            this.logger.LogWarning("HTTP URI mismatch. Expected: {Expected}, Actual: {Actual}", uri, htuClaim);
            return DPoPValidationResult.Failure("HTTP URI mismatch");
        }

        // Validate timestamp (iat should be recent)
        var iatClaim = dpopToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
        if (long.TryParse(iatClaim, out var iat))
        {
            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat);
            var now = DateTimeOffset.UtcNow;
            var maxAge = TimeSpan.FromMinutes(5); // Allow 5-minute window

            if (issuedAt > now.Add(TimeSpan.FromMinutes(1)) || issuedAt < now.Subtract(maxAge))
            {
                this.logger.LogWarning("DPoP token timestamp out of acceptable range. IssuedAt: {IssuedAt}, Now: {Now}", issuedAt, now);
                return DPoPValidationResult.Failure("DPoP token timestamp out of acceptable range");
            }
        }

        // Return a minimal success without ClaimsPrincipal since this is just structure validation
        return new DPoPValidationResult { IsValid = true };
    }

    private RSA? ExtractPublicKey(JwtSecurityToken dpopToken)
    {
        try
        {
            if (!dpopToken.Header.TryGetValue("jwk", out var jwkValue) || jwkValue is not JsonElement jwkElement)
            {
                this.logger.LogWarning("Missing or invalid jwk claim in DPoP header");
                return null;
            }

            var jwkJson = jwkElement.GetRawText();
            var jwk = JsonSerializer.Deserialize<Dictionary<string, object>>(jwkJson);

            if (jwk == null || !jwk.TryGetValue("kty", out var ktyValue) || ktyValue?.ToString() != "RSA")
            {
                this.logger.LogWarning("Invalid or unsupported key type in JWK");
                return null;
            }

            if (!jwk.TryGetValue("n", out var nValue) || !jwk.TryGetValue("e", out var eValue))
            {
                this.logger.LogWarning("Missing RSA parameters in JWK");
                return null;
            }

            var rsa = RSA.Create();
            var parameters = new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(nValue.ToString()!),
                Exponent = Base64UrlEncoder.DecodeBytes(eValue.ToString()!),
            };

            rsa.ImportParameters(parameters);
            return rsa;
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Error extracting public key from DPoP token");
            return null;
        }
    }

    private async Task<DPoPValidationResult> ValidateTokenSignatureAsync(string dpopProof, RSA publicKey, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // For async compliance

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false, // We validate this separately
                IssuerSigningKey = new RsaSecurityKey(publicKey),
                ValidateIssuerSigningKey = true,
            };

            this.tokenHandler.ValidateToken(dpopProof, validationParameters, out _);
            return new DPoPValidationResult { IsValid = true };
        }
        catch (SecurityTokenException ex)
        {
            this.logger.LogWarning(ex, "DPoP token signature validation failed");
            return DPoPValidationResult.Failure("Invalid DPoP token signature");
        }
    }

    private DPoPValidationResult ValidateTokenBinding(JwtSecurityToken dpopToken, string accessToken, RSA publicKey)
    {
        try
        {
            // Extract the cnf claim from the access token
            var accessJwt = this.tokenHandler.ReadJwtToken(accessToken);
            var cnfClaim = accessJwt.Claims.FirstOrDefault(c => c.Type == "cnf")?.Value;

            if (string.IsNullOrEmpty(cnfClaim))
            {
                this.logger.LogWarning("Missing cnf claim in access token");
                return DPoPValidationResult.Failure("Missing cnf claim in access token");
            }

            // Parse the cnf claim to get the jkt (JWK thumbprint)
            var cnf = JsonSerializer.Deserialize<Dictionary<string, object>>(cnfClaim);
            if (cnf == null || !cnf.TryGetValue("jkt", out var jktValue))
            {
                this.logger.LogWarning("Missing jkt in cnf claim");
                return DPoPValidationResult.Failure("Missing jkt in cnf claim");
            }

            // Calculate the thumbprint of the DPoP public key
            var publicKeyThumbprint = CalculateJwkThumbprint(publicKey);
            var expectedThumbprint = jktValue.ToString();

            if (publicKeyThumbprint != expectedThumbprint)
            {
                this.logger.LogWarning("DPoP public key thumbprint mismatch. Expected: {Expected}, Actual: {Actual}", expectedThumbprint, publicKeyThumbprint);
                return DPoPValidationResult.Failure("DPoP public key thumbprint mismatch");
            }

            return new DPoPValidationResult { IsValid = true };
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Error validating DPoP token binding");
            return DPoPValidationResult.Failure("Token binding validation error");
        }
    }
}
