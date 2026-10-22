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
| `src/Catalog.API/Services/CatalogAI.cs` | OK | AI/embedding helper (IsEnabled, truncation to configured dimensions, combined text); 8 unit tests pass. |
| `src/Catalog.API/Services/ICatalogAI.cs` | OK | Interface for AI embedding operations; single interface (not split) keeps callers simple; no changes needed. |
| `src/Catalog.API/Infrastructure/EntityConfigurations/CatalogBrandEntityTypeConfiguration.cs` | OK | Maps CatalogBrand to its table with a 100-char Brand max length; no changes needed. |
| `src/Catalog.API/Infrastructure/EntityConfigurations/CatalogTypeEntityTypeConfiguration.cs` | OK | Maps CatalogType to its table with a 100-char Type max length; no changes needed. |
| `src/Catalog.API/Infrastructure/EntityConfigurations/CatalogItemEntityTypeConfiguration.cs` | OK | Maps CatalogItem to Catalog table (Name 50, vector(384) embedding, FKs to brand/type, index on Name); no changes needed. |
| `src/Catalog.API/Apis/CatalogApi.cs` | OK | Minimal API endpoint registration/handlers (list, lookup, semantic search, update, create, delete, picture); no changes needed. |
| `src/Catalog.API/CatalogOptions.cs` | OK | Catalog settings (PicBaseUrl, UseCustomizationData, configurable EmbeddingDimensions default 384); no changes needed. |
| `src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs` | OK | Publishes via event bus with outbox marking; atomic save of catalog + outbox event; no changes needed. |
| `src/Catalog.API/IntegrationEvents/ICatalogIntegrationEventService.cs` | OK | Two-method contract for saving/publishing integration events; no changes needed. |
| `src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs` | OK | Validates stock availability on awaiting validation event, publishing confirmed/rejected event; no changes needed. |
| `src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToPaidIntegrationEventHandler.cs` | OK | Decrements stock on paid event using RemoveStock; saves context; no changes needed. |
| `src/Catalog.API/IntegrationEvents/Events/ConfirmedOrderStockItem.cs` | OK | Stock confirmation result record (ProductId, HasStock); no changes needed. |
| `src/Catalog.API/IntegrationEvents/Events/OrderStatusChangedToAwaitingValidationIntegrationEvent.cs` | OK | Event carrying order id + stock items for validation; no changes needed. |
| `src/Catalog.API/IntegrationEvents/Events/OrderStatusChangedToPaidIntegrationEvent.cs` | OK | Event carrying order id + stock items for stock decrement; no changes needed. |
| `src/Catalog.API/IntegrationEvents/Events/OrderStockConfirmedIntegrationEvent.cs` | OK | Confirmation event for an order with sufficient stock; no changes needed. |
| `src/Catalog.API/IntegrationEvents/Events/OrderStockItem.cs` | OK | Stock item line record (ProductId, Units); no changes needed. |
| `src/Catalog.API/IntegrationEvents/Events/OrderStockRejectedIntegrationEvent.cs` | OK | Rejection event with per-item stock availability; no changes needed. |
| `src/Catalog.API/IntegrationEvents/Events/ProductPriceChangedIntegrationEvent.cs` | OK | Price change event (ProductId, NewPrice, OldPrice); no changes needed. |
| `src/Catalog.API/Program.cs` | OK | Startup wiring (service defaults, application services, OpenAPI, catalog API); no changes needed. |
| `src/Catalog.API/Program.Testing.cs` | OK | Public partial Program marker for WebApplicationFactory integration tests; no changes needed. |
| `src/Catalog.API/GlobalUsings.cs` | OK | Project-wide global usings (asp.net versioning, EF, Npgsql, service defaults); no changes needed. |
| `src/Catalog.API/Infrastructure/Exceptions/CatalogDomainException.cs` | OK | Standard exception type (parameterless/message/inner) with doc summary; no changes needed. |
| `src/Catalog.API/Extensions/HostEnvironmentExtensions.cs` | OK | Detects the "Build" environment / GetDocument.Insider host used for OpenAPI build-time generation; null-safe entry assembly check; no changes needed. |
| `src/Catalog.API/Model/CatalogServices.cs` | OK | ASP.NET AsParameters aggregator over 5 services; ctor assignment to init properties; no changes needed. |

## Basket.API

