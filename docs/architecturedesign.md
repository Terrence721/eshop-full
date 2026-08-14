# Architecture Overview

<!-- markdownlint-disable-next-line MD036 -->
**Last Updated: August 14, 2026**

This document describes the architecture eShop-full is being built toward — verified against the real source app (Microsoft's [dotnet/eShop](https://github.com/dotnet/eShop), snapshotted locally at `F:\eShop-main\eShop-main`) and against what has actually landed in this repo, not a generic description of what an e-commerce microservices app "usually" looks like. See [todo.md](../todo.md) for exactly how much of this exists right now, and the [project board](https://github.com/users/Terrence721/projects/5) for the live per-project status.

**Current status, stated plainly:** as of this writing, `src/Shared/` (2 linked-source files) is added and reviewed, `src/EventBus/EventBus.csproj` exists but its 8 source files don't yet, and none of the other 18 projects exist on disk. Everything below describing the full system is the target this repo is being built toward one file at a time, not a claim that it already runs end-to-end.

## 1. What this is

eShop ("AdventureWorks") is Microsoft's reference .NET application for an e-commerce site built on a services-based architecture using [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/). The domain: browsing a product catalog, a shopping basket, placing orders, user identity/auth, payment processing, and outbound webhook notifications for third-party integrations.

## 2. Microservices, not a monolith

This is a deliberate architecture, not a default — and it's the whole point of Microsoft's reference sample: to demonstrate real microservices patterns with .NET Aspire, not to show how to structure a single app.

**Why the split holds up on inspection, not just by upstream precedent:**

- **Genuinely different scaling/consistency needs per service.** `Catalog.API` is read-heavy and could scale independently of everything else. `Basket.API` is Redis-backed, ephemeral cart state — a different persistence model entirely from `Ordering`, which needs transactional consistency for placing an order. `OrderProcessor`/`PaymentProcessor` are background workers processing queued work, not HTTP-facing services at all — they have no reason to share a deployable with anything that does.
- **Event-driven integration, not synchronous coupling.** Services don't call each other directly for cross-domain concerns — they publish/consume integration events through `EventBus`/`EventBusRabbitMQ` (RabbitMQ-backed). `Ordering` publishing an order-placed event that other services react to asynchronously is a fundamentally different (and more resilient) integration model than direct service-to-service HTTP calls.
- **Aspire's own value proposition assumes this shape.** `eShop.AppHost` and `eShop.ServiceDefaults` exist specifically to orchestrate and standardize telemetry/health-checks/resilience across *many* independently-deployable services with unified local-dev tooling (the Aspire dashboard). None of that infrastructure would earn its keep in a single-deployable app.
- **The frontends genuinely need different tech, not just different code.** `WebApp` (Blazor web storefront), `ClientApp` (native .NET MAUI mobile), and `HybridApp` (Blazor Hybrid) target fundamentally different platforms — they can't be the same deployable by construction, not by choice.

**The trade-off, stated plainly:** microservices buy independent scaling/deployment and failure isolation per service, in exchange for operational cost — more moving parts locally (which is exactly what Aspire's orchestration exists to tame), network calls where a monolith would have an in-process call, and eventual consistency at integration-event boundaries instead of a single database transaction. For a reference architecture whose explicit purpose is teaching this pattern, that trade-off is the point.

## 3. Repository structure

Target layout, verified against the source app (✅ = exists in this repo right now):

```text
eShop-full/
├── src/
│   ├── Shared/                  ✅ linked-source utilities (not a .csproj — see Section 4)
│   ├── EventBus/                ✅ .csproj added, source files pending
│   ├── EventBusRabbitMQ/
│   ├── eShop.ServiceDefaults/
│   ├── IntegrationEventLogEF/
│   ├── Identity.API/
│   ├── Catalog.API/
│   ├── Basket.API/
│   ├── Ordering.Domain/
│   ├── Ordering.Infrastructure/
│   ├── Ordering.API/
│   ├── OrderProcessor/
│   ├── PaymentProcessor/
│   ├── Webhooks.API/
│   ├── WebhookClient/
│   ├── WebApp/
│   ├── WebAppComponents/
│   ├── HybridApp/
│   ├── ClientApp/                # .NET MAUI
│   └── eShop.AppHost/             # Aspire orchestrator — added last, references everything
├── tests/                         # 5 test projects (Basket.UnitTests, Catalog.FunctionalTests,
│                                   # Ordering.FunctionalTests, Ordering.UnitTests, ClientApp.UnitTests)
├── build/                         # Build tooling from source repo
├── e2e/                          ✅ Playwright e2e specs
├── docs/                         ✅ this folder
├── .github/                      ✅ GitHub Actions workflows + dependabot.yml
├── global.json, Directory.Build.props/.targets, Directory.Packages.props, nuget.config  ✅
└── eShop.slnx, eShop.Web.slnf    ✅ kept trimmed to only projects that exist — see todo.md
```

`eShop.slnx`/`eShop.Web.slnf` deliberately do **not** list all 19 projects upfront the way the source repo's did — each project is added to them the moment it actually lands, so `dotnet build` and CI only ever try to build what exists. See `todo.md`'s "Still to do" for why this matters.

## 4. Services breakdown

- **Foundation/shared** (added or in progress first, everything else depends on these): `Shared` (linked-source utilities, not a standalone project — no `.csproj`, files are compiled directly into consuming projects), `EventBus` (abstractions), `EventBusRabbitMQ` (RabbitMQ implementation), `eShop.ServiceDefaults` (Aspire telemetry/health-check/resilience defaults shared by every service), `IntegrationEventLogEF` (EF Core-backed transactional outbox for integration events).
- **Domain APIs**: `Identity.API` (Duende IdentityServer — auth/OIDC provider for the whole system), `Catalog.API`, `Basket.API` (Redis-backed), `Ordering.API` + `Ordering.Domain` + `Ordering.Infrastructure` (domain-driven design split: domain model, EF Core persistence, and the API surface are three separate projects), `Webhooks.API`.
- **Background workers**: `OrderProcessor`, `PaymentProcessor` — queue-driven, no HTTP surface.
- **Sample integration consumer**: `WebhookClient` — a sample app demonstrating consuming `Webhooks.API`'s outbound notifications.
- **Web frontends**: `WebApp` (Blazor storefront), `WebAppComponents` (Razor components shared between `WebApp` and `HybridApp`), `HybridApp` (Blazor Hybrid), `ClientApp` (.NET MAUI native client).
- **Orchestration**: `eShop.AppHost` — the Aspire AppHost project. References every other project to wire up local orchestration, service discovery, and the Aspire dashboard. Added last in the migration by design, since it can't meaningfully exist until the things it orchestrates do.

## 5. Package management and tooling

- **.NET 10** (SDK pinned to `10.0.400` in `global.json`, stable — not the preview channel the source snapshot was on), **.NET Aspire 13.4.6**.
- **Central NuGet package management** via `Directory.Packages.props` — every version pin individually researched against actual current-latest rather than copied from source. See `todo.md`'s "Directory.Packages.props research" for the notable deviations (license-driven pins, packages that diverged from a shared version variable, etc.).
- **`.slnx`** (the newer XML solution format) instead of the legacy `.sln`, plus **`.slnf`** (a solution filter) for the Windows/non-MAUI dev loop — `eShop.Web.slnf` excludes `ClientApp`/`HybridApp`/`ClientApp.UnitTests` since those need the MAUI workload.
- **Yarn 4.18.0** (Berry, `node-modules` linker) for the Playwright e2e suite — the only JavaScript in this repo. Source used npm; switched to Yarn per the project requirements, using the `node-modules` linker rather than Berry's PnP default because Playwright's browser-install tooling and VS Code extension both assume a standard `node_modules` layout.

## 6. Local dev and orchestration flow

Once `eShop.AppHost` exists: `dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj` launches the Aspire dashboard, which orchestrates every service, their container dependencies (PostgreSQL, RabbitMQ, Redis), and inter-service discovery — no manual per-service startup. This isn't runnable yet since `eShop.AppHost` is deliberately the last project added (Section 4) — expected mid-migration, not a gap.

## 7. CI

- **`.github/workflows/pr-validation.yml`** — `dotnet build`/`dotnet test` against `eShop.Web.slnf` on every push/PR.
- **`.github/workflows/pr-validation-maui.yml`** — separate `windows-latest` job building/testing `ClientApp` with the MAUI workloads installed.
- **`.github/workflows/playwright.yml`** — e2e tests against a live `eShop.AppHost`-launched instance.
- **`.github/workflows/markdownlint.yml`** — markdown linting, required check on every PR.
- **`.github/dependabot.yml`** — watches `nuget`, `npm`, and `github-actions` ecosystems, weekly.
- Root **`ci.yml`** from the source repo is deliberately excluded — it's Microsoft's internal Azure DevOps pipeline (`1ESPipelineTemplates`, an internal-only agent pool), not something that runs outside their org. The GitHub Actions workflows above are the real CI for this public repo.

**All three build/test workflows are red right now, on purpose** — they reference `eShop.Web.slnf`/`ClientApp`, and only 1 of 19 projects exists so far. See `todo.md`'s "CI status" for the reasoning behind leaving this visible rather than masking it.

## 8. Where to go next

- Live progress and evidence trail: [todo.md](../todo.md)
- Per-project Kanban status: [GitHub Project board](https://github.com/users/Terrence721/projects/5)
- Getting started / running the solution: [README.md](../README.md)
