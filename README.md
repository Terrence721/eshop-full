# 🛍️ eShop — A .NET Aspire Microservices Platform

[![eShop Pull Request Validation](https://github.com/Terrence721/eshop-full/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/Terrence721/eshop-full/actions/workflows/pr-validation.yml)
[![CodeQL](https://github.com/Terrence721/eshop-full/actions/workflows/codeql.yml/badge.svg)](https://github.com/Terrence721/eshop-full/actions/workflows/codeql.yml)

**[📜 View the portfolio page →](https://terrence721.github.io/eshop-full/portfolio.html)**

Last updated: August 28, 2026 (5 of 21 projects done — `EventBus`, `EventBusRabbitMQ`, `eShop.ServiceDefaults`, `IntegrationEventLogEF`, `Identity.API`, plus `Shared` — 174/174 tests passing; `Identity.WebApp` scaffold added, its `Home` page in progress)

This is an independently modernized version of Microsoft's [dotnet/eShop](https://github.com/dotnet/eShop) reference app — a .NET Aspire microservices e-commerce platform (product catalog, basket, ordering, identity, payments, outbound webhooks) added **one file at a time**, each file evaluated and upgraded against actual current-latest package versions rather than copied over wholesale.

Not a fork left as-is. Every package version was individually re-researched, several real bugs were found and fixed in Microsoft's own reference source (verified against real assemblies and real builds, not assumed), and a handful of deliberate design departures — a Decorator split for `EventBusRabbitMQ`, React instead of Blazor for the web frontend — were made and recorded as this fork's own choices.

**At a glance:** 174/174 tests passing across `eShop.ServiceDefaults.UnitTests` + `EventBus.UnitTests` + `EventBusRabbitMQ.UnitTests` + `IntegrationEventLogEF.UnitTests` + `Identity.API.UnitTests`, added ahead of the `tests/` migration slot so every completed project ships with full coverage rather than deferring it to the end — see the **[Testing Strategy diagram](https://terrence721.github.io/eshop-full/diagrams/testing-strategy.html)**.

## 🧭 Start Here

- **[System Architecture](https://terrence721.github.io/eshop-full/diagrams/system-architecture.html)** — the 21-project target layout, four layers, and exactly what's real today
- **[Event Flow](https://terrence721.github.io/eshop-full/diagrams/event-flow.html)** — the `EventBus`/`EventBusRabbitMQ` Decorator chain, and the two real bugs found while building it
- **[Projects Reference](https://terrence721.github.io/eshop-full/diagrams/projects-reference.html)** — every project, its real one-line role, and its status
- **[Testing Strategy](https://terrence721.github.io/eshop-full/diagrams/testing-strategy.html)** — the patterns behind 174 passing tests, and the fix a Decorator split finally let get proven end-to-end

The [wiki](https://github.com/Terrence721/eshop-full/wiki) goes deeper per completed piece of work, each page linking back to the real source rather than repeating it.

- **[`todo.md`](todo.md)** — the evidence-backed log of everything done and everything still open, with commit hashes. This is the source of truth for progress.
- **[GitHub Project board](https://github.com/users/Terrence721/projects/5)** — a Scrum-style Backlog/Planned/In Progress/Verification & QA/Done view of the same work. Kept in sync with [`todo.md`](todo.md).
- **[`docs/architecturedesign.md`](docs/architecturedesign.md)** — the reasoning behind this repo's architectural decisions, verified against the real source app rather than described generically.
- **[`portfolio.html`](https://terrence721.github.io/eshop-full/portfolio.html)** — this repo as a portfolio piece: real bugs found, real design decisions made, and why, for anyone scanning it rather than reading it as documentation.
- **[CONTRIBUTING.md](./CONTRIBUTING.md)** — development setup and contribution principles.

On AI-assisted development: Commits co-authored as Claude are AI-assisted implementations directed, reviewed, and merged by Terrence Daniels — same process as every other change, documented in docs/code-review.md.

## 🧭 Why This Matters

.NET Aspire microservices, event-driven integration through a message bus, and a transactional outbox pattern show up constantly on resumes and rarely get built end-to-end with the reasoning behind each decision written down. `dotnet/eShop` is Microsoft's own teaching reference for exactly this shape — a genuinely useful thing to rebuild file by file rather than fork wholesale, since it means every package version gets re-verified, every file gets read closely enough to catch what's actually wrong with it, and every design choice that diverges from upstream is a deliberate call, not an oversight.

## 🏗 What's Here So Far

`Shared` (linked-source utilities), `EventBus` (transport-agnostic event abstractions), `EventBusRabbitMQ` (the RabbitMQ implementation, split into a 3-layer Decorator chain after two real bugs turned up in the original single-class version), `eShop.ServiceDefaults` (Aspire telemetry/health-check/resilience defaults), `IntegrationEventLogEF` (the EF Core-backed transactional outbox every event-publishing service will write through), and `Identity.API` (Duende IdentityServer, its Quickstart UI converted straight to a JSON API rather than shipping Razor views) are complete, reviewed, and fully tested. `Identity.WebApp` (React, replacing Duende's Quickstart Razor UI) has its scaffold added and its `Home` page working against a real dev-proxy to `Identity.API` — the rest of its pages are still open. The other 13 projects don't exist on disk yet. See [`todo.md`](todo.md) for the full build-out plan and the honest current state.

```text
  Shared/                 linked-source utilities                          ✅ done
  EventBus/                transport-agnostic event abstractions            ✅ done
  EventBusRabbitMQ/        RabbitMQ implementation, 3-layer Decorator       ✅ done
  eShop.ServiceDefaults/   Aspire telemetry/health-check/resilience         ✅ done
  IntegrationEventLogEF/  EF Core transactional outbox                     ✅ done
  Identity.API/            Duende IdentityServer (OIDC)                     ✅ done
  Identity.WebApp/         React replacement for Quickstart UI (new)        🚧 scaffold added
  Catalog.API/             product catalog                                 ⬜ not started
  Basket.API/              Redis-backed cart (gRPC)                        ⬜ not started
  Ordering.Domain/.Infrastructure/.API/  order placement (DDD split)        ⬜ not started
  OrderProcessor/          background worker                               ⬜ not started
  PaymentProcessor/        background worker                               ⬜ not started
  Webhooks.API/            outbound webhook notifications                  ⬜ not started
  WebhookClient/           sample Webhooks.API consumer                    ⬜ not started
  WebApp/                  React storefront (not Blazor — see below)       ⬜ not started
  WebBFF/                  Backend-for-Frontend, Duende.BFF (new)          ⬜ not started
  ClientApp/               native .NET MAUI                                ⬜ not started
  eShop.AppHost/           Aspire orchestrator, added last                 ⬜ not started
```

## 🖥 Getting Started

**Not runnable end-to-end yet** — `eShop.AppHost` (the Aspire orchestrator every service registers with) is deliberately the last project added, since it can't meaningfully exist until the things it orchestrates do. See [`todo.md`](todo.md) for the honest current state. What follows is prerequisite setup, useful today regardless of how much of the app exists.

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

### Mac, Linux, and Windows without Visual Studio

[Visual Studio Code with C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started) is recommended — see [`.vscode/extensions.json`](.vscode/extensions.json) for the exact set this repo uses. Or run the equivalent WinGet configuration script on Windows:

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

`eShop.slnx`/`eShop.Web.slnf` only ever list projects that actually exist on disk, so this builds cleanly against the 5 done projects above without failing on anything not yet added.

## Contributing

For more information on contributing to this repo, read [the contribution documentation](./CONTRIBUTING.md) and [the Code of Conduct](CODE-OF-CONDUCT.md).

### Sample data

The sample catalog data is defined in [catalog.json](https://github.com/dotnet/eShop/blob/main/src/Catalog.API/Setup/catalog.json). Those product names, descriptions, and brand names are fictional and were generated using [GPT-35-Turbo](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt), and the corresponding [product images](https://github.com/dotnet/eShop/tree/main/src/Catalog.API/Pics) were generated using [DALL·E 3](https://openai.com/dall-e-3).

## Acknowledgment

Built from Microsoft's [dotnet/eShop](https://github.com/dotnet/eShop) reference application. For the original, upstream-maintained version — including Azure OpenAI integration and Azure Developer CLI deployment, both out of scope until this fork is runnable end-to-end — see the source repo directly.
