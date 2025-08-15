# Copilot / MCP Prompts

**Design-first (ask before coding):**
> Use docs/PRD.md and docs/templates/DESIGN_TEMPLATE.md. Draft a design (What/Why/How/Tests) for Issue <#>. Propose tasks grouped as: functional, architectural, refactoring, technical debt. Do not mix groups in the same PR. Ask for GO before scaffolding code.

**Branching via GitHub MCP:**
> Using GitHub MCP, create a feature branch `feature/<issue>-<slug>` for Issue <#>, but ask for confirmation first. Do not open a PR until I say "GO PR".

**PR review gate:**
> When I say "GO PR", open a PR that only includes the scope defined in the design. Attach acceptance criteria and link Issue. Apply PR template.
