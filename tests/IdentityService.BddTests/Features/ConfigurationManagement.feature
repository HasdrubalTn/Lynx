Feature: Configuration Management
  As an administrator
  I want to manage client configurations through a web interface
  So that I can update OAuth clients and scopes without service restart

  Background:
    Given I am authenticated as an admin user
    And the Identity Configuration API is available

  @configuration @admin @crud
  Scenario: Admin creates new DPoP client
    Given no client exists with ID "NewDPoPClient"
    When I create a client with the following details:
      | Field        | Value           |
      | ClientId     | NewDPoPClient   |
      | ClientName   | New DPoP App    |
      | RequireDPoP  | true           |
      | RedirectUri  | https://app.com |
    Then the client should be created successfully
    And the client should require DPoP
    And the configuration should be active immediately

  @configuration @admin @crud
  Scenario: Admin updates client DPoP requirement
    Given a client "ExistingClient" exists with DPoP disabled
    When I update the client to require DPoP
    Then the client configuration should be updated
    And new token requests should require DPoP proof
    And existing tokens should remain valid until expiry

  @configuration @admin @crud  
  Scenario: Admin deletes unused client
    Given a client "UnusedClient" exists
    And the client has no active tokens
    When I delete the client "UnusedClient"
    Then the client should be removed from configuration
    And future token requests for this client should fail

  @configuration @security @authorization
  Scenario: Non-admin user cannot access configuration
    Given I am authenticated as a regular user
    When I attempt to access the client configuration API
    Then the request should be forbidden
    And the status code should be 403 Forbidden

  @configuration @security @authorization
  Scenario: Unauthenticated user cannot access configuration
    Given I am not authenticated
    When I attempt to access the client configuration API
    Then the request should be unauthorized
    And the status code should be 401 Unauthorized

  @configuration @validation
  Scenario: Invalid client configuration is rejected
    When I attempt to create a client with invalid data:
      | Field      | Value  |
      | ClientId   |        |
      | ClientName | Test   |
    Then the request should fail validation
    And the response should indicate "ClientId is required"
    And the status code should be 400 Bad Request

  @configuration @hot-reload
  Scenario: Configuration changes apply without restart
    Given a client "TestClient" with specific scopes
    When I update the client's allowed scopes
    Then the new scopes should be effective immediately
    And no service restart should be required
    And active sessions should continue working

  @configuration @scopes @crud
  Scenario: Admin manages API scopes
    Given I want to create a new API scope
    When I create a scope with the following details:
      | Field       | Value             |
      | Name        | user.write        |
      | DisplayName | Write User Data   |
      | Required    | false            |
    Then the scope should be created successfully
    And clients can request this scope

  @configuration @error-handling
  Scenario: Configuration service error is handled gracefully
    Given the configuration database is unavailable
    When I attempt to retrieve client configurations
    Then the request should fail gracefully
    And the status code should be 500 Internal Server Error
    And an appropriate error message should be returned

  @configuration @audit
  Scenario: Configuration changes are logged
    Given a client "AuditClient" exists
    When I update the client configuration
    Then the change should be logged in the audit trail
    And the log should include who made the change
    And the log should include what was changed
    And the log should include when the change occurred
