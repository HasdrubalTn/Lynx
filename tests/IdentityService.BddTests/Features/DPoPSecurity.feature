Feature: DPoP Token Security
  As a security-conscious system
  I want to implement DPoP (Demonstration of Proof-of-Possession) token binding
  So that tokens are cryptographically bound to client keys and prevent token theft

  Background:
    Given the Identity Service supports DPoP
    And the API Gateway validates DPoP proofs

  @dpop @security
  Scenario: Issue DPoP-bound access token
    Given a client "WebApp" requires DPoP
    And the client generates a DPoP key pair
    When the client requests an access token with DPoP proof
    Then the access token should be DPoP-bound
    And the token should contain the key thumbprint
    And the token type should be "DPoP"

  @dpop @validation @security
  Scenario: Valid DPoP proof allows API access
    Given a DPoP-bound access token
    And a valid DPoP proof for "GET /api/users"
    When the client calls the protected API with the token and proof
    Then the API request should succeed
    And the user context should contain DPoP claims

  @dpop @security @replay-protection
  Scenario: Replayed DPoP proof is rejected
    Given a DPoP-bound access token
    And a valid DPoP proof for "GET /api/users"
    And the proof has already been used once
    When the client attempts to reuse the same proof
    Then the API request should be rejected
    And the response should indicate "replay detected"
    And the status code should be 401 Unauthorized

  @dpop @validation @security
  Scenario: Mismatched token and proof binding fails
    Given a DPoP-bound access token for key "KeyA"
    And a DPoP proof signed with different key "KeyB"
    When the client calls the protected API
    Then the API request should be rejected
    And the response should indicate "binding mismatch"
    And the status code should be 401 Unauthorized

  @dpop @backward-compatibility
  Scenario: Bearer tokens still work during transition
    Given a client "LegacyApp" does not require DPoP
    And a valid Bearer access token
    When the client calls the protected API with Bearer token
    Then the API request should succeed
    And no DPoP validation should be performed

  @dpop @security
  Scenario: Invalid DPoP proof signature is rejected
    Given a DPoP-bound access token
    And a DPoP proof with invalid signature
    When the client calls the protected API
    Then the API request should be rejected
    And the response should indicate "invalid signature"
    And the status code should be 401 Unauthorized

  @dpop @token-binding
  Scenario: DPoP token without proof is rejected
    Given a DPoP-bound access token
    When the client calls the API without DPoP proof header
    Then the API request should be rejected
    And the response should indicate "DPoP proof required"
    And the status code should be 401 Unauthorized

  @dpop @expiration
  Scenario: Expired DPoP token is rejected
    Given an expired DPoP-bound access token
    And a valid DPoP proof
    When the client calls the protected API
    Then the API request should be rejected
    And the response should indicate "token expired"
    And the status code should be 401 Unauthorized
