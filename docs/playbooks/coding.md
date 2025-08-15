# Coding Playbook

## Languages, Frameworks, Libraries
- .NET 9 (ASP.NET Core)
- React + Vite (TS)
- PostgreSQL
- Tests: xUnit, AutoFixture, AutoFixture.AutoNSubstitute, NSubstitute, FluentAssertions
- HTTP mocking: custom `HttpMessageHandler` or `RichardSzalay.MockHttp`

## Patterns
- Typed HttpClient + Polly for retries.
- CancellationToken in all async public methods.
- Structured logging (Microsoft.Extensions.Logging).
- Results vs exceptions: prefer `Result<T>` style or return 4xx/5xx properly in minimal APIs.

## Example: service with typed HttpClient
```csharp
public sealed class UserClient(HttpClient http, ILogger<UserClient> log)
{
    public async Task<UserDto?> GetAsync(Guid id, CancellationToken ct)
    {
        using var _ = log.BeginScope("UserId:{UserId}", id);
        var res = await http.GetAsync($"/users/{id}", ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct);
    }
}
```
