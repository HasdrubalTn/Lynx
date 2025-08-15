# Design — <Feature Name>

**What:**  
Describe the feature clearly in one or two sentences.

**Why:**  
Explain the business/technical value and constraints.

**How:**  
- Architecture choices
- Contracts (DTOs, endpoints)
- Data model changes (if any)
- Rollout plan / migration
- Risks & mitigations

**Tests:**  
- Unit test list (xUnit + AutoFixture + NSubstitute + FluentAssertions)
- Contract tests
- Performance checks (if relevant)

---
## Mermaid Diagrams (for architectural features)

### Context
```mermaid
C4Context
title System Context - Lynx
Boundary(Boundary_Lynx, "Lynx") {
  Person(User, "User")
  System(WebApp, "WebApp")
  System(AdminApp, "AdminApp")
  System_Boundary(Services, "Microservices") {
    System(Identity, "IdentityService (Duende)")
    System(Notif, "NotificationService")
    System(ApiGw, "ApiGateway")
  }
  SystemDb(Pg, "PostgreSQL")
}
Rel(User, WebApp, "Uses")
Rel(AdminApp, Identity, "OIDC")
Rel(WebApp, ApiGw, "HTTP/JSON")
Rel(ApiGw, Identity, "OIDC Introspection")
Rel(ApiGw, Notif, "HTTP/JSON")
Rel(Identity, Pg, "Stores identity data")
Rel(Notif, Pg, "Stores notification state")
```

### Container
```mermaid
flowchart LR
  subgraph Client
    A[WebApp] -->|OIDC| B((IdentityService))
    C[AdminApp] -->|OIDC| B
  end
  subgraph Backend
    D[ApiGateway/BFF]
    E[NotificationService]
    B[IdentityService]
    PG[(PostgreSQL)]
  end
  A --> D
  C --> D
  D --> E
  B --> PG
  E --> PG
```
