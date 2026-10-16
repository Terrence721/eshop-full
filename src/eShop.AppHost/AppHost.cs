var builder = DistributedApplication.CreateBuilder(args);

// One Postgres server, 4 logical databases - matches upstream's own real resource graph
// (confirmed via earlier research in todo.md). ankane/pgvector, not the default postgres
// image, since Catalog.API's CatalogContext requires the vector extension.
var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector", "latest")
    .WithLifetime(ContainerLifetime.Persistent);
var catalogDb = postgres.AddDatabase("catalogdb");
var identityDb = postgres.AddDatabase("identitydb");
var orderingDb = postgres.AddDatabase("orderingdb");
var webhooksDb = postgres.AddDatabase("webhooksdb");

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Named "eventbus" to match every service's existing GetConnectionString("eventbus") /
// AddKafkaEventBus("eventbus") call -- zero appsettings.json changes needed anywhere.
var eventBus = builder.AddKafka("eventbus")
    .WithLifetime(ContainerLifetime.Persistent);

// Local embeddings for Catalog.API's semantic search. "embedding" is the connection name
// Catalog.API's own AddOllamaApiClient("embedding") call already expects. all-minilm produces
// 384-dim embeddings, matching the Catalog table's own "vector(384)" column exactly.
var ollama = builder.AddOllama("ollama")
    .WithLifetime(ContainerLifetime.Persistent);
var embeddingModel = ollama.AddModel("embedding", "all-minilm");

var identityApi = builder.AddProject<Projects.Identity_API>("identity-api")
    .WithReference(identityDb)
    .WaitFor(postgres);

// WaitFor(embeddingModel) is required, not optional -- live-verified: without it, seeding ran
// before Ollama's image finished pulling, and CatalogContextSeed's embedding-generation call
// threw an unhandled exception that crashed the whole process (startup-time seeding has no
// retry/resilience around that call).
var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(eventBus, connectionName: "eventbus")
    .WithReference(embeddingModel, connectionName: "embedding")
    .WithEnvironment("OllamaEnabled", "true")
    .WaitFor(postgres)
    .WaitFor(eventBus)
    .WaitFor(embeddingModel);

// Identity__Url feeds AddDefaultAuthentication's JWT bearer Authority (eShop.ServiceDefaults) --
// live-verified as a real pre-existing gap: appsettings.json's own "Identity" section never had a
// "Url" value (only "Audience"), since no real Identity.API instance existed to point at before
// now.
var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(eventBus, connectionName: "eventbus")
    .WithReference(identityApi)
    .WithEnvironment("Identity__Url", identityApi.GetEndpoint("http"))
    .WaitFor(redis)
    .WaitFor(eventBus)
    .WaitFor(identityApi);

var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(orderingDb)
    .WithReference(eventBus, connectionName: "eventbus")
    .WithReference(identityApi)
    .WithEnvironment("Identity__Url", identityApi.GetEndpoint("http"))
    .WaitFor(postgres)
    .WaitFor(eventBus)
    .WaitFor(identityApi);

// Overrides appsettings.json's own stale hardcoded "Identity:Url" (a fixed dev port from before
// this project existed) with the real dashboard-assigned one.
var webhooksApi = builder.AddProject<Projects.Webhooks_API>("webhooks-api")
    .WithReference(webhooksDb)
    .WithReference(eventBus, connectionName: "eventbus")
    .WithReference(identityApi)
    .WithEnvironment("Identity__Url", identityApi.GetEndpoint("http"))
    .WaitFor(postgres)
    .WaitFor(eventBus)
    .WaitFor(identityApi);

// Upstream's own comment, preserved: "wait for the orderingApi to be ready because that
// contains the EF migrations" -- OrderProcessor reads the same orderingdb directly via raw
// ADO.NET/Npgsql, with no migrations of its own.
var orderProcessor = builder.AddProject<Projects.OrderProcessor>("order-processor")
    .WithReference(orderingDb)
    .WithReference(eventBus, connectionName: "eventbus")
    .WaitFor(orderingApi);

var paymentProcessor = builder.AddProject<Projects.PaymentProcessor>("payment-processor")
    .WithReference(eventBus, connectionName: "eventbus")
    .WaitFor(eventBus);

// BasketUrl/YARP cluster destinations resolve "basket-api"/"catalog-api"/"ordering-api" via
// WithReference + Microsoft.Extensions.ServiceDiscovery.Yarp's destination resolver (already
// wired in WebBFF itself). IdentityUrl is WebBFF's own flat config key (not a connection string
// WithReference would populate), read directly by Duende.BFF's OIDC Authority -- needs the
// explicit WithEnvironment, live-verified as a real startup crash (GetRequiredValue("IdentityUrl")
// throws) without it.
var webBff = builder.AddProject<Projects.WebBFF>("webbff")
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderingApi)
    .WithReference(identityApi)
    .WithEnvironment("IdentityUrl", identityApi.GetEndpoint("http"))
    .WaitFor(identityApi);

var webhookClient = builder.AddProject<Projects.WebhookClient>("webhook-client")
    .WithReference(webhooksApi)
    .WithReference(identityApi)
    .WithEnvironment("IdentityUrl", identityApi.GetEndpoint("http"))
    .WaitFor(webhooksApi)
    .WaitFor(identityApi);

// CallBackUrl (SignedOutRedirectUri) needs WebhookClient's own address -- a self-reference,
// resolved lazily at runtime like any other EndpointReference, so it has to be set after the
// resource itself is declared rather than inline in the chain above.
webhookClient.WithEnvironment("CallBackUrl", webhookClient.GetEndpoint("http"));

// All three are Yarn workspaces (this repo's only JavaScript package manager), not npm --
// WithYarn() overrides AddViteApp's npm default. Each still reads its own hardcoded dev-proxy
// target constant in vite.config.ts until that's updated separately to consume these
// service-discovery references instead.
var identityWebApp = builder.AddViteApp("identity-webapp", "../Identity.WebApp")
    .WithYarn()
    .WithReference(identityApi)
    .WaitFor(identityApi);

var webApp = builder.AddViteApp("webapp", "../WebApp")
    .WithYarn()
    .WithReference(webBff)
    .WaitFor(webBff);

var webhookClientWebApp = builder.AddViteApp("webhookclient-webapp", "../WebhookClient.WebApp")
    .WithYarn()
    .WithReference(webhookClient)
    .WaitFor(webhookClient);

builder.Build().Run();
