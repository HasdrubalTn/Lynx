Feature: Persistent Storage
  As the Identity Service
  I want to use EF Core for persistent storage
  So that configuration and tokens survive service restarts

  Background:
    Given the Identity Service uses PostgreSQL for storage
    And EF Core is configured for IdentityServer stores

  @storage @persistence @clients
  Scenario: Client configuration survives service restart
    Given a client "PersistentClient" is configured in the database
    And the client has DPoP enabled
    When the Identity Service restarts
    Then the client configuration should still be available
    And the client should still require DPoP

  @storage @persistence @tokens
  Scenario: Issued tokens persist across restarts
    Given a DPoP-bound access token is issued to "WebApp"
    And the token is stored in the database
    When the Identity Service restarts
    Then the token should still be valid
    And the DPoP binding should be preserved

  @storage @clients @dpop
  Scenario: DPoP client configuration is stored correctly
    Given I need to store a client with DPoP requirements
    When the client configuration is saved to the database
    Then the RequireDPoP flag should be persisted
    And the client can be retrieved with DPoP settings intact

  @storage @grants @dpop-binding
  Scenario: DPoP token binding is persisted
    Given a DPoP-bound token with thumbprint "abc123"
    When the token is stored in persisted grants
    Then the DPoP thumbprint should be stored
    And the thumbprint can be retrieved for validation

  @storage @grants @cleanup
  Scenario: Expired tokens are cleaned up
    Given expired access tokens exist in the database
    When the cleanup process runs
    Then expired tokens should be removed
    And DPoP bindings should be cleaned up too

  @storage @error-handling @database
  Scenario: Database connection failure is handled
    Given the PostgreSQL database is unavailable
    When a client attempts to get a token
    Then the request should fail gracefully
    And an appropriate error should be returned
    And the service should not crash

  @storage @performance @concurrent
  Scenario: Multiple concurrent token requests are handled
    Given multiple clients request tokens simultaneously
    When the database processes these requests
    Then all valid requests should succeed
    And no data corruption should occur
    And DPoP bindings should be correctly stored

  @storage @migration @data-integrity
  Scenario: Configuration migration preserves data
    Given existing in-memory client configurations
    When migrating to EF Core storage
    Then all client configurations should be preserved
    And DPoP settings should be correctly migrated
    And no configuration data should be lost

  @storage @backup @recovery
  Scenario: Configuration can be backed up and restored
    Given client configurations exist in the database
    When a database backup is created
    And the database is restored from backup
    Then all client configurations should be intact
    And DPoP settings should be preserved

  @storage @queries @performance
  Scenario: Client retrieval is performant
    Given 1000 clients exist in the database
    When retrieving a specific client by ID
    Then the query should complete quickly
    And the correct client should be returned
    And DPoP configuration should be included
