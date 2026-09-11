global using Asp.Versioning;
global using Asp.Versioning.Conventions;
global using eShop.Catalog.API;
global using eShop.Catalog.API.Infrastructure;
global using eShop.Catalog.API.Infrastructure.EntityConfigurations;
global using eShop.Catalog.API.Infrastructure.Exceptions;
global using eShop.Catalog.API.IntegrationEvents.Events;
global using eShop.Catalog.API.IntegrationEvents.EventHandling;
global using eShop.Catalog.API.Model;
global using eShop.EventBus.Abstractions;
global using eShop.EventBus.Events;
global using eShop.IntegrationEventLogEF;
global using eShop.IntegrationEventLogEF.Services;
global using eShop.IntegrationEventLogEF.Utilities;
global using eShop.ServiceDefaults;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Options;
global using Npgsql;

// The following upstream global using references this project's own
// eShop.Catalog.API.IntegrationEvents namespace, which doesn't have any
// files yet -- C# genuinely can't resolve a `global using` for a namespace
// with zero declarations anywhere in the compilation (CS0234), not just
// "empty of members". Add it back once ICatalogIntegrationEventService.cs/
// CatalogIntegrationEventService.cs land there:
// eShop.Catalog.API.IntegrationEvents.
