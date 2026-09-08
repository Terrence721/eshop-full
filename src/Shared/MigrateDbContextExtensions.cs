using System.Diagnostics;

namespace Microsoft.AspNetCore.Hosting;

internal static class MigrateDbContextExtensions
{
    private static readonly string ActivitySourceName = "DbMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
        => services.AddMigration<TContext>((_, _) => Task.CompletedTask);

    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services, Func<TContext, IServiceProvider, Task> seeder)
        where TContext : DbContext
    {
        // Enable migration tracing
        services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

        return services.AddHostedService(sp => new MigrationHostedService<TContext>(sp, seeder));
    }

    public static IServiceCollection AddMigration<TContext, TDbSeeder>(this IServiceCollection services)
        where TContext : DbContext
        where TDbSeeder : class, IDbSeeder<TContext>
    {
        services.AddScoped<IDbSeeder<TContext>, TDbSeeder>();
        return services.AddMigration<TContext>((context, sp) => sp.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context));
    }

    private static async Task MigrateDbContextAsync<TContext>(this IServiceProvider services, Func<TContext, IServiceProvider, Task> seeder) where TContext : DbContext
    {
        using var scope = services.CreateScope();
        var scopeServices = scope.ServiceProvider;
        var logger = scopeServices.GetRequiredService<ILogger<TContext>>();
        var context = scopeServices.GetRequiredService<TContext>();

        await RunWithActivityAsync(
            $"Migration operation {typeof(TContext).Name}",
            async () =>
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                var strategy = context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(() => InvokeSeeder(seeder, context, scopeServices));
            },
            ex => logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name));
    }

    private static Task InvokeSeeder<TContext>(Func<TContext, IServiceProvider, Task> seeder, TContext context, IServiceProvider services)
        where TContext : DbContext
        => RunWithActivityAsync(
            $"Migrating {typeof(TContext).Name}",
            async () =>
            {
                await context.Database.MigrateAsync();
                await seeder(context, services);
            });

    // Shared by MigrateDbContextAsync and InvokeSeeder: both need an activity
    // scoped to the work, with the exception tagged onto it and rethrown on
    // failure. onError lets MigrateDbContextAsync also log, without forcing
    // InvokeSeeder to.
    private static async Task RunWithActivityAsync(string activityName, Func<Task> action, Action<Exception>? onError = null)
    {
        using var activity = ActivitySource.StartActivity(activityName);

        try
        {
            await action();
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);

            activity?.SetExceptionTags(ex);

            throw;
        }
    }

    // Composition-over-inheritance fix: this used to inherit BackgroundService
    // purely for its IHostedService plumbing, then discarded the one method
    // (ExecuteAsync) BackgroundService exists to provide, overriding it as a
    // bare no-op. StartAsync already fully overrode BackgroundService's own
    // StartAsync without calling base.StartAsync(), so BackgroundService's
    // internal _executeTask was never populated -- meaning its inherited
    // StopAsync always hit its own early-exit-on-null-_executeTask path
    // anyway. A plain IHostedService with an honest no-op StopAsync is
    // therefore behavior-identical, not just a smaller type.
    private class MigrationHostedService<TContext>(IServiceProvider serviceProvider, Func<TContext, IServiceProvider, Task> seeder)
        : IHostedService where TContext : DbContext
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return serviceProvider.MigrateDbContextAsync(seeder);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
