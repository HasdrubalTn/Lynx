# Product Requirements Document — Lynx Generic Skeleton

## Vision
Provide a production-ready skeleton for microservices + React applications with built-in Identity, RBAC, Admin, and Notification services.

## Core Components
- **IdentityService** (Duende IdentityServer Community) for authentication, OIDC, RBAC claims issuance.
- **NotificationService** for SMS/Email/In-App.
- **ApiGateway** for routing and BFF patterns.
- **WebApp** (React + Vite) for end-user.
- **AdminApp** (React + Vite) for admins (RBAC protected).
- **Shared Packages** (NuGet) for shared abstractions (e.g., `Lynx.Abstractions`, `Lynx.Testing`, `Lynx.Http`).
- **Observability** hooks (structured logging, health checks, readiness/liveness).

## Non-Functional
- CI must enforce code style, tests, and linked issues.
- One feature per PR policy.
- PostgreSQL as default database for services.
- Docker-compose for local orchestration.
- Hostinger VPS deployment via container images & compose.

## Authentication & Authorization
- OIDC flows via IdentityService.
- Users & roles (RBAC) backed by IdentityService and claims mapping.
- AdminApp requires role `admin`.

## Environments
- Local (docker-compose), Staging, Production.

## Acceptance (Definition of Done)
- Design Document created and approved.
- Feature branch created from issue using MCP GitHub.
- Unit tests and CI pass; analyzers clean; docs updated.
- No mixing of feature types in the same PR.
