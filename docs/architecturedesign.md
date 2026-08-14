# Architecture Overview

<!-- markdownlint-disable-next-line MD036 -->
**Last Updated: August 14, 2026**

This document describes the architecture eShop-full is being built toward — verified against the real source app (Microsoft's [dotnet/eShop](https://github.com/dotnet/eShop), snapshotted locally at `F:\eShop-main\eShop-main`) and against what has actually landed in this repo, not a generic description of what an e-commerce microservices app "usually" looks like. See [todo.md](../todo.md) for exactly how much of this exists right now, and the [project board](https://github.com/users/Terrence721/projects/5) for the live per-project status.

**Current status, stated plainly:** as of this writing, `src/Shared/` (2 linked-source files), `EventBus`, `EventBusRabbitMQ`, and `eShop.ServiceDefaults` are added and reviewed, and none of the other 15 projects exist on disk. Everything below describing the full system is the target this repo is being built toward one file at a time, not a claim that it already runs end-to-end.

## 1. What this is

eShop ("AdventureWorks") is Microsoft's reference .NET application for an e-commerce site built on a services-based architecture using [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/). The domain: browsing a product catalog, a shopping basket, placing orders, user identity/auth, payment processing, and outbound webhook notifications for third-party integrations.

## 2. Microservices, not a monolith

This is a deliberate architecture, not a default — and it's the whole point of Microsoft's reference sample: to demonstrate real microservices patterns with .NET Aspire, not to show how to structure a single app.

**Why the split holds up on inspection, not just by upstream precedent:**

- **Genuinely different scaling/consistency needs per service.** `Catalog.API` is read-heavy and could scale independently of everything else. `Basket.API` is Redis-backed, ephemeral cart state — a different persistence model entirely from `Ordering`, which needs transactional consistency for placing an order. `OrderProcessor`/`PaymentProcessor` are background workers processing queued work, not HTTP-facing services at all — they have no reason to share a deployable with anything that does.
- **Event-driven integration, not synchronous coupling.** Services don't call each other directly for cross-domain concerns — they publish/consume integration events through `EventBus`/`EventBusRabbitMQ` (RabbitMQ-backed). `Ordering` publishing an order-placed event that other services react to asynchronously is a fundamentally different (and more resilient) integration model than direct service-to-service HTTP calls.
- **Aspire's own value proposition assumes this shape.** `eShop.AppHost` and `eShop.ServiceDefaults` exist specifically to orchestrate and standardize telemetry/health-checks/resilience across *many* independently-deployable services with unified local-dev tooling (the Aspire dashboard). None of that infrastructure would earn its keep in a single-deployable app.
- **The frontends genuinely need different tech, not just different code.** `WebApp` (React web storefront) and `ClientApp` (native .NET MAUI mobile) target fundamentally different platforms — they can't be the same deployable by construction, not by choice.

**The trade-off, stated plainly:** microservices buy independent scaling/deployment and failure isolation per service, in exchange for operational cost — more moving parts locally (which is exactly what Aspire's orchestration exists to tame), network calls where a monolith would have an in-process call, and eventual consistency at integration-event boundaries instead of a single database transaction. For a reference architecture whose explicit purpose is teaching this pattern, that trade-off is the point.

## 3. Repository structure

Target layout, verified against the source app (✅ = exists in this repo right now):

```text
eShop-full/
├── src/
│   ├── Shared/                  ✅ linked-source utilities (not a .csproj — see Section 4)
│   ├── EventBus/                ✅ added and reviewed
│   ├── EventBusRabbitMQ/        ✅ added and reviewed — see Section 8
│   ├── eShop.ServiceDefaults/    ✅ added and reviewed
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
│   ├── WebApp/                    # React, not Blazor — see Section 9
│   ├── WebBFF/                    # new, not in upstream — see Section 9
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

`eShop.slnx`/`eShop.Web.slnf` deliberately do **not** list all 18 of this fork's target projects upfront the way the source repo lists all 19 of its own from day one — each project is added to them the moment it actually lands, so `dotnet build` and CI only ever try to build what exists. See `todo.md`'s "Still to do" for why this matters.

## 4. Services breakdown

- **Foundation/shared** (added or in progress first, everything else depends on these): `Shared` (linked-source utilities, not a standalone project — no `.csproj`, files are compiled directly into consuming projects), `EventBus` (abstractions), `EventBusRabbitMQ` (RabbitMQ implementation), `eShop.ServiceDefaults` (Aspire telemetry/health-check/resilience defaults shared by every service), `IntegrationEventLogEF` (EF Core-backed transactional outbox for integration events).
- **Domain APIs**: `Identity.API` (Duende IdentityServer — auth/OIDC provider for the whole system), `Catalog.API`, `Basket.API` (Redis-backed), `Ordering.API` + `Ordering.Domain` + `Ordering.Infrastructure` (domain-driven design split: domain model, EF Core persistence, and the API surface are three separate projects), `Webhooks.API`.
- **Background workers**: `OrderProcessor`, `PaymentProcessor` — queue-driven, no HTTP surface.
- **Sample integration consumer**: `WebhookClient` — a sample app demonstrating consuming `Webhooks.API`'s outbound notifications.
- **Web frontends**: `WebApp` (React storefront — see Section 9 for why this diverges from upstream's Blazor frontend), `WebBFF` (new project, not in upstream — backend-for-frontend for `WebApp`, see Section 9), `ClientApp` (.NET MAUI native client, unaffected by that change).
- **Orchestration**: `eShop.AppHost` — the Aspire AppHost project. References every other project to wire up local orchestration, service discovery, and the Aspire dashboard. Added last in the migration by design, since it can't meaningfully exist until the things it orchestrates do.

## 5. Package management and tooling

- **.NET 10** (SDK pinned to `10.0.400` in `global.json`, stable — not the preview channel the source snapshot was on), **.NET Aspire 13.4.6**.
- **Central NuGet package management** via `Directory.Packages.props` — every version pin individually researched against actual current-latest rather than copied from source. See `todo.md`'s "Directory.Packages.props research" for the notable deviations (license-driven pins, packages that diverged from a shared version variable, etc.).
- **`.slnx`** (the newer XML solution format) instead of the legacy `.sln`, plus **`.slnf`** (a solution filter) for the Windows/non-MAUI dev loop — `eShop.Web.slnf` excludes `ClientApp`/`ClientApp.UnitTests` since those need the MAUI workload.
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

**All build/test workflows are green** — not because everything's built, but because the workflows and solution files only ever reference what actually exists on disk, with the steps that structurally can't pass yet (needing `ClientApp`/`eShop.AppHost`) explicitly commented out and tracked rather than left red. See `todo.md`'s "CI status" for the live detail per workflow.

## 8. Decorator for cross-cutting concerns

This is a deliberately designed fork, not a copy-paste port — upstream's structure is a starting point to evaluate, not a constraint to preserve. The clearest example so far: `EventBusRabbitMQ`'s upstream `RabbitMQEventBus` class owned RabbitMQ connection/channel/publish/consume plumbing *and* OpenTelemetry tracing *and* Polly retry logic, all in one class. Digging into why turned up two real bugs living in that mixing — a null-conditional (`?.`) that silently never threw the exception it looked like it was guarding, and a Polly `Execute` vs `ExecuteAsync` mismatch that made the retry pipeline inert for the exact exceptions it was configured to catch (verified against the actual `Polly.Core` assembly, not assumed — see `todo.md`'s `EventBusRabbitMQ` section for both).

The fix was a **Decorator split**: `IEventBus` is now implemented three times — a bare `RabbitMQEventBus` (transport only), wrapped by `TelemetryEventBusDecorator` (OpenTelemetry), wrapped by `ResilientEventBusDecorator` (Polly retry, now using the correct async API). Each concern is isolated enough to reason about — and get right — on its own.

That's the template for the rest of this build, not a one-off. `eShop.ServiceDefaults` already reuses the same idea without new code: `AddServiceDefaults`'s `ConfigureHttpClientDefaults(http => { http.AddStandardResilienceHandler(); http.AddServiceDiscovery(); })` is the framework's own `DelegatingHandler`-chain Decorator, applied automatically to every service that calls it. `Ordering.API` (MediatR's `IPipelineBehavior<TRequest,TResponse>` is the same idea applied to the CQRS pipeline) is next in line. Not every project needs the same treatment, though — e.g. `Catalog.API` (EF Core-backed) and `Basket.API` (Redis-backed) call for different data-access patterns given how differently queryable their stores are, a call to make honestly when each project is actually added rather than forced into one shape now.

## 9. Frontend: React instead of Blazor

Upstream's `WebApp` is a Blazor storefront, with `WebAppComponents` sharing Razor components between it and `HybridApp` (a Blazor Hybrid wrapper embedded in the native mobile shell). This fork drops Blazor for the browser frontend entirely — `WebApp` is a React app instead, and `WebAppComponents`/`HybridApp` are dropped from the plan rather than converted, since they only existed to share Blazor components that no longer exist once `WebApp` isn't Blazor. `ClientApp` (native .NET MAUI) is unaffected — it was always a separate client in [eShop's own architecture diagram](../img/eshop_architecture.png) (`Mobile App` → `Mobile BFF`/`Mobile API`), distinct from the browser-facing `Web App` box that's actually changing here.

**Second consequence: `WebApp` needs its own BFF.** Blazor Server got request aggregation/orchestration for free — its C# runs server-side, so it implicitly acted as its own backend-for-frontend. A React SPA has no server-side execution at all, so that implicit BFF disappears along with Blazor, not just the rendering technology. The diagram already establishes this exact shape for mobile (`Mobile App` → `Mobile BFF` → `Mobile API`) — `WebApp` picks up the same pattern instead of the browser calling `Catalog.API`/`Ordering.API`/`Basket.API` directly. Planned as a new `WebBFF` project using **`Duende.BFF`** specifically — `Identity.API` is already Duende IdentityServer, and Duende publishes that package for exactly this SPA-plus-their-own-IdentityServer scenario. It keeps OAuth tokens server-side (an httpOnly session cookie to the browser instead of a JWT sitting in reachable JS), closing off token-theft-via-XSS as an attack surface, rather than the SPA managing tokens/CORS against several services itself. Targeted for the week after 2026-08-14, not built yet — tracked in `todo.md`'s "Still to do" table.

**Why:** a React frontend paired with an ASP.NET Core Web API backend is a far more common, more in-demand combination in the .NET job market than a Blazor frontend — this repo is a job-search portfolio piece, and demonstrating a genuine full-stack split (C# backend, TypeScript frontend) is worth more than staying single-language for its own sake.

**The real technical consequence, not just a frontend swap:** the architecture diagram shows `Basket API` called via **gRPC** directly from the Blazor Web App — something Blazor Server can do natively (the gRPC client runs server-side in .NET) that a browser-based React SPA fundamentally cannot (browsers don't support the HTTP/2 trailers native gRPC needs). Decided **not** to replace gRPC with REST for `Basket.API`, and **not** to duplicate it with a second REST surface — instead, `Basket.API` will add ASP.NET Core's first-party `Grpc.AspNetCore.Web` middleware, letting the same gRPC service handle native gRPC (server-to-server, e.g. `Mobile BFF`/`Ordering.API`) *and* gRPC-Web (the React frontend, via a client like `@connectrpc/connect-web`) without a separate proxy service. This keeps gRPC as `Basket.API`'s genuine service contract — matching the diagram's intent and worth demonstrating on its own — while solving the browser problem with the minimal, Microsoft-supported path rather than guessing at a workaround when `Basket.API` actually gets built.

## 10. Where to go next

- Live progress and evidence trail: [todo.md](../todo.md)
- Per-project Kanban status: [GitHub Project board](https://github.com/users/Terrence721/projects/5)
- Getting started / running the solution: [README.md](../README.md)
