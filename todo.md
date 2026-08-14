# 📝 TODO

<!-- markdownlint-disable-next-line MD036 -->
**Last Updated: August 14, 2026**

A living list of what's done and what's left on this build. This is an independently modernized version of Microsoft's [dotnet/eShop](https://github.com/dotnet/eShop) reference app — a .NET Aspire microservices e-commerce platform added **one file at a time**, evaluated and upgraded as it comes in, not a wholesale copy of the source repo. See [docs/architecturedesign.md](docs/architecturedesign.md) for how it's put together and the [project board](https://github.com/users/Terrence721/projects/5) for the live per-project Kanban view of the migration itself.

## At a glance

**Done, in full:**

| Item | Detail |
|---|---|
| Repo bootstrap | git init, public GitHub repo live at [Terrence721/eshop-full](https://github.com/Terrence721/eshop-full) |
| SDK/build config | `global.json`, `Directory.Build.props`/`.targets`, `nuget.config` — see "SDK and build config" below |
| Central package versions | `Directory.Packages.props` — ~50 packages individually researched against actual current-latest, not copied as-is — see "Directory.Packages.props research" below |
| Dotfiles, solution files, CI | `.editorconfig`, `.gitattributes`, `.gitignore`, `eShop.slnx`/`eShop.Web.slnf`, `.github/workflows/*`, `.github/dependabot.yml` — see "Dotfiles, solution files, and CI" below |
| Package manager | Switched npm → Yarn 4.18.0 (Berry, node-modules linker) via corepack — see "Yarn + e2e" below |
| `src/Shared/` | Both linked-source files added, reviewed for code quality, no changes needed |
| `EventBus` | All 8 source files added — see "EventBus" below |
| `EventBusRabbitMQ` | All 6 source files added, one real bug found and fixed — see "EventBusRabbitMQ" below |

**Still to do:** 17 of 19 `.csproj` projects, plus `tests/` and `build/` — see the "Still to do" table below and [project board](https://github.com/users/Terrence721/projects/5) for the live board.

## ✅ Done

### SDK and build config

| File | Change | Why |
|---|---|---|
| `global.json` | SDK pin `10.0.100` + `allowPrerelease: true` → stable `10.0.400` (current as of 2026-08-11); `MSTest.Sdk` `4.0.2` → `4.3.3` | Source repo was written during .NET 10's preview cycle. .NET 10 is GA now — pinning to a prerelease channel is unnecessary risk (could silently pull .NET 11 previews) |
| `Directory.Build.props` | Removed `SuppressNETCoreSdkPreviewMessage` (no longer applicable); narrowed `NoWarn` from `NU1901;NU1902;NU1903;NU1904` to `NU1901;NU1902` | The source blanket-suppressed NuGet security-audit warnings for **all** severities, including high/critical. That's a real anti-pattern for a repo we're actively adding fresh packages to — we want to know if a critical vuln shows up in a transitive dependency |
| `Directory.Build.targets`, `nuget.config` | Copied as-is | Already current, nothing to change |

Commits: `56cc1e6`.

### Directory.Packages.props research

Every one of ~50 central package pins was checked against its actual current-latest version (web search, not assumption) rather than copied from source. Routine patch bumps aren't itemized here — the notable ones:

| Package(s) | Change | Detail |
|---|---|---|
| `Asp.Versioning.Http`/`.Mvc.ApiExplorer`/`.OpenApi` | `10.0.0-preview.2` → stable `10.0.0` | Hit GA since the source snapshot was taken |
| `Asp.Versioning.Http.Client` | Split out to `8.1.0` | Diverged from the rest of the `Asp.Versioning.*` family onto its own version line — pinning it to the same variable as the others would have been wrong |
| `Aspire.*` | `13.2.0` → `13.4.6` | Routine, but `Aspire.Azure.AI.OpenAI` stays on the unstable/preview variable — Microsoft genuinely hasn't shipped a stable release of that one yet |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design` | `8.0.0-rc.1.23461.3` → `10.0.2` | Was stuck on a .NET-8-RC-era prerelease; a real stable release exists now |
| `Duende.IdentityServer*` | Unused `DuendeVersion` variable (source declared `7.3.1` but every package was hardcoded to `7.3.2` instead) wired up properly; bumped to `8.x` | User decision (major version bump, breaking-change risk deferred to verification when `Identity.API` is actually added — see "Still to do") |
| `IdentityModel` → `Duende.IdentityModel` | `7.0.0` → `8.1.0`, package renamed | Upstream `IdentityModel` is archived; `Duende.IdentityModel` is the maintained successor, still Apache-2.0. Any `using IdentityModel;` code will need a namespace update when added |
| `MediatR` | Pinned to `12.5.0` | User decision. MediatR v13+ moved to a dual RPL-1.5/commercial license (LuckyPenny Software, July 2025); `12.5.0` is the last MIT/Apache-2.0 release |

Commit: `ffa2730`.

### Dotfiles, solution files, and CI

Added as-is (already current): `.editorconfig`, `.gitattributes`, `.gitignore`, `.spectral.yml`, markdownlint config, `eShop.slnx`, `eShop.Web.slnf`, `.aspire/settings.json`.

**Excluded** — Microsoft-internal, non-functional outside their org:

- `ci.yml` (root) — internal Azure DevOps pipeline (`1ESPipelineTemplates`, `NetCore1ESPool-Svc-Internal` agent pool). `.github/workflows/*` is the CI that actually runs on this public repo.
- `.config/CredScanSuppressions.json`, `.config/tsaoptions.json` — internal security-scanning tool config.
- `.devcenter/` — its only file (`imagedefinition.yaml`) contains literal placeholder text ("Initial Commit"), not a valid Dev Home image definition. Broken even upstream.
- `es-metadata.yml` — internal DevOps routing metadata (service GUID, `devdiv` org/path).

**Fixed while adding:**

- `README.md` — stale ".NET 9" reference (source's `global.json` already said 10) corrected; WinGet config file paths pointed at a nonexistent `.configurations/*.dsc.yaml` that doesn't match the actual `.config/*.winget` files on disk, even upstream.
- `.config/*.winget` — installed preview channels (`Microsoft.DotNet.SDK.Preview`, `VisualStudio.17.Preview`) switched to stable (`Microsoft.DotNet.SDK.10`, `VisualStudio.17.Release`); added the Aspire VS Code extension to the bootstrap list.
- `.github/workflows/*` — `actions/checkout` v4→v5, `actions/setup-dotnet` v3/v4→v5, `actions/setup-node` v4→v6, `actions/upload-artifact` v4→v6; dropped `dotnet-quality: 'preview'` now that .NET 10 is GA; `playwright.yml` switched from `npm ci` to `yarn install --frozen-lockfile`.
- `.github/dependabot.yml` — kept the existing NuGet grouping, added `npm` and `github-actions` ecosystems so the whole toolchain stays current, not just NuGet.

**Already working as designed:** Dependabot opened 4 real PRs against the actions versions above within hours of them landing — `actions/checkout` v5→v7, `actions/setup-dotnet` v5→v6, `actions/upload-artifact` v6→v7, `actions/setup-node` v4→v6→v7 (superseded twice). Not yet merged — see "Still to do".

Commit: `a0c162f`.

### Yarn + e2e

Source used npm (`package-lock.json`) for the Playwright e2e suite — the only JS in this repo. Switched to Yarn 4.18.0 (Berry) via corepack, using the `node-modules` linker rather than Berry's PnP default: this project's only JS usage is Playwright, and Playwright's browser-install tooling and VS Code extension both assume a standard `node_modules` layout.

Bumped: `@playwright/test` `1.42.1` → `1.62.1`, `dotenv` `16.4.5` → `17.4.2`, `@types/node` `^20` → `^24` (matching the Node 24 LTS line the CI workflows target via `lts/*`, not the non-LTS `@types/node` 26.x line).

`playwright.config.ts`'s `dotenv` load modernized from `require(...)` to an ES import, consistent with the `path` import already in that file. e2e spec files (`login.setup.ts`, `BrowseItemTest.spec.ts`, `AddItemTest.spec.ts`, `RemoveItemTest.spec.ts`) added unchanged.

Commits: `12fbf3e`, `a8da68a`, `14a82f2`, `6110d6f`, `f23d298`, `bb6d745`, `ca98dac`.

### Editor config

| File | Change | Why |
|---|---|---|
| `.vscode/settings.json` | `dotnet.defaultSolution` pinned to `eShop.slnx` | C# Dev Kit couldn't auto-detect which solution to load and showed a "No Solution" badge, disabling IntelliSense and build integration |

Commit: `0185981`.

### `src/Shared/`

Not a `.csproj` project — a shared-source folder linked directly into consuming projects (absent from `eShop.slnx` for exactly that reason).

- `ActivityExtensions.cs` — reviewed for code quality, left unchanged. No namespace is deliberate (global namespace lets any consuming project call `.SetExceptionTags()` without a `using`), and the internal null-check on the `Activity` receiver is load-bearing: it's an extension method, callable on a `null` receiver, and `RabbitMQEventBus.cs` (not yet added) relies on that by calling it without `?.`.
- `MigrateDbContextExtensions.cs` — reviewed for code quality; deliberately overrides `BackgroundService.StartAsync` (not `ExecuteAsync`) so host startup actually waits on the migration to finish before the app is considered ready. Correct, not an oversight. **Revisited 2026-08-14** for a DRY/SOLID pass, which found two real (if minor) issues — both present verbatim in real upstream Microsoft source, not local mistakes, so fixing them is a deliberate, documented exception to this project's usual "keep upstream architecture intact" rule:
  - `MigrateDbContextAsync` and the private `InvokeSeeder` had near-identical `StartActivity`/try-catch/`SetExceptionTags`/rethrow shapes — extracted into one private `RunWithActivityAsync` helper; external behavior (both activities tagged on failure, one log message) is unchanged.
  - `IDbSeeder<TContext>` moved out of this file into its own `Abstractions/IDbSeeder.cs` (same `Microsoft.AspNetCore.Hosting` namespace, so no consumer-facing change) for discoverability.

Commits: `3c263e4`, `5e65794` (original); `0c361d9`, `10df006` (2026-08-14 DRY/SOLID fixes).

### EventBus

`.csproj` added in `a2e37eb`. All 8 remaining source files reviewed for correctness and SOLID/DRY/composition-over-inheritance, added unchanged — none needed a fix:

- `GlobalUsings.cs`, `Events/IntegrationEvent.cs`, `Abstractions/IIntegrationEventHandler.cs`, `Abstractions/IEventBus.cs`, `Abstractions/IEventBusBuilder.cs` — all minimal and ISP-compliant; `IntegrationEvent`'s inheritance-based design (a base record other events extend for shared Id/CreationDate) is a legitimate "is-a" value-object hierarchy, not a smell.
- `Abstractions/EventBusSubscriptionInfo.cs` — an Options-pattern class bundling `EventTypes` + `JsonSerializerOptions`, the idiomatic shape for that pattern rather than an SRP violation; its AOT/trimming pragma suppressions are correctly scoped to one reflection-based fallback method.
- `Extensions/GenericTypeExtensions.cs` — the `object` overload delegates to the `Type` overload instead of duplicating its logic.
- `Extensions/EventBusBuilderExtensions.cs` — both methods share the `Services.Configure<EventBusSubscriptionInfo>(...)` + `return eventBusBuilder;` shape, but that's Options-pattern/fluent-builder boilerplate, not duplicated logic — the configured behavior differs completely between the two.

Commits: `c430813`, `96ca024`, `8e7b556`, `e4db372`, `bb84b6b`, `3dd2a7e`, `81d1e63`, `1058b5c`.

### EventBusRabbitMQ

`.csproj` added in `f3de7d1`. All 6 source files reviewed for correctness and SOLID/DRY/composition-over-inheritance:

- `GlobalUsings.cs`, `EventBusOptions.cs` — minimal, no changes needed.
- `RabbitMQTelemetry.cs` — no changes needed; `OpenTelemetry.Api` (its `TextMapPropagator`/`Propagators` types) resolves transitively via `Aspire.RabbitMQ.Client`, confirmed against `project.assets.json` rather than assumed.
- `RabbitMQEventBus.cs` — found and fixed a real bug, present verbatim in upstream Microsoft source: `PublishAsync`'s `(await _rabbitMQConnection?.CreateChannelAsync()) ?? throw new InvalidOperationException("RabbitMQ connection is not open")` looks like it handles a null connection gracefully, but doesn't. The null-conditional (`?.`) short-circuits the *entire* parenthesized expression to a null `Task<IChannel>` when `_rabbitMQConnection` is null; `await`-ing a null task throws `NullReferenceException` immediately, so the `?? throw` branch is unreachable dead code — a caller hitting this path during startup (before `StartAsync` finishes setting the connection) would get an opaque NRE instead of the intended message. Fixed with an explicit `if (_rabbitMQConnection is null) throw ...` before the `await`, preserving the original message/intent.
- `RabbitMqDependencyInjectionExtensions.cs` — no changes needed at first pass; the sole concrete `IEventBusBuilder` implementation, consistent with the abstraction `EventBus`'s `EventBusBuilderExtensions.cs` already builds on. Rewired below once the Decorator split landed.

**Decorator split (2026-08-14):** upstream fidelity is no longer this project's design constraint — see [architecturedesign.md Section 9](docs/architecturedesign.md#9-decorator-for-cross-cutting-concerns) for the fork-wide principle this established. `RabbitMQEventBus` mixed connection/channel/publish/consume plumbing with cross-cutting telemetry and resilience concerns in one class; split into three `IEventBus` implementations composed as `ResilientEventBusDecorator` → `TelemetryEventBusDecorator` → bare `RabbitMQEventBus`.

That investigation turned up a second real bug, more significant than the first, also present verbatim in upstream Microsoft source: `PublishAsync` ran its publish step through `_pipeline.Execute(async () => {...})`. Verified against the actual `Polly.Core 8.6.6` assembly (via a throwaway console app reflecting `ResiliencePipeline`'s real method set, not assumed) that this binds to the **synchronous** `Execute<TResult>(Func<TResult> callback)` with `TResult` inferred as `Task` — it invokes the lambda once and treats the returned `Task` object itself as the outcome, without awaiting it. Any exception thrown from inside the lambda after its first `await` (i.e. from `channel.BasicPublishAsync`, exactly where `BrokerUnreachableException`/`SocketException` would occur) happens after `Execute` has already returned, so the retry pipeline's `ShouldHandle` never observes it — the retry logic was inert for the exact failures it was configured to catch. `ResilientEventBusDecorator` fixes this using the real `ExecuteAsync<TState>(Func<TState, CancellationToken, ValueTask>, TState, CancellationToken)` overload instead, which genuinely awaits the inner call.

Also moved during the split: `SetActivityContext` off `RabbitMQEventBus` and onto `RabbitMQTelemetry` (the class both the receive path and the new publish decorator depend on for `ActivitySource`/`Propagator`), and `RabbitMQEventBus.PublishAsync`'s context-propagation injection now sources its `ActivityContext` from the ambient `Activity.Current` (set by `TelemetryEventBusDecorator`) rather than a locally-created activity — `Activity.Current` is `AsyncLocal`-backed and flows through the `await` chain automatically, which is a real improvement over the original's manual activity-threading through nested lambdas. Receive-side tracing (`OnMessageReceived`/`ProcessEvent`) is deliberately unchanged: it isn't reachable through `IEventBus.PublishAsync` at all (driven by `RabbitMQEventBus`'s own `IHostedService`/consumer-callback wiring), so there's no decorator seam for it — a documented asymmetry, not an oversight.

No RabbitMQ broker exists yet to integration-test the retry fix end-to-end (no `eShop.AppHost` yet) — the fix's correctness rests on the reflected Polly API signatures, not a live run; flagged honestly rather than claimed as verified.

**Scoped out of this pass, left as follow-ups:** extracting an `IEventSerializer` strategy for `SerializeMessage`/`DeserializeMessage` (currently hardcoded to `System.Text.Json`), and making an explicit call on `ProcessEvent`'s sequential-vs-parallel handler dispatch (upstream leaves a `// REVIEW: This could be done in parallel` comment unresolved).

Commits: `f3de7d1`, `2fbeb5b`, `9643055`, `8b66c22`, `c781a42`, `16a0dad`, `88ae99e`, `6321b20`, `1e00b64`, `f5ec63c`, `2014f55`.

## 🚧 Still to do

Migration order is **foundation first**: shared/foundation projects, then the services that depend on them, then the web frontends, then `eShop.AppHost` (references everything, so it goes last), then `tests/` and `build/`. See [project board](https://github.com/users/Terrence721/projects/5) for the live board — this table is the flat list.

| # | Project | Status |
|---|---|---|
| 1 | `EventBus` | ✅ Done — see "EventBus" above |
| 2 | `EventBusRabbitMQ` | Not started |
| 3 | `eShop.ServiceDefaults` | Not started |
| 4 | `IntegrationEventLogEF` | Not started |
| 5 | `Identity.API` | Not started — **flag when reached**: verify `Duende.IdentityServer` 7.x→8.x breaking API/DB-schema changes against actual usage |
| 6 | `Catalog.API` | Not started |
| 7 | `Basket.API` | Not started |
| 8 | `Ordering.Domain` | Not started |
| 9 | `Ordering.Infrastructure` | Not started |
| 10 | `Ordering.API` | Not started — **flag when reached**: verify `MediatR` 12.5.0 usage compiles clean (pinned below source's original 13.0.0 per the license decision above) |
| 11 | `OrderProcessor` | Not started |
| 12 | `PaymentProcessor` | Not started |
| 13 | `Webhooks.API` | Not started |
| 14 | `WebhookClient` | Not started |
| 15 | `WebApp` | Not started |
| 16 | `WebAppComponents` | Not started |
| 17 | `HybridApp` | Not started |
| 18 | `ClientApp` (.NET MAUI) | Not started — **flag when reached**: uncomment the `Build`/`Test` steps in `.github/workflows/pr-validation-maui.yml` (commented out since `src/ClientApp/ClientApp.csproj` doesn't exist yet) |
| 19 | `eShop.AppHost` | Not started — deliberately last, references every other project — **flag when reached**: uncomment the `Install Playwright Browsers`/`Run Playwright tests`/`upload-artifact` steps in `.github/workflows/playwright.yml` (commented out since `playwright.config.ts`'s `webServer` needs this project to launch the app) |
| 20 | `tests/` (5 test projects) | Not started |
| 21 | `build/` tooling | Not started |

### Open Dependabot PRs

Not yet merged — opened automatically within hours of `.github/workflows/*` landing, confirming the new `dependabot.yml` ecosystems work as intended:

| PR | Change |
|---|---|
| `dependabot/github_actions/actions/checkout-7` | `actions/checkout` v5 → v7 |
| `dependabot/github_actions/actions/setup-dotnet-6` | `actions/setup-dotnet` v5 → v6 |
| `dependabot/github_actions/actions/upload-artifact-7` | `actions/upload-artifact` v6 → v7 |
| `dependabot/github_actions/actions/setup-node-7` | `actions/setup-node` v6 → v7 |

### CI status

Originally `eShop.slnx`/`eShop.Web.slnf` referenced all 19 projects while only 1 (`EventBus`) existed on disk, which failed every build/test workflow and GitHub's own auto-injected "Automatic Dependency Submission" check. Fixed 2026-08-14 by trimming the solution files to only list projects that actually exist, adding each one incrementally as it's added (see `docs/architecturedesign.md` Section 3) — decided with the user to keep this practice going forward rather than list projects upfront again.

Current state:

- ✅ **`eShop Pull Request Validation`** — green. Its `Test` step also needed a fix: `dotnet test --solution` hard-fails with "No test projects were found" when the solution has zero test projects (structural, not a real failure, since `tests/` hasn't been added yet) — the workflow now tolerates that specific case while still failing on any real test failure.
- ✅ **`dynamic / submit-nuget`** (GitHub's Automatic Dependency Submission) — green, now that a real restorable project exists.
- ✅ **CodeQL** — green. Originally GitHub's Default setup, which extracted C# with `build-mode: none` and got flagged for "Low C# analysis quality" (55% call-target resolution, 67% known-type expressions, both under the 85% threshold — can't resolve NuGet package types or cross-project references without an actual build). Switched to Advanced setup (`.github/workflows/codeql.yml`) with `build-mode: manual` for `csharp` (runs `dotnet build eShop.Web.slnf` first); `javascript-typescript` and `actions` stay `build-mode: none`.
- ✅ **`Playwright Tests for eShop`** — green. Three real fixes plus one deferral: corepack wasn't enabled before `yarn install`, so the runner's stock Yarn 1.22.22 couldn't read `package.json`'s `"packageManager": "yarn@4.18.0"` pin; the HTTPS dev-cert step tried `--trust`, which fails on Ubuntu (no OS trust store) even though nothing needs it trusted (tests run over HTTP via `ESHOP_USE_HTTP_ENDPOINTS`); and the `Install Playwright Browsers`/`Run Playwright tests`/`upload-artifact` steps are commented out until `eShop.AppHost` exists (see row 19 above) — that project needs to exist for `playwright.config.ts`'s `webServer` to launch the app at all.
- ✅ **`eShop Pull Request Validation - .NET MAUI`** — green. `Build`/`Test` steps commented out until `ClientApp` exists (see row 18 above); the workload-install steps stay live since a broken workload feed is still a real regression worth catching.
- All four workflows also picked up explicit `permissions: contents: read` after CodeQL's `actions` language scan flagged `pr-validation.yml`, `pr-validation-maui.yml`, and `playwright.yml` for not declaring permissions (default `GITHUB_TOKEN` scope is broader than any of them need).

Every currently-red step in the two commented-out sections above is an honest, tracked gap, not a bug — they'll come back once `ClientApp` and `eShop.AppHost` respectively exist.
