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

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(eventBus, connectionName: "eventbus")
    .WithReference(embeddingModel, connectionName: "embedding")
    .WithEnvironment("OllamaEnabled", "true")
    .WaitFor(postgres)
    .WaitFor(eventBus);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(eventBus, connectionName: "eventbus")
    .WaitFor(redis)
    .WaitFor(eventBus);

var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(orderingDb)
    .WithReference(eventBus, connectionName: "eventbus")
    .WaitFor(postgres)
    .WaitFor(eventBus);

var webhooksApi = builder.AddProject<Projects.Webhooks_API>("webhooks-api")
    .WithReference(webhooksDb)
    .WithReference(eventBus, connectionName: "eventbus")
    .WaitFor(postgres)
    .WaitFor(eventBus);

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

// WithReference here makes each API's real dashboard-assigned endpoint available to WebBFF's
// process via Aspire's service-discovery env vars -- WebBFF's own BasketUrl/YARP cluster
// destinations still read from hardcoded appsettings.json values until that's updated separately
// to actually consume them.
var webBff = builder.AddProject<Projects.WebBFF>("webbff")
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderingApi)
    .WithReference(identityApi)
    .WaitFor(identityApi);

var webhookClient = builder.AddProject<Projects.WebhookClient>("webhook-client")
    .WithReference(webhooksApi)
    .WaitFor(webhooksApi);

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
