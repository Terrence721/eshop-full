# Code Review Status

Record of source files reviewed during the `eshop-full` code review pass.
Result legend: `OK` = reviewed, no changes required; `FIX` = reviewed and changed.

## Shared

| File | Result | Notes |
| --- | --- | --- |
| `src/Shared/Abstractions/IDbSeeder.cs` | OK | Non-generic IDbSeeder contract; no changes needed. |
| `src/Shared/ActivityExtensions.cs` | OK | Null-safe exception tagging; no changes needed. |
| `src/Shared/LogSanitizer.cs` | OK | CR/LF stripping for log forging; no changes needed. |
| `src/Shared/MigrateDbContextExtensions.cs` | OK | Hosting/migration helper reviewed; composition-based IHostedService, activity + error handling intact. |

## EventBus

| File | Result | Notes |
| --- | --- | --- |
| `src/EventBus/Abstractions/EventBusSubscriptionInfo.cs` | OK | AOT-aware DefaultSerializerOptions; no changes needed. |
| `src/EventBus/Abstractions/IEventBus.cs` | OK | Minimal publish contract; no changes needed. |
| `src/EventBus/Abstractions/IEventBusBuilder.cs` | OK | Services accessor; no changes needed. |
| `src/EventBus/Abstractions/IIntegrationEventHandler.cs` | OK | Default interface method cast safe; no changes needed. |
| `src/EventBus/Events/IntegrationEvent.cs` | OK | Record with required-goal-friendly init fields; no changes needed. |
| `src/EventBus/Extensions/EventBusBuilderExtensions.cs` | OK | Keyed handlers, event type mapping; no changes needed. |
| `src/EventBus/Extensions/GenericTypeExtensions.cs` | OK | Generic type name helper; no changes needed. |
| `src/EventBus/GlobalUsings.cs` | OK | Single global using; no changes needed. |

## EventBusKafka

| File | Result | Notes |
| --- | --- | --- |
| `src/EventBusKafka/EventBusOptions.cs` | OK | Carried-over options; consumer group id + retry count; no changes needed. |
| `src/EventBusKafka/GlobalUsings.cs` | OK | Kafka/abstraction/usings; no changes needed. |
| `src/EventBusKafka/IEventSerializer.cs` | OK | Swappable serializer seam; no changes needed. |
| `src/EventBusKafka/KafkaDependencyInjectionExtensions.cs` | OK | Producer/consumer/telemetry/resilience wiring verified; no changes needed. |
| `src/EventBusKafka/KafkaEventBus.cs` | OK | Publish/consume/decorator-hosted lifecycle; no changes needed. |
| `src/EventBusKafka/KafkaTelemetry.cs` | OK | Semconv tags; no changes needed. |
| `src/EventBusKafka/ResilientEventBusDecorator.cs` | OK | Polly retry of `KafkaRetriableException`; no changes needed. |
| `src/EventBusKafka/SystemTextJsonEventSerializer.cs` | OK | AOT-safe serialization; no changes needed. |
| `src/EventBusKafka/TelemetryEventBusDecorator.cs` | OK | Span + exception tags; no changes needed. |

## eShop.ServiceDefaults

| File | Result | Notes |
| --- | --- | --- |
| `src/eShop.ServiceDefaults/AuthenticationExtensions.cs` | OK | JWT auth setup and Android emulator issuer handling; no changes needed. |
| `src/eShop.ServiceDefaults/ClaimsPrincipalExtensions.cs` | OK | "sub" + NameIdentifier fallback; no changes needed. |
| `src/eShop.ServiceDefaults/ConfigurationExtensions.cs` | OK | Required config value helper; no changes needed. |
| `src/eShop.ServiceDefaults/Extensions.cs` | OK | OTel/metrics/tracing/health/discovery defaults; no changes needed. |
| `src/eShop.ServiceDefaults/HttpClientExtensions.cs` | OK | Token delegating handler via IHttpContextAccessor; no changes needed. |
| `src/eShop.ServiceDefaults/OpenApi.Extensions.cs` | OK | Versioned OpenAPI + Scalar wiring; implicit OAuth flow remains a marked TODO. |
| `src/eShop.ServiceDefaults/OpenApiOptionsExtensions.cs` | OK | Document transformers and security scheme application; no changes needed. |

