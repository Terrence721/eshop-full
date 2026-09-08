global using Asp.Versioning;
global using Asp.Versioning.Conventions;
global using eShop.Catalog.API;
global using eShop.Catalog.API.Infrastructure;
global using eShop.Catalog.API.Infrastructure.EntityConfigurations;
global using eShop.Catalog.API.Infrastructure.Exceptions;
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

// The following upstream global usings reference this project's own
// namespaces (Infrastructure/, IntegrationEvents/, Model/), which don't
// have any files yet -- C# genuinely can't resolve a `global using` for a
// namespace with zero declarations anywhere in the compilation (CS0234),
// not just "empty of members". Add each one back as its real folder lands:
// eShop.Catalog.API.IntegrationEvents,
// eShop.Catalog.API.IntegrationEvents.EventHandling,
// eShop.Catalog.API.IntegrationEvents.Events.
