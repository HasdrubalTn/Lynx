# Lynx — Generic Microservices + React Skeleton

Stack: **ASP.NET Core 9**, **React + Vite**, **PostgreSQL**, **Docker**, **GitHub CI/CD**, **Hostinger VPS** target.
Identity: **Duende IdentityServer (Community Edition)** in a dedicated microservice.
Notifications: dedicated microservice (SMS, Email, In-App).

This repo is pre-wired for:
- MCP GitHub server usage (create branches, issues, PRs; guarded prompts).
- "One feature per PR" with validators and templates.
- Separation of concerns: no mixing **technical / architectural / refactoring / functional** features.
- Design-first flow: a **Design Document** is approved by the Co-Developer before implementation.
- Test standards: xUnit, AutoFixture, NSubstitute, FluentAssertions. HTTP mocked via `HttpMessageHandler`.

> Project codename: **Lynx**.


## Git hooks — auto-enable options

> For security, Git **will not execute** repository-supplied hooks automatically on clone.
> Use one of these near-automatic methods:

**Option A (at clone time — single command):**
```bash
git clone --config core.hooksPath=.githooks <repo-url>
```

**Option B (one-time bootstrap after clone):**
```bash
# macOS/Linux
make bootstrap
# or
./scripts/enable-githooks.sh

# Windows PowerShell
./scripts/enable-githooks.ps1
```

**Option C (global includeIf — set once, then automatic for all Lynx clones):**
```bash
# Replace <your-Lynx-path> with your real path; this links repo-local .gitconfig
git config --global includeIf."gitdir:<your-Lynx-path>/.git".path "<your-Lynx-path>/.gitconfig"
```
Then add this to `<repo>/.gitconfig` (already present as sample below) to pin hooks:
```ini
[core]
    hooksPath = .githooks
```
