# Architecture Overview

<!-- markdownlint-disable-next-line MD036 -->
**Last Updated: August 29, 2026**

This document describes the architecture eShop-full is being built toward — verified against the real source app (Microsoft's [dotnet/eShop](https://github.com/dotnet/eShop), snapshotted locally at `F:\eShop-main\eShop-main`) and against what has actually landed in this repo, not a generic description of what an e-commerce microservices app "usually" looks like. See [todo.md](../todo.md) for exactly how much of this exists right now, and the [project board](https://github.com/users/Terrence721/projects/5) for the live per-project status.

**Current status, stated plainly:** as of this writing, `src/Shared/` (2 linked-source files), `EventBus`, `EventBusRabbitMQ`, `eShop.ServiceDefaults`, `IntegrationEventLogEF`, and `Identity.API` are all added and reviewed, with complete test coverage (33, 16, 17, 17, and 91 passing tests respectively, 174 total) — see Section 11. `Identity.API`'s full source tree (`Models`/`Configuration`/`Data`/`Quickstart`/`Services`/`Program.cs`) is done, its Quickstart UI converted directly to a JSON API rather than added as Razor UI first (see Section 9). `Identity.WebApp`'s scaffold is added (real Vite/React/TypeScript project, verified building and linting clean) with its `Home` page's two actions (`Index`/`Error`) fully done — router wired, `App.tsx` placeholder retired — against a real dev-proxy to `Identity.API`. Its `Account` area has two real backend bugs found and fixed directly in `Identity.API` (`LoginPostResult` was missing a `ValidationError` field; `AccountController.Login` wasn't surfacing it) but no frontend pages yet — the rest of its Quickstart-area pages are still open. The other 15 projects don't exist on disk yet. Everything below describing the full system is the target this repo is being built toward one file at a time, not a claim that it already runs end-to-end.

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
│   ├── IntegrationEventLogEF/   ✅ added and reviewed
│   ├── Identity.API/            ✅ added and reviewed
│   ├── Identity.WebApp/           # new, not in upstream — see Section 9
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
├── tests/                        ✅ 5 test projects added (174 tests total) — see Section 11.
│                                   # Upstream's own 5 (Basket.UnitTests, Catalog.FunctionalTests,
│                                   # Ordering.FunctionalTests, Ordering.UnitTests, ClientApp.UnitTests)
│                                   # land alongside the projects they test, not batched at the end.
├── build/                         # Build tooling from source repo
├── e2e/                          ✅ Playwright e2e specs
├── docs/                         ✅ this folder
├── .github/                      ✅ GitHub Actions workflows + dependabot.yml
├── global.json, Directory.Build.props/.targets, Directory.Packages.props, nuget.config  ✅
└── eShop.slnx, eShop.Web.slnf    ✅ kept trimmed to only projects that exist — see todo.md
```

`eShop.slnx`/`eShop.Web.slnf` deliberately do **not** list all 19 of this fork's target `src/` projects upfront the way the source repo lists all 19 of its own from day one (the count matches upstream's by coincidence: `-2` `WebAppComponents`/`HybridApp`, `+1` `WebBFF`, `+1` `Identity.WebApp`, net zero) — each project is added to them the moment it actually lands, so `dotnet build` and CI only ever try to build what exists. See `todo.md`'s "Still to do" for why this matters.

## 4. Services breakdown

See [todo.md's Milestones section](../todo.md#-milestones) for the current done/in-progress/not-started status of each layer below.

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

The fix was a **Decorator split**: `IEventBus` is now implemented three times — a bare `RabbitMQEventBus` (transport only), wrapped by `TelemetryEventBusDecorator` (OpenTelemetry), wrapped by `ResilientEventBusDecorator` (Polly retry, now using the correct async API). Each concern is isolated enough to reason about — and get right — on its own. The split paid off for testing too, not just design clarity: decorators wrap *any* `IEventBus`, so `ResilientEventBusDecoratorTests.cs` verified the retry fix against a fake inner bus that fails then succeeds — closing a gap that had sat unverified since the original fix, since no RabbitMQ broker exists yet to test the real transport end-to-end.

That's the template for the rest of this build, not a one-off. `eShop.ServiceDefaults` already reuses the same idea without new code: `AddServiceDefaults`'s `ConfigureHttpClientDefaults(http => { http.AddStandardResilienceHandler(); http.AddServiceDiscovery(); })` is the framework's own `DelegatingHandler`-chain Decorator, applied automatically to every service that calls it. `Ordering.API` (MediatR's `IPipelineBehavior<TRequest,TResponse>` is the same idea applied to the CQRS pipeline) is next in line. Not every project needs the same treatment, though — e.g. `Catalog.API` (EF Core-backed) and `Basket.API` (Redis-backed) call for different data-access patterns given how differently queryable their stores are, a call to make honestly when each project is actually added rather than forced into one shape now.

## 9. Frontend: React instead of Blazor

Upstream's `WebApp` is a Blazor storefront, with `WebAppComponents` sharing Razor components between it and `HybridApp` (a Blazor Hybrid wrapper embedded in the native mobile shell). This fork drops Blazor for the browser frontend entirely — `WebApp` is a React app instead, and `WebAppComponents`/`HybridApp` are dropped from the plan rather than converted, since they only existed to share Blazor components that no longer exist once `WebApp` isn't Blazor. `ClientApp` (native .NET MAUI) is unaffected — it was always a separate client in [eShop's own architecture diagram](../img/eshop_architecture.png) (`Mobile App` → `Mobile BFF`/`Mobile API`), distinct from the browser-facing `Web App` box that's actually changing here.

**Second consequence: `WebApp` needs its own BFF.** Blazor Server got request aggregation/orchestration for free — its C# runs server-side, so it implicitly acted as its own backend-for-frontend. A React SPA has no server-side execution at all, so that implicit BFF disappears along with Blazor, not just the rendering technology. The diagram already establishes this exact shape for mobile (`Mobile App` → `Mobile BFF` → `Mobile API`) — `WebApp` picks up the same pattern instead of the browser calling `Catalog.API`/`Ordering.API`/`Basket.API` directly. Planned as a new `WebBFF` project using **`Duende.BFF`** specifically — `Identity.API` is already Duende IdentityServer, and Duende publishes that package for exactly this SPA-plus-their-own-IdentityServer scenario. It keeps OAuth tokens server-side (an httpOnly session cookie to the browser instead of a JWT sitting in reachable JS), closing off token-theft-via-XSS as an attack surface, rather than the SPA managing tokens/CORS against several services itself. Targeted for the week after 2026-08-14, not built yet — tracked in `todo.md`'s "Still to do" table.

**Why:** a React frontend paired with an ASP.NET Core Web API backend is a far more common, more in-demand combination in the .NET job market than a Blazor frontend — this repo is a job-search portfolio piece, and demonstrating a genuine full-stack split (C# backend, TypeScript frontend) is worth more than staying single-language for its own sake.

**The real technical consequence, not just a frontend swap:** the architecture diagram shows `Basket API` called via **gRPC** directly from the Blazor Web App — something Blazor Server can do natively (the gRPC client runs server-side in .NET) that a browser-based React SPA fundamentally cannot (browsers don't support the HTTP/2 trailers native gRPC needs). Decided **not** to replace gRPC with REST for `Basket.API`, and **not** to duplicate it with a second REST surface — instead, `Basket.API` will add ASP.NET Core's first-party `Grpc.AspNetCore.Web` middleware, letting the same gRPC service handle native gRPC (server-to-server, e.g. `Mobile BFF`/`Ordering.API`) *and* gRPC-Web (the React frontend, via a client like `@connectrpc/connect-web`) without a separate proxy service. This keeps gRPC as `Basket.API`'s genuine service contract — matching the diagram's intent and worth demonstrating on its own — while solving the browser problem with the minimal, Microsoft-supported path rather than guessing at a workaround when `Basket.API` actually gets built.

**The same swap applies to `Identity.API`'s own UI, decided 2026-08-20 — and converted directly, not added as Razor first, per a 2026-08-24 pivot.** Duende IdentityServer's Quickstart scaffold (`Account`/`Consent`/`Device`/`Diagnostics`/`Grants`) is server-rendered Razor MVC, the same rendering model being dropped everywhere else in this fork's browser-facing surface. All of it goes React, in a new `Identity.WebApp` project rather than co-located in `Identity.API`'s own `wwwroot` — consistent with the `WebApp`/`WebBFF` split rather than a one-off exception for the login flow. The original plan was to add the Quickstart UI as Razor first and convert it once `Identity.WebApp` work started; the user pivoted that timing to avoid doing the work twice — `Identity.API`'s Quickstart controllers converted straight to a JSON API (`ActionResult<T>`/`Ok(...)` instead of `View(...)`), no Razor views or `wwwroot` assets added at all. `Home`/`Grants`/`Diagnostics`/`Consent`/`Device`/`Account`/`External` are all done this way — the whole Quickstart UI is converted. Anti-forgery (`[ValidateAntiForgeryToken]`, tied to server-rendered forms) is dropped from every converted action — a known, deliberately deferred gap, to revisit once `Identity.WebApp`'s real deployment topology is known. `Identity.WebApp`'s own scaffold is added (2026-08-28) — a pure Yarn workspace, no `.csproj`/`.esproj` project file (see `todo.md`'s "Frontend and API layer" section for the real, documented reasons: a Visual Studio JS Project System bug in VS Code's C# Dev Kit, and the classic ASP.NET Core SPA-hosting pattern being explicitly labeled "legacy" by Microsoft's own current docs). `Home`'s two pages are done against a real dev-proxy to a genuinely running `Identity.API`; the rest of the Quickstart-area pages are still open.

## 11. Testing strategy

Framework: **`MSTest.Sdk`** on .NET's newer **`Microsoft.Testing.Platform`** (MTP) runner — not a new choice made for testing specifically, `global.json` already pinned both from the original SDK/build-config research, and upstream's own `tests/` folder is MSTest-based too. MTP is a genuinely different CLI surface from the older VSTest-based `dotnet test` (its own `--help` states it plainly: "doesn't support VSTest") — coverage (`--coverage --coverage-output-format cobertura`) and TRX reporting (`--report-trx`) are both built into the runner itself, confirmed against a real scratch project rather than assumed. CI (`pr-validation.yml`) publishes results via [`dorny/test-reporter`](https://github.com/dorny/test-reporter) as a PR-visible check run and uploads the coverage artifact — verified end-to-end against a real GitHub Actions run, not just written and hoped to work.

**Testing is no longer a separate end-of-migration phase.** The original plan deferred all of `tests/` to its own migration slot, batched after every service and frontend existed. That's reversed: `tests/eShop.ServiceDefaults.UnitTests` (**all 7 source files, 33 passing tests**), `tests/EventBus.UnitTests` (**all 6 applicable source files, 16 passing tests**), `tests/EventBusRabbitMQ.UnitTests` (**all 6 applicable source files, 17 passing tests**), and `tests/IntegrationEventLogEF.UnitTests` (**all 5 applicable source files, 17 passing tests**) were all added and grown to full coverage — 83 tests total. `tests/Identity.API.UnitTests` (**91 passing tests**) followed the same "every applicable file gets covered" bar, but on a deliberately different schedule: given `Identity.API`'s much larger source tree (129 files vs. single-digit-to-low-teens for every project before it), its test project was deferred until the whole `.csproj`'s source was complete, rather than built file-by-file alongside it — 174 tests total, solution-wide. Going forward, projects at the smaller, single-digit-to-low-teens file-count scale go back to getting their own test project in the same unit of work as their source files; a project `Identity.API`'s size or larger defers testing the same way. `src/Shared/` gets the same treatment despite not being a `.csproj` project itself, since its linked-source files are real logic other projects depend on.

**Not every source file needs a test file — and the bar for "not every" is narrower than it sounds.** `EventBus`'s `IEventBus`/`IEventBusBuilder` are pure interfaces with zero behavior of their own — nothing to assert at runtime that compile-time checking across the rest of the solution doesn't already guarantee. The rule isn't "skip simple files": a bare `enum` and a single-default-value POCO both have real tests elsewhere in this repo, because both guard a real contract (persisted ordinals, a default value). What genuinely gets skipped is a file with zero contract to guard at all — a pure interface, a `GlobalUsings.cs`, a plain data-holder with no defaults and no computed properties. Stated explicitly per file, not silently assumed.

Not every method in a file is a pure-unit-test candidate, and that's stated explicitly rather than papered over: `MapDefaultEndpoints` (needs a live `WebApplication`/Kestrel listener), the OTLP-exporter branch of `AddOpenTelemetryExporters` (only observable via OpenTelemetry SDK internals), and `AddDefaultOpenApi`'s real `IApiVersioningBuilder`-wiring path plus all of `UseDefaultOpenApi`'s route behavior (needs a live server and three NuGet packages' extension-method interplay not yet verified against a real API) are documented as genuine integration-test territory in `todo.md`, not silently skipped.

Two patterns worth carrying forward, both discovered while building `eShop.ServiceDefaults.UnitTests`:

- **`InternalsVisibleTo` for `internal` classes worth testing directly.** `OpenApiOptionsExtensions` (and the whole class it lives in) is `internal` — nothing in it is reachable from a test assembly without explicit visibility. Rather than reach for reflection (brittle, tests implementation detail instead of behavior) or skip coverage of logic that had already shipped one real bug, the fix was the idiomatic one: widen the specific members to `internal` and add `<InternalsVisibleTo Include="{ProjectName}.UnitTests" />` to the source project. No public API changes.
- **`NSubstitute` for behavior that only exists behind a DI/HTTP pipeline.** `HttpClientExtensions`'s bearer-token-injecting `DelegatingHandler` is a *private* nested class — the only way to exercise it is through `AddAuthToken`'s public surface, a real `IServiceCollection`, and a mocked `IAuthenticationService` standing in for the ASP.NET Core auth pipeline. `NSubstitute` was already a centrally-pinned package (upstream's own `Ordering.UnitTests` uses it), so this isn't a new dependency, just its first real use in this fork.
- **`ServiceCollection` no longer requires the full DI package — `BuildServiceProvider()` still does.** Testing `EventBusBuilderExtensions`'s DI-registration methods needed a real container. `Microsoft.Extensions.DependencyInjection.Abstractions` now bundles the concrete `ServiceCollection` class itself, confirmed by reflecting the actual assembly rather than assumed from older API-shape knowledge — but the working container (`ServiceProvider`/`BuildServiceProvider()`) still needs the separate `Microsoft.Extensions.DependencyInjection` package, added and centrally versioned once this surfaced.
- **Extracting a reflection-coupled seam into a pure function, not mocking around it.** `IntegrationEventLogService<TContext>`'s event-type resolution was `private static` logic hard-wired to `Assembly.GetEntryAssembly()`, which made its new collision-detection branch untestable in isolation (any two colliding test-event types anywhere in the test assembly would break every other test sharing that generic class's static field). Rather than reach for a mocking seam, it moved into its own `internal static IntegrationEventTypeResolver` class taking plain `IEnumerable<Type>`/`IReadOnlyDictionary<string, Type>` inputs — the reflection dependency stays at the one real call site, and the logic worth testing becomes a pure function testable with hand-built inputs.

**Honest gap, tracked not solved:** wanted one combined HTML report across every test project, not one file per project (confirmed via a real 2-project scratch solution that `--report-html` produces per-project output, same as `--report-trx`). The real fix — [microsoft/testfx#10529](https://github.com/microsoft/testfx/pull/10529), "Add HTML report artifact consolidation" — merged 2026-08-09 but hasn't shipped in a `Microsoft.Testing.Extensions.HtmlReport` NuGet release yet (still `2.3.3`, published 2026-07-28, checked directly against the NuGet API). Dependabot already tracks that package, so no new tooling is needed to know when it ships — see `todo.md`'s "Testing strategy" section and the tracked Backlog card.

## 12. Where to go next

- Live progress and evidence trail: [todo.md](../todo.md)
- Per-project Kanban status: [GitHub Project board](https://github.com/users/Terrence721/projects/5)
- Getting started / running the solution: [README.md](../README.md)