| File | Result | Notes |
| --- | --- | --- |
| `src/Basket.API/Model/BasketItem.cs` | OK | Validatable basket line (Quantity < 1 rejected); self-consistency constraints minimal; no changes needed. |
| `src/Basket.API/Model/CustomerBasket.cs` | OK | Customer basket aggregate (BuyerId, Items, parameterless/optional constructor); no changes needed. |
| `src/Basket.API/Repositories/IBasketRepository.cs` | OK | Focused CRUD contract for customer basket (get, update, delete); ISP-compliant; no changes needed. |
| `src/Basket.API/Repositories/RedisBasketRepository.cs` | FIX | Redis-backed basket repository (get/update/delete, JSON serialization); marked `BasketKeyPrefix` `readonly` (it was mutable despite being a `static` prefix). |
| `src/Basket.API/Extensions/ServerCallContextIdentityExtensions.cs` | OK | Two single-responsibility gRPC extension methods (sub/name claim extraction); focused, ISP-compliant; class-level XML doc added; no functional changes. |
| `src/Basket.API/Extensions/Extensions.cs` | OK | Application composition root (auth, Redis, singleton basket repository, event-bus subscriptions, JSON context); SRP respected at composition layer; class-level XML doc added; no functional changes. |
| `src/Basket.API/Grpc/BasketService.cs` | OK | gRPC controller (get/update/delete), caller-scoped identity checks, mapping helpers; repository interface only (ISP); class-level XML doc added; no functional changes. |
| `src/Basket.API/IntegrationEvents/EventHandling/OrderStartedIntegrationEventHandler.cs` | OK | Posts-order basket deletion via IBasketRepository; focused, SRP/ISP compliant; class-level XML doc added; no functional changes. |
| `src/Basket.API/IntegrationEvents/Events/OrderStartedIntegrationEvent.cs` | OK | Immutable record (UserId) inheriting IntegrationEvent; XML doc added; no functional changes. |
| `src/Basket.API/Program.cs` | OK | Composition root (basic defaults, app services, gRPC, gRPC-Web, MapGrpcService); SRP compliant; entrypoint comment added. |
| `src/Basket.API/GlobalUsings.cs` | OK | Consolidated global `using` aliases for cross-cutting namespaces; no unused/undocumented imports; header comment added. |

## Ordering.API

