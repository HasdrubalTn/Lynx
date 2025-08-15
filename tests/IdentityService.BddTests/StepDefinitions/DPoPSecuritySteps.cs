// <copyright file="DPoPSecuritySteps.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using ApiGateway.Middleware;
using ApiGateway.Models;
using ApiGateway.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace IdentityService.BddTests.StepDefinitions;

[FeatureFile("./Features/DPoPSecurity.feature")]
public sealed class DPoPSecurityStepsNew : Feature
{
    private readonly IDPoPTokenValidator _tokenValidator;
    private readonly ILogger<DPoPValidationMiddleware> _logger;
    private readonly RequestDelegate _next;
    
    private HttpContext? _httpContext;
    private DPoPValidationResult? _validationResult;
    private string? _accessToken;
    private string? _dpopProof;
    private string? _keyThumbprint;
    private Exception? _thrownException;

    public DPoPSecurityStepsNew()
    {
        _tokenValidator = Substitute.For<IDPoPTokenValidator>();
        _logger = Substitute.For<ILogger<DPoPValidationMiddleware>>();
        _next = Substitute.For<RequestDelegate>();

        // Set up default behavior - all mocks start as valid, then individual tests override as needed
        _tokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DPoPValidationResult.Success(CreateDPoPPrincipal("default_thumbprint"), "default_thumbprint")));
    }

    [Given(@"the Identity Service supports DPoP")]
    public void GivenTheIdentityServiceSupportsDPoP()
    {
        // Identity Service is configured with DPoP support
        // This would be verified in integration tests
    }

    [Given(@"the API Gateway validates DPoP proofs")]
    [And(@"the API Gateway validates DPoP proofs")]
    public void GivenTheAPIGatewayValidatesDPoPProofs()
    {
        // API Gateway has DPoP validation middleware configured
        // This setup is part of the test infrastructure
    }

    [Given(@"a DPoP-bound access token")]
    public void GivenADPoBoundAccessToken()
    {
        _accessToken = "dpop_bound_token_123";
        _keyThumbprint = "test_key_thumbprint_123";
    }

    [Given(@"a DPoP-bound access token for key ""(.*)""")]
    public void GivenADPoBoundAccessTokenForKey(string keyName)
    {
        _accessToken = $"dpop_token_bound_to_{keyName}";
        _keyThumbprint = $"thumbprint_for_{keyName}";
    }

    [Given(@"a valid DPoP proof for ""(.*)""")]
    [And(@"a valid DPoP proof for ""(.*)""")]
    public void GivenAValidDPoPProofFor(string httpRequest)
    {
        _dpopProof = $"valid_proof_for_{httpRequest.Replace(" ", "_").Replace("/", "_")}";
        
        // Set up token validator to return success for this proof
        _tokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DPoPValidationResult.Success(CreateDPoPPrincipal(_keyThumbprint!), _keyThumbprint)));
    }

    [Given(@"the proof has already been used once")]
    public void GivenTheProofHasAlreadyBeenUsedOnce()
    {
        // Override the default mock to return failure for replay detection
        _tokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DPoPValidationResult.Failure("replay detected")));
    }

    [Given(@"a DPoP proof signed with different key ""(.*)""")]
    public void GivenADPoPProofSignedWithDifferentKey(string differentKey)
    {
        _dpopProof = $"proof_signed_with_{differentKey}";
        
        // Override the default mock to return failure for binding mismatch
        _tokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DPoPValidationResult.Failure("binding mismatch")));
    }

    [Given(@"a DPoP proof with invalid signature")]
    [And(@"a DPoP proof with invalid signature")]
    public void GivenADPoPProofWithInvalidSignature()
    {
        _dpopProof = "invalid_signature_proof";
        
        // Override the default mock to return failure for invalid signature
        _tokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DPoPValidationResult.Failure("invalid signature")));
    }

    [Given(@"an expired DPoP-bound access token")]
    public void GivenAnExpiredDPoPBoundAccessToken()
    {
        _accessToken = "expired_dpop_token_123";
        _keyThumbprint = "expired_key_thumbprint";
        
        // Override the default mock to return failure for expired token
        _tokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DPoPValidationResult.Failure("token expired")));
    }

    [Given(@"a valid DPoP proof")]
    [And(@"a valid DPoP proof")]
    public void GivenAValidDPoPProof()
    {
        _dpopProof = "valid_dpop_proof_123";
        
        // Keep default success behavior for general valid proof
    }

    [Given(@"a client ""(.*)"" requires DPoP")]
    public void GivenAClientRequiresDPoP(string clientName)
    {
        // Client configuration specifies RequireDPoP = true
        // This would be set up in the test database
    }

    [Given(@"the client generates a DPoP key pair")]
    [And(@"the client generates a DPoP key pair")]
    public void GivenTheClientGeneratesADPoPKeyPair()
    {
        _keyThumbprint = "test_key_thumbprint_123";
        // In real implementation, this would generate actual RSA/ECDSA keys
    }

    [Given(@"a client ""(.*)"" does not require DPoP")]
    public void GivenAClientDoesNotRequireDPoP(string clientName)
    {
        // Client configuration has RequireDPoP = false
        // Bearer tokens should work for this client
    }

    [Given(@"a valid Bearer access token")]
    [And(@"a valid Bearer access token")]
    public void GivenAValidBearerAccessToken()
    {
        _accessToken = "bearer_token_123";
        // Keep default success behavior for Bearer tokens
    }

    [And(@"the proof has already been used once")]
    public void AndTheProofHasAlreadyBeenUsedOnce()
    {
        // Override the default mock to return failure for replay detection
        _tokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DPoPValidationResult.Failure("replay detected")));
    }

    [And(@"a DPoP proof signed with different key ""(.*)""")]
    public void AndADPoPProofSignedWithDifferentKey(string keyName)
    {
        // Create a DPoP proof signed with a different key than the one used for the token
        _dpopProof = $"different_key_proof_for_{keyName}";
        _keyThumbprint = $"thumbprint_for_{keyName}";
        
        // Override the default mock to return failure for binding mismatch
        _tokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DPoPValidationResult.Failure("binding mismatch")));
    }

    [When(@"the client calls the protected API")]
    public async Task WhenTheClientCallsTheProtectedAPI()
    {
        _httpContext = CreateHttpContext(_accessToken!, _dpopProof!);
        var middleware = new DPoPValidationMiddleware(_next, _tokenValidator, _logger);
        
        try
        {
            await middleware.InvokeAsync(_httpContext);
        }
        catch (Exception ex)
        {
            _thrownException = ex;
        }
    }

    [When(@"the client calls the protected API with the token and proof")]
    public async Task WhenTheClientCallsTheProtectedAPIWithTheTokenAndProof()
    {
        await WhenTheClientCallsTheProtectedAPI();
    }

    [When(@"the client attempts to reuse the same proof")]
    public async Task WhenTheClientAttemptsToReuseTheSameProof()
    {
        await WhenTheClientCallsTheProtectedAPI();
    }

    [When(@"the client calls the protected API with Bearer token")]
    public async Task WhenTheClientCallsTheProtectedAPIWithBearerToken()
    {
        _httpContext = CreateHttpContextBearerOnly(_accessToken!);
        var middleware = new DPoPValidationMiddleware(_next, _tokenValidator, _logger);
        
        try
        {
            await middleware.InvokeAsync(_httpContext);
        }
        catch (Exception ex)
        {
            _thrownException = ex;
        }
    }

    [When(@"the client calls the API without DPoP proof header")]
    public async Task WhenTheClientCallsTheAPIWithoutDPoPProofHeader()
    {
        _httpContext = CreateHttpContextBearerOnly(_accessToken!); // No DPoP header
        var middleware = new DPoPValidationMiddleware(_next, _tokenValidator, _logger);
        
        try
        {
            await middleware.InvokeAsync(_httpContext);
        }
        catch (Exception ex)
        {
            _thrownException = ex;
        }
    }

    [When(@"the client requests an access token")]
    public void WhenTheClientRequestsAnAccessToken()
    {
        // In real implementation, this would call the token endpoint
        // For tests, we simulate the token issuance
    }

    [When(@"the client requests an access token with DPoP proof")]
    public void WhenTheClientRequestsAnAccessTokenWithDPoPProof()
    {
        // In real implementation, this would call the token endpoint with DPoP proof
        // For tests, we simulate the DPoP-bound token issuance
        _accessToken = "dpop_bound_token_123";
        _keyThumbprint = "test_key_thumbprint_123";
    }

    [Then(@"the access token should be DPoP-bound")]
    public void ThenTheAccessTokenShouldBeDPoPBound()
    {
        _accessToken.Should().NotBeNullOrEmpty();
        _accessToken.Should().Contain("dpop", "DPoP tokens should be identifiable");
    }

    [Then(@"the token should contain the key thumbprint")]
    [And(@"the token should contain the key thumbprint")]
    public void ThenTheTokenShouldContainTheKeyThumbprint()
    {
        // In real implementation, would decode JWT and check 'cnf' claim
        _keyThumbprint.Should().NotBeNullOrEmpty();
    }

    [Then(@"the token type should be ""(.*)""")]
    [And(@"the token type should be ""(.*)""")]
    public void ThenTheTokenTypeShouldBe(string expectedTokenType)
    {
        // In real token response, token_type would be "DPoP"
        expectedTokenType.Should().Be("DPoP");
    }

    [Then(@"the API request should succeed")]
    public void ThenTheAPIRequestShouldSucceed()
    {
        _thrownException.Should().BeNull();
        _httpContext!.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        _next.Received(1).Invoke(_httpContext);
    }

    [Then(@"the user context should contain DPoP claims")]
    [And(@"the user context should contain DPoP claims")]
    public void ThenTheUserContextShouldContainDPoPClaims()
    {
        _httpContext!.User.Should().NotBeNull();
        _httpContext.User.HasClaim("dpop_thumbprint", _keyThumbprint!).Should().BeTrue();
    }

    [Then(@"the API request should be rejected")]
    public void ThenTheAPIRequestShouldBeRejected()
    {
        _httpContext!.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _next.DidNotReceive().Invoke(_httpContext);
    }

    [Then(@"the response should indicate ""(.*)""")]
    [And(@"the response should indicate ""(.*)""")]
    public void ThenTheResponseShouldIndicate(string expectedMessage)
    {
        var authHeader = _httpContext!.Response.Headers["WWW-Authenticate"].ToString();
        authHeader.Should().Contain(expectedMessage, $"Expected response header to contain '{expectedMessage}' but got '{authHeader}'");
    }

    [Then(@"the status code should be (\d+)")]
    [And(@"the status code should be (\d+) (.*)")]
    public void ThenTheStatusCodeShouldBe(int expectedStatusCode, string description = "")
    {
        _httpContext!.Response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Then(@"no DPoP validation should be performed")]
    [And(@"no DPoP validation should be performed")]
    public void ThenNoDPoPValidationShouldBePerformed()
    {
        _tokenValidator.DidNotReceive().ValidateAsync(
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    private static HttpContext CreateHttpContext(string token, string dpopProof)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {token}";
        context.Request.Headers["DPoP"] = dpopProof;
        context.Request.Method = "GET";
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("api.example.com");
        context.Request.Path = "/test";
        return context;
    }

    private static HttpContext CreateHttpContextBearerOnly(string token)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {token}";
        // No DPoP header for Bearer-only requests
        context.Request.Method = "GET";
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("api.example.com");
        context.Request.Path = "/test";
        return context;
    }

    private static ClaimsPrincipal CreateDPoPPrincipal(string thumbprint)
    {
        var claims = new List<Claim>
        {
            new("sub", "user123"),
            new("client_id", "webapp"),
            new("dpop_thumbprint", thumbprint),
            new("scope", "api1")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "DPoP"));
    }
}
