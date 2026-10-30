<!-- markdownlint-disable-next-line MD041 -->
![eShop — A .NET Aspire Microservices Platform](img/readme-banner.png)

[![eShop Pull Request Validation](https://github.com/Terrence721/eshop-full/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/Terrence721/eshop-full/actions/workflows/pr-validation.yml)
[![CodeQL](https://github.com/Terrence721/eshop-full/actions/workflows/codeql.yml/badge.svg)](https://github.com/Terrence721/eshop-full/actions/workflows/codeql.yml)

**[📜 View the portfolio page →](https://terrence721.github.io/eshop-full/portfolio.html)**

Last updated: October 24, 2026 (migration complete — all 21 target projects added and reviewed; 785 tests passing — 514 .NET across 14 test projects + 271 frontend (118 `Identity.WebApp`, 52 `WebhookClient.WebApp`, 101 `WebApp`); full Playwright e2e suite (Add/Remove/Browse items) passes against a live Aspire host; project board 71/71 Done — see `todo.md`)

This is an independently modernized version of Microsoft's [dotnet/eShop](https://github.com/dotnet/eShop) reference app — a .NET Aspire microservices e-commerce platform (product catalog, basket, ordering, identity, payments, outbound webhooks) added **one file at a time**, each file evaluated and upgraded against actual current-latest package versions rather than copied over wholesale.

Not a fork left as-is. Every package version was individually re-researched, several real bugs were found and fixed in Microsoft's own reference source (verified against real assemblies and real builds, not assumed), and a handful of deliberate design departures — a Decorator split for the event bus (`EventBusRabbitMQ`, later replaced outright by `EventBusKafka`), React instead of Blazor for the web frontend — were made and recorded as this fork's own choices.

**At a glance:** 785/785 tests passing — 514 .NET across 14 test projects plus 271 frontend (118 in `Identity.WebApp`, 52 in `WebhookClient.WebApp`, 101 in `WebApp`), plus the full Playwright e2e suite (Add/Remove/Browse items) passing against a live Aspire host — see [`todo.md`](todo.md) and the **[Testing Strategy diagram](https://terrence721.github.io/eshop-full/diagrams/testing-strategy.html)**.

## 🧭 Start Here

- **[System Architecture](https://terrence721.github.io/eshop-full/diagrams/system-architecture.html)** — the 21-project target layout, four layers, and exactly what's real today
- **[Event Flow](https://terrence721.github.io/eshop-full/diagrams/event-flow.html)** — the `EventBus`/`EventBusKafka` Decorator chain, and the two real bugs found while building its RabbitMQ predecessor
- **[Projects Reference](https://terrence721.github.io/eshop-full/diagrams/projects-reference.html)** — every project, its real one-line role, and its status
- **[Testing Strategy](https://terrence721.github.io/eshop-full/diagrams/testing-strategy.html)** — the patterns behind 186 passing .NET tests, and the fix a Decorator split finally let get proven end-to-end

The [wiki](https://github.com/Terrence721/eshop-full/wiki) goes deeper per completed piece of work, each page linking back to the real source rather than repeating it.

- **[`todo.md`](todo.md)** — the evidence-backed log of everything done and everything still open, with commit hashes. This is the source of truth for progress.
- **[GitHub Project board](https://github.com/users/Terrence721/projects/5)** — a Scrum-style Backlog/Planned/In Progress/Verification & QA/Done view of the same work. Kept in sync with [`todo.md`](todo.md).
- **[`docs/architecturedesign.md`](docs/architecturedesign.md)** — the reasoning behind this repo's architectural decisions, verified against the real source app rather than described generically.
- **[`portfolio.html`](https://terrence721.github.io/eshop-full/portfolio.html)** — this repo as a portfolio piece: real bugs found, real design decisions made, and why, for anyone scanning it rather than reading it as documentation.
- **[CONTRIBUTING.md](./CONTRIBUTING.md)** — development setup and contribution principles.

On AI-assisted development: this project is built with Claude Code, directed, reviewed, and merged by Terrence Daniels file by file — every commit is a deliberate, individually-approved change, not a bulk AI drop. No commit carries an AI co-author trailer; authorship stays Terrence Daniels throughout.

## 🧭 Why This Matters

.NET Aspire microservices, event-driven integration through a message bus, and a transactional outbox pattern show up constantly on resumes and rarely get built end-to-end with the reasoning behind each decision written down. `dotnet/eShop` is Microsoft's own teaching reference for exactly this shape — a genuinely useful thing to rebuild file by file rather than fork wholesale, since it means every package version gets re-verified, every file gets read closely enough to catch what's actually wrong with it, and every design choice that diverges from upstream is a deliberate call, not an oversight.

## 🏗 What's Here So Far

The migration is complete: all 21 target projects are on disk and reviewed — `Shared` (linked-source utilities), `EventBus` + `EventBusKafka` (the 3-layer Decorator chain, Kafka after a RabbitMQ predecessor was replaced outright), `eShop.ServiceDefaults`, `IntegrationEventLogEF`, `Identity.API`, `Identity.WebApp`, `Catalog.API`, `Basket.API`, `Ordering.Domain`/`Ordering.Infrastructure`/`Ordering.API`, `OrderProcessor`, `PaymentProcessor`, `Webhooks.API`, `WebhookClient` (backend + `WebhookClient.WebApp`), `WebApp` (React, not Blazor), `WebBFF` (Duende.BFF), `ClientApp` (Avalonia, not MAUI), and `eShop.AppHost` (the Aspire orchestrator). Each was added one file at a time, evaluated and upgraded against actual current-latest versions rather than copied wholesale. See [`todo.md`](todo.md) for the evidence-backed log.

```text
  Shared/                 linked-source utilities                          ✅ done
  EventBus/                transport-agnostic event abstractions            ✅ done
  EventBusKafka/           Kafka implementation, 3-layer Decorator          ✅ done
  eShop.ServiceDefaults/   Aspire telemetry/health-check/resilience         ✅ done
  IntegrationEventLogEF/  EF Core transactional outbox                     ✅ done
  Identity.API/            Duende IdentityServer (OIDC)                     ✅ done
  Identity.WebApp/         React replacement for Quickstart UI (new)        ✅ done
  Catalog.API/             product catalog, v1/v2 API                       ✅ done
  Basket.API/              Redis-backed cart (gRPC + gRPC-Web)              ✅ done
  Ordering.Domain/.Infrastructure/.API/  order placement (DDD split)        ✅ done
  OrderProcessor/          background worker                               ✅ done
  PaymentProcessor/        background worker                               ✅ done
  Webhooks.API/            outbound webhook notifications                  ✅ done
  WebhookClient/           sample Webhooks.API consumer                    ✅ done
  WebApp/                  React storefront (not Blazor)                   ✅ done
  WebBFF/                  Backend-for-Frontend, Duende.BFF (new)          ✅ done
  ClientApp/               native app, Avalonia UI (not MAUI)              ✅ done
  eShop.AppHost/           Aspire orchestrator                             ✅ done
```

## 🖥 Getting Started

**Runnable end-to-end** — `eShop.AppHost` now orchestrates every service, their container dependencies (PostgreSQL, Kafka, Redis), and inter-service discovery. Start it with `npm run run:aspire` (or `pwsh -NoProfile -File scripts/Start-Aspire.ps1`) and open the Aspire dashboard. See [`todo.md`](todo.md) for the full build-out log. What follows is prerequisite setup.

- Clone this repository: `https://github.com/Terrence721/eshop-full`
- [Install & start Docker Desktop](https://docs.docker.com/engine/install/)
- Install the latest [.NET 10 SDK](https://dot.net/download?cid=eshop)

### Windows with Visual Studio

Install [Visual Studio 2022 version 17.10 or newer](https://visualstudio.microsoft.com/vs/) with the `ASP.NET and web development` workload and the `.NET Aspire SDK` component (`Individual components`) — or run the WinGet configuration script:

```powershell
install-Module -Name Microsoft.WinGet.Configuration -AllowPrerelease -AcceptLicense -Force
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
get-WinGetConfiguration -file .\.config\configuration.vs.winget | Invoke-WinGetConfiguration -AcceptConfigurationAgreements
```

### VS Code (Windows, Mac, or Linux)

This repo is actively developed in [VS Code with C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started) — see [`.vscode/extensions.json`](.vscode/extensions.json) for the exact extension set this repo uses. On Windows, run the WinGet configuration script instead of installing manually:

```powershell
install-Module -Name Microsoft.WinGet.Configuration -AllowPrerelease -AcceptLicense -Force
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
get-WinGetConfiguration -file .\.config\configuration.vsCode.winget | Invoke-WinGetConfiguration -AcceptConfigurationAgreements
```

> On Mac with Apple Silicon, Rosetta 2 is needed for `grpc-tools`.

### Building what exists today

```powershell
dotnet build eShop.Web.slnf
```

`eShop.slnx`/`eShop.Web.slnf` now list every project that exists on disk, so this builds cleanly against the complete (21-project) migration.

### Running tests

Functional/integration test projects (for example `tests/Catalog.FunctionalTests` and `tests/Ordering.FunctionalTests`) are backed by the .NET Aspire AppHost and their container dependencies (PostgreSQL, Kafka/Redpanda, Redis, etc.). **Docker Desktop and Aspire must be up and running before running any of these tests** — running them with the Docker daemon stopped (or with no Aspire host running) will fail trying to connect to the infrastructure endpoints:

- Start Docker Desktop first.
- Start the Aspire orchestrator with `npm run run:aspire` (or `pwsh -NoProfile -File scripts/Start-Aspire.ps1`) and wait for the dashboard/resources to be healthy.
- Then run the tests, for example: `dotnet test tests/Ordering.FunctionalTests/Ordering.FunctionalTests.csproj`.

Unit test projects that do not need services can run directly with `dotnet test`, but use the same Docker + Aspire-first rule for anything that launches an AppHost or reaches live infrastructure.

### Running the client app hosts

With npm:

```powershell
npm run run:clientapp:all
```

With Yarn:

```powershell
yarn run run:clientapp:all
```

Those launch the desktop, browser, and Android hosts (the Android target still requires an emulator to be running first).

## Contributing

For more information on contributing to this repo, read [the contribution documentation](./CONTRIBUTING.md) and [the Code of Conduct](CODE-OF-CONDUCT.md).

## Acknowledgment

Built from Microsoft's [dotnet/eShop](https://github.com/dotnet/eShop) reference application. For the original, upstream-maintained version — including Azure OpenAI integration and Azure Developer CLI deployment, both out of scope until this fork is runnable end-to-end — see the source repo directly.

<!-- trigger 2 -->
