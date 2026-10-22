# Incident Report: eShop CI Validation Failures and Historical Run Cleanup

## Incident Details

- Incident Window: 2026-10-22 (UTC)
- Severity: Medium (CI signal quality and validation noise — no application runtime impact)
- Status: Resolved
- Source repository: `Terrence721/eshop-full`

## Overview

The `eshop-full` repository experienced widespread GitHub Actions failures on PR and push validations. Some failures were caused by a compile-time test mismatch left behind after a production handler was renamed; others were pre-existing/stale runs created before the fix, stale dependency-submission workflow runs, and superseded Pages/CodeQL runs. These produced a long trail of red run statuses that looked like repository-level breakage even though current `main` still built and tested successfully.

## User-Visible Impact

- Review PRs displayed red eShop Pull Request Validation and CodeQL statuses.
- GitHub Actions listed dozens of older failed/canceled eShop runs.
- CI noise made it difficult to distinguish real current-main regressions from historical failures.
- CodeQL and Markdownlint statuses for several earlier commits remained red.

## Primary Symptoms

- Build error in `tests/Webhooks.API.UnitTests/OrderStatusChangedToPaidIntegrationEventHandlerTests.cs:20`: `cannot convert from ILogger<OrderStatusChangedToShippedIntegrationEventHandler>` to `ILogger<OrderStatusChangedToPaidIntegrationEventHandler>`.
- Markdownlint docs table spacing error (`MD058`) around the Webhooks tables in `docs/code-review-status.md`.
- `pages build and deployment` and CodeQL "higher priority run" cancellations.
- `Automatic Dependency Submission (NuGet)` failures that occurred after the workflow completed and did not affect application builds.
- Red status on older pre-fix commits because they never contained the later test correction.

## Root Causes

- `OrderStatusChangedToPaidIntegrationEventHandler` used `ILogger<OrderStatusChangedToPaidIntegrationEventHandler>`, but its corresponding Webhooks test still wired `ILogger<OrderStatusChangedToShippedIntegrationEventHandler>`.
- Review-result tables in `docs/code-review-status.md` had insufficient blank-line spacing around Markdown tables.
- Pre-fix commits cannot become green when rerun because their code still contains the obsolete logger type.
- Automatic dependency-submission job runs fail transiently in GitHub's action setup and produce red icons unrelated to repository correctness.
- Concurrent Pages/CodeQL runs superseded earlier runs, leaving them canceled rather than successful.
- Failed historic runs accumulated as the code review progressed across many small PRs.

## Remediation Actions

- Corrected the logger generic type in `OrderStatusChangedToPaidIntegrationEventHandlerTests.cs` to `ILogger<OrderStatusChangedToPaidIntegrationEventHandler>`.
- Added proper blank-line spacing around the Webhooks status tables.
- Merged the combined fix in PR #317 and verified all current-main validation runs pass.
- Re-ran the CodeQL run for the corrected commit.
- Removed unrelated, infrastructure-only Automatic Dependency Submission failures and all superseded/canceled Pages builds.
- Removed historical failed and canceled Actions runs so the current Actions view reflects only current-main health.

## Validation Evidence

- Webhooks unit tests passed after the logger-type correction.
- `eShop Pull Request Validation` on `05af39c` passed.
- CodeQL on `05af39c` passed.
- Playwright tests on `05af39c` passed.
- Pages deployment on `05af39c` passed.
- Later review PRs (PaymentProcessor and status docs) all passed ordinary validation once they completed.
- Final Actions state had zero failed and zero canceled runs.

## Final Outcome

- Current `main` had no failing ordinary CI validations after the fix.
- The failed Webhooks test logger issue was corrected.
- Markdown table issue was corrected.
- Historical Actions noise removed; remaining runs were green or queued/in-progress from newer commits only.
- Review work (Webhooks.API, WebhookClient, PaymentProcessor) continued without introducing application regressions.

## Preventive Measures

- Update or remove tests in the same change when renaming event handlers/integration types.
- Restore exact logger type generic arguments to match the class under test, especially during code-review batches.
- Run all unit tests and Markdownlint before merging code-review documentation.
- Apply Markdownlint rules around table spacing consistently.
- Recognize Automatic Dependency Submission red statuses as separate from application/object code.
- Keep broad historical run cleanup available when many small review PRs create run-status noise.
