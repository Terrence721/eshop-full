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

builder.Build().Run();
