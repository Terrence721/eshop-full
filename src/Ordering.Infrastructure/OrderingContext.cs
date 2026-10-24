using eShop.IntegrationEventLogEF;

namespace eShop.Ordering.Infrastructure
{
    /// <remarks>
    /// Add migrations using the following command inside the 'Ordering.Infrastructure' project directory:
    ///
    /// dotnet ef migrations add --startup-project Ordering.API --context OrderingContext [migration-name]
    /// </remarks>
    public class OrderingContext : DbContext, IUnitOfWork
    {
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<PaymentMethod> Payments { get; set; } = null!;
        public DbSet<Buyer> Buyers { get; set; } = null!;
        public DbSet<CardType> CardTypes { get; set; } = null!;

        private readonly IMediator _mediator = null!;
        private IDbContextTransaction? _currentTransaction;

        public OrderingContext(DbContextOptions<OrderingContext> options) : base(options) { }

        public IDbContextTransaction? GetCurrentTransaction() => _currentTransaction;

        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(_currentTransaction))]
        public bool HasActiveTransaction => _currentTransaction != null;

        public OrderingContext(DbContextOptions<OrderingContext> options, IMediator mediator) : base(options)
        {
            ArgumentNullException.ThrowIfNull(mediator);
            _mediator = mediator;


            System.Diagnostics.Debug.WriteLine("OrderingContext::ctor ->" + this.GetHashCode());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("ordering");
            modelBuilder.ApplyConfiguration(new ClientRequestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentMethodEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new OrderItemEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CardTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BuyerEntityTypeConfiguration());

            // Centralizes what upstream repeats as .Ignore(x => x.DomainEvents) in each
            // IEntityTypeConfiguration<T> for every Entity-derived aggregate (Order,
            // OrderItem, Buyer, PaymentMethod) -- every Entity subtype needs this, so
            // configuring it once here avoids 4 duplicated lines across those files.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).Ignore(nameof(Entity.DomainEvents));
                }
            }

            modelBuilder.UseIntegrationEventLogs();
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch Domain Events collection.
            // Choices:
            // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including
            // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
            // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions.
            // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers.
            await _mediator.DispatchDomainEventsAsync(this);

            // After executing this line all the changes (from the Command Handler and Domain Event Handlers)
            // performed through the DbContext will be committed
            _ = await base.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<IDbContextTransaction?> BeginTransactionAsync()
        {
            if (_currentTransaction != null) return null;

            _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

            try
            {
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (HasActiveTransaction)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (HasActiveTransaction)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }
}
