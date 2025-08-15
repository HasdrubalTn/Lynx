// <copyright file="DPoPValidationResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Models;

using System.Security.Claims;

/// <summary>
/// Result of DPoP (Demonstration of Proof-of-Possession) token and proof validation.
/// Used by DPoP validation middleware to communicate validation outcomes.
/// </summary>
public sealed record DPoPValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the DPoP proof and token binding are valid.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Gets the claims principal if validation succeeded, null if failed.
    /// Contains user claims and DPoP-specific claims like thumbprint.
    /// </summary>
    public ClaimsPrincipal? ClaimsPrincipal { get; init; }

    /// <summary>
    /// Gets the error message if validation failed, null if succeeded.
    /// Used to provide specific feedback about what went wrong.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the DPoP key thumbprint from the validated proof.
    /// Used for token binding verification.
    /// </summary>
    public string? DPoPThumbprint { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="claimsPrincipal">The validated user claims.</param>
    /// <param name="dpopThumbprint">The DPoP key thumbprint.</param>
    /// <returns>A successful validation result.</returns>
    public static DPoPValidationResult Success(ClaimsPrincipal claimsPrincipal, string? dpopThumbprint = null)
    {
        return new DPoPValidationResult
        {
            IsValid = true,
            ClaimsPrincipal = claimsPrincipal,
            ErrorMessage = null,
            DPoPThumbprint = dpopThumbprint,
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errorMessage">The reason for validation failure.</param>
    /// <returns>A failed validation result.</returns>
    public static DPoPValidationResult Failure(string errorMessage)
    {
        return new DPoPValidationResult
        {
            IsValid = false,
            ClaimsPrincipal = null,
            ErrorMessage = errorMessage,
            DPoPThumbprint = null,
        };
    }
}
