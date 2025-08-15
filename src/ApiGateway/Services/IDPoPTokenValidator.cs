// <copyright file="IDPoPTokenValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Services;

using System.Threading;
using System.Threading.Tasks;
using ApiGateway.Models;

/// <summary>
/// Service for validating DPoP (Demonstration of Proof-of-Possession) tokens.
/// Provides security validation for token binding and replay attack prevention.
/// </summary>
public interface IDPoPTokenValidator
{
    /// <summary>
    /// Validates a DPoP proof token against the access token and HTTP context.
    /// </summary>
    /// <param name="dpopProof">The DPoP proof JWT token from the DPoP header.</param>
    /// <param name="accessToken">The access token being validated.</param>
    /// <param name="httpMethod">The HTTP method of the request.</param>
    /// <param name="uri">The URI of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A validation result indicating success or failure with details.</returns>
    Task<DPoPValidationResult> ValidateAsync(
        string dpopProof,
        string accessToken,
        string httpMethod,
        string uri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a DPoP proof token has been replayed (used before).
    /// </summary>
    /// <param name="jti">The unique identifier (jti) from the DPoP proof token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the token has been replayed, false otherwise.</returns>
    Task<bool> IsReplayedTokenAsync(string jti, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a DPoP proof token JTI to prevent replay attacks.
    /// </summary>
    /// <param name="jti">The unique identifier (jti) from the DPoP proof token.</param>
    /// <param name="expiry">The expiration time of the token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreTokenAsync(string jti, System.DateTimeOffset expiry, CancellationToken cancellationToken = default);
}
