# Design — OIDC Auth + RBAC (Admin) through IdentityService

**What:**  
Implement OpenID Connect authentication with Authorization Code + PKCE flow for WebApp and AdminApp, plus role-based access control enforcing admin-only access to /admin/* routes through ApiGateway JWT validation.

**Why:**  
Establishes secure, standards-based authentication foundation and admin-only access controls required for all future features. Provides proper separation between public user features and administrative functionality while maintaining security best practices with no client secrets and proper CORS configuration.

**How:**

### IdentityService Configuration
- **SPA Client Configuration:**
  - Register two OIDC clients: `lynx-webapp` and `lynx-adminapp`
  - Redirect URIs: `http://localhost:3000/signin-oidc`, `http://localhost:3001/signin-oidc` (dev), `https://app.lynx.com/signin-oidc`, `https://admin.lynx.com/signin-oidc` (prod)
  - Post-logout URIs: `http://localhost:3000`, `http://localhost:3001` (dev), `https://app.lynx.com`, `https://admin.lynx.com` (prod)
  - Grant types: `authorization_code`, `refresh_token`
  - Response types: `code`
  - PKCE required: `true`
  - Client secrets: None (public clients)
  - Scopes: `openid`, `profile`, `roles`, `lynx_api`
- **CORS Policy:**
  - Allow origins: `http://localhost:3000`, `http://localhost:3001` (dev), `https://app.lynx.com`, `https://admin.lynx.com` (prod)
  - Allow credentials: `true`
  - Allow headers: `Content-Type`, `Authorization`
- **Role Claims Issuance:**
  - Configure custom claims transformation to include `role` claim in access tokens
  - Map ASP.NET Identity roles to OIDC `role` claim
- **Seed Admin User:**
  - Username: `admin@lynx.local`
  - Password: `Admin123!` (configurable via environment variable)
  - Role: `admin`
  - Auto-create during IdentityService startup if not exists

### ApiGateway JWT Authentication
- **JWT Bearer Authentication:**
  - Configure JWT validation with IdentityService as authority
  - Validate issuer, audience, signature, and expiration
  - Map `role` claim to ClaimsPrincipal roles
- **Authorization Policies:**
  - Create `AdminPolicy` requiring `role=admin` claim
  - Apply `[Authorize(Policy = "AdminPolicy")]` to all `/admin/*` controllers/actions
  - Return 401 for invalid/missing tokens, 403 for insufficient roles
- **/me Endpoint:**
  - Route: `GET /api/me`
  - Returns: `{ "id": "guid", "username": "string", "roles": ["string"] }`
  - Requires valid access token
  - Maps from ClaimsPrincipal to user info DTO

### WebApp/AdminApp OIDC Integration
- **OIDC Configuration:**
  - Library: `oidc-client-ts` for TypeScript support
  - Authority: `http://localhost:8081` (dev), `https://identity.lynx.com` (prod)
  - Client IDs: `lynx-webapp`, `lynx-adminapp`
  - Redirect URI: `/signin-oidc`
  - Post-logout redirect: `/`
  - Response type: `code`
  - Scope: `openid profile roles lynx_api`
  - PKCE: enabled
- **Login/Logout Flows:**
  - Login button redirects to IdentityService `/connect/authorize`
  - Handle callback at `/signin-oidc` route
  - Store tokens in `sessionStorage` (not localStorage for security)
  - Logout clears tokens and redirects to IdentityService `/connect/endsession`
- **Token Management:**
  - Automatic token refresh using refresh tokens
  - Include `Authorization: Bearer {access_token}` header in all API calls
  - Clear tokens on 401 responses and redirect to login
- **Route Guards:**
  - AdminApp: Check for valid token and `admin` role before rendering /admin routes
  - Show 403 error page for authenticated but unauthorized users
  - Redirect to login for unauthenticated users

### Security Considerations
- **No Client Secrets:** SPAs are public clients, rely on PKCE for security
- **CORS Configuration:** Strict origin validation, no wildcards in production
- **HTTPS Enforcement:** All production traffic over TLS
- **Token Storage:** sessionStorage only, automatic cleanup on tab close
- **CSP Headers:** Restrict script sources and inline execution

### Rollout Plan
1. Configure IdentityService with Duende IdentityServer Community Edition
2. Implement ApiGateway JWT validation and /me endpoint
3. Add AdminApp OIDC integration with route guards
4. Add WebApp OIDC integration (prepare for future user features)
5. Deploy with proper HTTPS and CORS configuration

### Risks & Mitigations
- **Risk:** OIDC configuration complexity
  - **Mitigation:** Use well-tested libraries (oidc-client-ts, ASP.NET JWT middleware)
- **Risk:** Token refresh failures leading to poor UX
  - **Mitigation:** Implement automatic refresh with fallback to re-authentication
- **Risk:** CORS misconfigurations blocking legitimate requests
  - **Mitigation:** Comprehensive testing across development and production origins

**Tests:**

