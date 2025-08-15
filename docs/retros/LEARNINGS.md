# Learnings
Append bullet points from each PR's "Notes/Learnings" section.

## Bootstrap Health Checks Feature (PR #1)

- **AutoFixture + NSubstitute Integration**: Custom `AutoDataWithMockingAttribute` is essential for proper interface injection in xUnit tests. Default AutoFixture doesn't handle NSubstitute mocking correctly without explicit customization.
- **HttpClient Timeout Patterns**: Using `CancellationTokenSource.CreateLinkedTokenSource()` provides better timeout control than HttpClient timeout properties, especially for health check scenarios where you need precise timeout behavior.
- **Docker Multi-Project Dependencies**: When containerizing .NET solutions with project references, build context must be at solution root, not individual project directories. Copy solution files first, restore dependencies, then copy source code for optimal layer caching.
- **Frontend CI Reliability**: Always commit `package-lock.json` files to ensure npm ci works consistently across environments. Missing lock files cause unpredictable dependency resolution in CI/CD pipelines.
- **Container Port Management**: Explicitly configure different ports for each service in Docker Compose (8080, 8081, 8082) and ensure Dockerfile EXPOSE directives match the service's actual listening port to avoid routing confusion.
- **Test Infrastructure Investment**: Spending time on proper test setup (code coverage, mocking frameworks, CI integration) pays dividends immediately. We went from unreliable tests to 15/15 passing with consistent behavior across local and CI environments.

## Test Email Functionality Feature (PR #10)

- **ActionResult Testing Patterns**: When testing ASP.NET Core controllers returning `ActionResult<T>`, access the underlying result with `.Result` property and cast to `ObjectResult` for status code assertions. Direct casting fails due to generic wrapper types.
- **Service-to-Service HttpClient Configuration**: For microservice communication, configure typed HttpClient in DI with base address and proper error handling. Avoid hardcoded URLs in controller logic—use configuration or service discovery patterns.
- **Email Validation Best Practices**: `System.Net.Mail.MailAddress` constructor provides robust email format validation without external dependencies. Wrap in try-catch and return proper 400 responses for invalid formats.
- **Structured Logging with Scopes**: Use `logger.BeginScope()` for contextual information (user ID, email address) that automatically appears in all log entries within the scope. Essential for tracing requests across service boundaries.
- **HTTP Client Mocking Complexity**: NSubstitute doesn't easily mock HttpMessageHandler for HttpClient testing. Consider integration testing with TestServer or accept simplified unit tests focusing on controller logic rather than HTTP transport.
- **Message ID Generation**: Random 12-character alphanumeric IDs (using Random.Shared) provide good uniqueness for short-term tracking without UUID complexity. Use consistent patterns across microservices for correlation.
