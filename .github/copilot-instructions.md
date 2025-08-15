Yes — what Copilot generated is a decent start, but it’s missing some of your **critical guardrails** and the **stronger MCP flow** we discussed earlier.
Here’s an **updated `copilot-instructions.md`** that merges your rules, the Lynx architecture, and the way you want Copilot to behave — so it can be your *always-on operating manual* inside the repo.

---

````md
# Lynx — AI Agent Instructions

You are my Co-Developer for the **Lynx** project.
Use the GitHub MCP server for all repo operations (branches, PRs, issues).

---

## Architecture & Components
Lynx is a microservices skeleton with:

- **ApiGateway** (port 8080) — BFF / routing layer
- **IdentityService** (port 8081) — Duende IdentityServer (Community Edition)
- **NotificationService** (port 8082) — SMS / Email / In-App messaging
- **WebApp** / **AdminApp** — React + TypeScript + Vite frontends
- **PostgreSQL** (port 5432) — shared DB
- **Lynx.Abstractions** — shared contracts/interfaces packaged as NuGet
- **Lynx.Testing** — shared test helpers packaged as NuGet

Services communicate via HTTP/JSON with typed `HttpClient`.  
No message bus yet — only direct HTTP calls.

---

## Development Workflow (MANDATORY)

1. **Design-First Flow**
   - Draft a design using `docs/templates/DESIGN_TEMPLATE.md`
   - Include *What*, *Why*, *How*, *Tests*, and Mermaid diagrams for architectural features.
   - Ask: **"GO DESIGN?"** — wait for approval before continuing.
   - After “GO DESIGN”, ask: **"GO BUILD?"** before coding.

2. **Feature Typing**
   - One PR = one feature type (`functional`, `architectural`, `refactoring`, `technical`).
   - **NEVER** mix types in a single PR.
   - Functional = product behavior; Architectural = cross-cutting structure; Refactoring = code cleanup; Technical = tooling/infrastructure.

3. **Branch & PR Rules**
   - Create branches via MCP or `scripts/new-feature-branch.ps1` → `feature/<issue>-<slug>`
   - Always confirm before creating branch.
   - Do not open PRs until I explicitly say “GO PR”.
   - PR must:
     - Link an Issue with `Fixes #<number>`
     - Use `.github/pull_request_template.md`
     - Pass CI and all validators.

4. **Post-Merge Retros**
   - After merge, propose bullet points for `docs/retros/LEARNINGS.md`.

---

## Code Patterns & Standards

### Backend (.NET 9)
- **Async API** — Always include `CancellationToken` in public async methods.
- **Logging** — Structured logging with scopes (`log.BeginScope`).
- **Error Handling** — Prefer `Result<T>` or problem-details over throwing exceptions for business logic.
- **HttpClient** — Typed clients with Polly retry policies.
- **Global Settings** — `Directory.Build.props`: nullable enabled, warnings as errors, analyzers enforced.

**Example — Typed HttpClient**
```csharp
public sealed class UserClient(HttpClient http, ILogger<UserClient> log)
{
    public async Task<UserDto?> GetAsync(Guid id, CancellationToken ct)
    {
        using var _ = log.BeginScope("UserId:{UserId}", id);
        var res = await http.GetAsync($"/users/{id}", ct);
        return res.IsSuccessStatusCode
            ? await res.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct)
            : null;
    }
}
````

---

### Testing Stack

* **Frameworks** — xUnit + AutoFixture + AutoFixture.AutoNSubstitute + NSubstitute + FluentAssertions.
* **HTTP mocking** — `HttpMessageHandler` or `RichardSzalay.MockHttp` (no real HTTP calls in tests).
* **Test Usings** — Every test project includes:

```csharp
global using Xunit;
global using FluentAssertions;
global using AutoFixture;
global using AutoFixture.Xunit2;
global using AutoFixture.AutoNSubstitute;
global using NSubstitute;
```

---

### Frontend (React + Vite)

* TypeScript required.
* React 18 + Vite starter with `useState`/`useEffect` (simple state for now).
* OIDC integration with IdentityService (planned).
* AdminApp = RBAC-protected (`admin` role minimum).

---

## Build & Deploy

* **Local**: `docker-compose up`
* **Build**: `dotnet build`
* **Test**: `dotnet test`
* **Target**: Hostinger VPS container deployment

---

## Key Reference Files

* `docs/PRD.md` — Product Requirements Document
* `docs/templates/DESIGN_TEMPLATE.md` — Required design format
* `docs/playbooks/coding.md` — Coding patterns
* `.github/ISSUE_TEMPLATE/feature.yml` — Feature issue template
* `.github/pull_request_template.md` — PR checklist
* `.githooks/` — Git hooks for formatting & commit validation

---

## Behavior Reminders

* Never invent features outside the PRD.
* Always confirm before executing MCP commands.
* When in doubt, **ask**.

```

---

If you drop this into your repo root (or under `/docs/copilot-instructions.md`), Copilot will **pull from it automatically** in completions and chat, so you won’t need to re-paste your rules every session.  

Do you want me to **inject this file directly into your Lynx repo zip** so it’s ready to use? That way your Copilot will see it immediately.
```