### Unit Tests (ApiGateway)
- `JwtAuthenticationMiddleware_ValidToken_SetsClaimsPrincipal`
- `JwtAuthenticationMiddleware_ExpiredToken_Returns401`
- `JwtAuthenticationMiddleware_InvalidSignature_Returns401`
- `JwtAuthenticationMiddleware_MissingToken_Returns401`
- `AdminAuthorizationPolicy_UserWithAdminRole_AllowsAccess`
- `AdminAuthorizationPolicy_UserWithoutAdminRole_Returns403`
- `AdminAuthorizationPolicy_UnauthenticatedUser_Returns401`
- `MeEndpoint_ValidToken_ReturnsUserInfo`
- `MeEndpoint_ValidToken_ReturnsUserRoles`
- `MeEndpoint_InvalidToken_Returns401`
- `MeEndpoint_MissingClaims_ReturnsEmptyRoles`

### Integration Tests
- **IdentityService:** OIDC discovery endpoint returns correct configuration
- **E2E Flow:** Complete login flow from AdminApp through IdentityService to ApiGateway
- **Token Validation:** ApiGateway correctly validates tokens issued by IdentityService

### UI Guard Tests (Minimal)
- AdminApp login button redirects to IdentityService
- AdminApp shows 403 page for non-admin users
- AdminApp clears tokens and redirects on logout
- WebApp handles authentication state correctly

---
## Mermaid Diagrams

### Container Architecture
```mermaid
C4Container
title Container Diagram - OIDC Auth + RBAC

Person(user, "User")
Person(admin, "Admin User")

Container_Boundary(spa, "Single Page Applications") {
  Container(webapp, "WebApp", "React + TypeScript", "Public user interface")
  Container(adminapp, "AdminApp", "React + TypeScript", "Admin interface with RBAC")
}

Container_Boundary(backend, "Backend Services") {
  Container(gateway, "ApiGateway", "ASP.NET Core", "JWT validation, /me endpoint, RBAC enforcement")
  Container(identity, "IdentityService", "Duende IdentityServer", "OIDC provider, user management, role claims")
  Container(notification, "NotificationService", "ASP.NET Core", "Business logic service")
}

ContainerDb(postgres, "PostgreSQL", "Database", "User accounts, roles, application data")

Rel(user, webapp, "Uses", "HTTPS")
Rel(admin, adminapp, "Uses", "HTTPS")
Rel(webapp, identity, "Authenticates via", "OIDC Authorization Code + PKCE")
Rel(adminapp, identity, "Authenticates via", "OIDC Authorization Code + PKCE")
Rel(webapp, gateway, "API calls", "HTTP/JSON with JWT")
Rel(adminapp, gateway, "API calls", "HTTP/JSON with JWT")
Rel(gateway, identity, "Token validation", "JWT introspection")
Rel(gateway, notification, "Service calls", "HTTP/JSON")
Rel(identity, postgres, "Stores", "Users, roles, clients")
Rel(notification, postgres, "Stores", "Application data")

UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="2")
```

### OIDC Authorization Code + PKCE Flow
```mermaid
sequenceDiagram
    participant U as User
    participant A as AdminApp (SPA)
    participant I as IdentityService
    participant G as ApiGateway
    participant DB as PostgreSQL

    Note over U,DB: OIDC Authorization Code + PKCE Authentication Flow

    U->>A: Click "Login"
    A->>A: Generate code_verifier & code_challenge (PKCE)
    A->>I: Redirect to /connect/authorize<br/>?client_id=lynx-adminapp<br/>&response_type=code<br/>&scope=openid profile roles lynx_api<br/>&redirect_uri=.../signin-oidc<br/>&code_challenge=...&code_challenge_method=S256
    
    I->>I: Show login form
    U->>I: Enter credentials
    I->>DB: Validate user & load roles
    DB-->>I: User data + roles
    I->>A: Redirect to /signin-oidc?code=AUTH_CODE
    
    A->>I: POST /connect/token<br/>grant_type=authorization_code<br/>code=AUTH_CODE<br/>redirect_uri=.../signin-oidc<br/>client_id=lynx-adminapp<br/>code_verifier=...
    I->>I: Validate PKCE & issue tokens
    I-->>A: access_token + refresh_token + id_token
    
    A->>A: Store tokens in sessionStorage
    A->>G: GET /api/me<br/>Authorization: Bearer ACCESS_TOKEN
    G->>G: Validate JWT signature & claims
    G-->>A: { "id": "...", "username": "admin@lynx.local", "roles": ["admin"] }
    
    A->>A: Check roles for route access
    A->>A: Render admin interface
    
    Note over A,G: Subsequent API calls include JWT Bearer token
    A->>G: GET /admin/users<br/>Authorization: Bearer ACCESS_TOKEN
    G->>G: Validate JWT & check admin role
    alt User has admin role
        G-->>A: 200 OK + user data
    else User lacks admin role
        G-->>A: 403 Forbidden
    end

    Note over U,A: Logout flow
    U->>A: Click "Logout"
    A->>A: Clear sessionStorage
    A->>I: Redirect to /connect/endsession<br/>?post_logout_redirect_uri=...
    I-->>A: Redirect to AdminApp homepage
```