## IntegrationEventLogEF

| File | Result | Notes |
| --- | --- | --- |
| `src/IntegrationEventLogEF/EventState.cs` | OK | Outbox state enum; no changes needed. |
| `src/IntegrationEventLogEF/GlobalUsings.cs` | OK | EF/view assembly usings; no changes needed. |
| `src/IntegrationEventLogEF/IntegrationEventLogEntry.cs` | OK | Outbox entry + deferred JSON deserialization; no changes needed. |
| `src/IntegrationEventLogEF/IntegrationLogExtensions.cs` | OK | EF model mapping; no changes needed. |
| `src/IntegrationEventLogEF/Services/IIntegrationEventLogService.cs` | OK | Outbox service contract; no changes needed. |
| `src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs` | OK | Save/retrieve/mark flow; no changes needed. |
| `src/IntegrationEventLogEF/Services/IntegrationEventTypeResolver.cs` | OK | Entry-assembly type resolution; no changes needed. |
| `src/IntegrationEventLogEF/Utilities/ResilientTransaction.cs` | OK | Shared execution strategy + explicit transaction; no changes needed. |

## Catalog.API

| File | Result | Notes |
| --- | --- | --- |
| `src/Catalog.API/Model/CatalogItem.cs` | OK | 9 unit tests pass (RemoveStock/AddStock paths); no changes needed. |
| `src/Catalog.API/Model/CatalogBrand.cs` | OK | Trivial entity (Id + required Brand, brand-only constructor); no unit tests target it; no changes needed. |
| `src/Catalog.API/Model/CatalogType.cs` | OK | Trivial entity (Id + required Type, type-only constructor); no unit tests target it; no changes needed. |
| `src/Catalog.API/Model/PaginatedItems.cs` | OK | Generic paging envelope (PageIndex/PageSize/Count/Data); init-only reads; no changes needed. |
| `src/Catalog.API/Model/PaginationRequest.cs` | OK | Paging input record (PageSize default 10, PageIndex default 0); no validation boundaries; no changes needed. |
| `src/Catalog.API/Extensions/Extensions.cs` | FIX | Hardened `OllamaEnabled` parsing (`bool.Parse` -> `bool.TryParse`) so a non-boolean config value no longer throws at startup; startup wiring otherwise unchanged. |
| `src/Catalog.API/Infrastructure/CatalogContext.cs` | OK | DbContext for CatalogItems/Brands/Types plus vendor vector extension and integration event log table; no changes needed. |
| `src/Catalog.API/Infrastructure/CatalogContextSeed.cs` | OK | Idempotent seeder (catalog.json -> brands/types/items, optional embeddings); no changes needed. |
| `src/Catalog.API/Infrastructure/EntityConfigurations/CatalogBrandEntityTypeConfiguration.cs` | OK | Maps CatalogBrand to its table with a 100-char Brand max length; no changes needed. |
| `src/Catalog.API/Infrastructure/EntityConfigurations/CatalogTypeEntityTypeConfiguration.cs` | OK | Maps CatalogType to its table with a 100-char Type max length; no changes needed. |
| `src/Catalog.API/Extensions/HostEnvironmentExtensions.cs` | OK | Detects the "Build" environment / GetDocument.Insider host used for OpenAPI build-time generation; null-safe entry assembly check; no changes needed. |
| `src/Catalog.API/Model/CatalogServices.cs` | OK | ASP.NET AsParameters aggregator over 5 services; ctor assignment to init properties; no changes needed. |