| File | Result | Notes |
| --- | --- | --- |
| `src/Ordering.API/Apis/OrdersApi.cs` | FIX | Minimal-API endpoints (list/get/create/cancel/ship/draft); guarded card-number masking so short/empty values no longer throw `ArgumentOutOfRangeException`; class-level XML doc added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Apis/OrderServices.cs` | OK | ASP.NET Core [AsParameters] aggregate of mediator, queries, identity and logger; cohesive, single responsibility; class-level XML doc added. |
| `src/Ordering.API/Extensions/BasketItemExtensions.cs` | OK | BasketItem→OrderItemDTO mapping (including single-item and collection extension); null-forgiving documented; class-level XML doc added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Extensions/Extensions.cs` | OK | Composition root (auth, EF Core/Npgsql, migration, integration services, identity, mediator, validators, repositories, event bus); class-level XML doc added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Extensions/LinqSelectExtensions.cs` | OK | Per-element try/select with captured exceptions and pluggable handlers; class-level XML doc added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Extensions/OrderingApiTrace.cs` | OK | Source-generated partial logger for ordering trace events (status/payment/buyer changes); internal, focused; class-level XML doc added. |
| `src/Ordering.API/GlobalUsings.cs` | OK | Global using consolidation for Ordering.API cross-cutting namespaces; header comment added; consistent with layering. |
| `src/Ordering.API/Program.cs` | OK | Composition root (defaults, app services, problem details, versioning, OpenAPI, MapOrdersApiV1, authorization); entry-point comment added. |
| `src/Ordering.API/Program.Testing.cs` | OK | Public partial `Program` for WebApplicationFactory integration tests; XML doc added; no functional changes. |
| `src/Ordering.API/Application/Behaviors/LoggingBehavior.cs` | OK | MediatR pipeline behavior logging request/response; single responsibility; XML doc added; no functional changes. |
| `src/Ordering.API/Application/Behaviors/TransactionBehavior.cs` | FIX | MediatR transaction/outbox pipeline behavior; made null validation use `ArgumentNullException` instead of `ArgumentException` (corrected mismatched parameter-name check); XML doc added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Application/Behaviors/ValidatorBehavior.cs` | FIX | MediatR validation pipeline behavior; added constructor null checks; XML doc added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs` | OK | Draft handler + OrderDraftDTO/OrderItemDTO records; XML doc added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Application/Commands/IdentifiedCommand.cs` | OK | Identified-command (request/id) wrapper used for idempotency; XML doc added; no functional changes. |
| `src/Ordering.API/Application/Commands/IdentifiedCommandHandler.cs` | FIX | Abstract idempotent base handler (existence check, mediation, logging, duplicate handling); added mediator/requestManager null guards; XML already present; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Application/Commands/SetAwaitingValidationOrderStatusCommand.cs` | OK | Single-purpose status-transition command record; XML doc added; no functional changes. |
| `src/Ordering.API/Application/Commands/SetPaidOrderStatusCommand.cs` | OK | Single-purpose paid-status transition command record (OrderNumber only); XML doc present; primary constructor N/A; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Commands/SetStockConfirmedOrderStatusCommand.cs` | OK | Single-purpose stock-confirmed status transition command record (OrderNumber only); XML doc present; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Commands/SetStockConfirmedOrderStatusCommandHandler.cs` | OK | Stock-confirmed handler (simulated 10s confirmation) + idempotent wrapper; class-level XML doc present; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Commands/SetStockRejectedOrderStatusCommand.cs` | OK | Stock-rejected status transition command (OrderNumber + rejected OrderStockItems); class-level XML doc present; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Commands/SetStockRejectedOrderStatusCommandHandler.cs` | OK | Stock-rejected handler + idempotent wrapper: class-level XML doc present. SOLID: one coordinated status transition, depends only on IOrderRepository (DIP), LSP not applicable. DRY: shared simulated-delay pattern repeated but intentionally transition-specific. ISP: IOrderRepository is focused. No constructor null guard, consistent with other simple status handlers. No functional changes. |
| `src/Ordering.API/Application/Commands/SetAwaitingValidationOrderStatusCommandHandler.cs` | OK | Set-awaiting-validation handler + idempotent wrapper; XML doc added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Application/Commands/SetPaidOrderStatusCommandHandler.cs` | OK | Paid-status handler (simulated 10s validation) + idempotent wrapper; class-level XML docs added; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Commands/CancelOrderCommand.cs` | OK | Single-purpose cancel-order command record; XML doc added; no functional changes. |
| `src/Ordering.API/Application/Commands/CancelOrderCommandHandler.cs` | OK | Cancel handler + idempotent identified wrapper; XML docs added; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Commands/CreateOrderCommand.cs` | OK | Large, DDD-styled DataContract command (private setters, [DataContract] parameterless constructor for deserialization); class-level XML doc added; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs` | OK | Create-order handler (+ idempotent wrapper): outbox event, address/order construction, repository add/save; XML docs added; SOLID/DRY/ISP checked. |
| `src/Ordering.API/Application/Commands/CreateOrderDraftCommand.cs` | OK | Draft command record (buyer + basket items) returning OrderDraftDTO; XML doc added; no functional changes. |
| `src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentFailedIntegrationEventHandler.cs` | OK | Mediator-based payment-failed event handler emitting `CancelOrderCommand`; focused, SRP/ISP compliant; class-level XML doc added; no functional changes. |
| `src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentSucceededIntegrationEventHandler.cs` | OK | Mediator-based payment-succeeded event handler emitting `SetPaidOrderStatusCommand`; focused, SRP/ISP compliant; class-level XML doc added; no functional changes. |
| `src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockRejectedIntegrationEventHandler.cs` | OK | Mediator-based stock-rejected event handler; filters out-of-stock items and issues `SetStockRejectedOrderStatusCommand`; focused, SRP/ISP compliant; class-level XML doc added; no functional changes. |
| `src/Ordering.API/Application/IntegrationEvents/IOrderingIntegrationEventService.cs` | OK | Two-method contract for publishing and enqueuing integration events; focused, ISP-compliant; interface-level XML doc added; no functional changes. |
| `src/Ordering.API/Application/IntegrationEvents/OrderingIntegrationEventService.cs` | OK | Outbox service (save event with transaction, publish pending events); primary constructor with null validation; class-level XML doc added; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Models/BasketItem.cs` | OK | Bogus DTO handed over from Basket creation; init-only null-aware props; class-level XML doc added; no functional changes (primary-constructor N/A - no .ctor). |
| `src/Ordering.API/Application/Queries/IOrderQueries.cs` | OK | Read-model contract (three methods); focused, ISP-compliant; interface-level XML doc added; no functional changes. |
| `src/Ordering.API/Application/Queries/OrderQueries.cs` | OK | EF Core read-model implementation (order details, user orders, card types); primary-constructor cohesive; class-level XML doc added; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Application/Queries/OrderViewModel.cs` | OK | Query result DTO records (Order, Orderitem, OrderSummary, CardType); body records / init refs; class-level XML doc added; SOLID/DRY/ISP checked; no functional changes. |
| `src/Ordering.API/Infrastructure/OrderingContextSeed.cs` | OK | Seeds predefined card types when missing; walks DB context (seed adapter); class-level XML doc added; no functional changes; primary constructor N/A. |
| `src/Ordering.API/Infrastructure/Services/IIdentityService.cs` | OK | Focused two-method contract for resolving the current user's "sub" identity and display name from the ambient HttpContext; ISP-compliant, DIP-friendly seam; interface-level and member XML docs added; SOLID/DRY checked; no functional changes. |
| `src/Ordering.API/Infrastructure/Services/IdentityService.cs` | OK | HttpContext-backed IIdentityService implementation (primary constructor); reads "sub" claim and Identity.Name; null-forgiving documented; class-level XML doc added; SOLID/DRY/ISP checked; no functional changes. |