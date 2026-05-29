namespace vm2.Repository.EntityFramework;

/// <summary>
/// Provides extension methods to <see cref="IRepository"/> that are typical for <see cref="DbContext"/>.
/// </summary>
public static class EfRepositoryExtensions
{
    const string notSupportedMessage = $"The repository is not an instance of {nameof(DbContext)}.";

    /// <summary>
    /// Retrieves the <see cref="DbContext"/> instance from the specified repository.
    /// </summary>
    /// <param name="repository">The repository from which to retrieve the <see cref="DbContext"/>.</param>
    /// <returns>The <see cref="DbContext"/> instance associated with the repository.</returns>
    public static DbContext DbContext(this IRepository repository)
        => repository as DbContext
                ?? throw new NotSupportedException(notSupportedMessage);

    /// <summary>
    /// Retrieves the <see cref="ChangeTracker"/> associated with the specified <see cref="IRepository"/>.
    /// </summary>
    /// <param name="repository">The <see cref="IRepository"/> instance from which to obtain the change tracker.</param>
    /// <returns>The <see cref="ChangeTracker"/> that tracks changes to entities within the repository.</returns>
    public static ChangeTracker ChangeTracker(this IRepository repository)
        => repository.DbContext().ChangeTracker;

    /// <summary>
    /// Retrieves the <see cref="DatabaseFacade"/> associated with the specified <see cref="IRepository"/>.
    /// </summary>
    /// <param name="repository">The <see cref="IRepository"/> instance from which to obtain the database facade.</param>
    /// <returns>The <see cref="DatabaseFacade"/> of the specified repository.</returns>
    public static DatabaseFacade Database(this IRepository repository)
        => repository.DbContext().Database;

    /// <summary>
    /// Gets the <see cref="EntityEntry"/> envelope for the specified entity within the repository's context/change-tracker.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="repository">The repository containing the entity.</param>
    /// <param name="entity">The entity whose entry is requested.</param>
    /// <returns>The <see cref="EntityEntry"/> that wraps the entity in the context's change-tracker.</returns>
    public static EntityEntry Entry<TEntity>(this IRepository repository, TEntity entity) where TEntity : class
        => repository.DbContext().Entry(entity);

    /// <summary>
    /// Determines the current state of the specified entity within the repository's context.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="repository">The repository containing the entity.</param>
    /// <param name="entity">The entity whose state is to be determined. Cannot be null.</param>
    /// <returns>The <see cref="StateOf"/> representing the current state of the entity in the context.</returns>
    public static EntityState StateOf<TEntity>(this IRepository repository, TEntity entity) where TEntity : class
        => repository.Entry(entity).State;

    /// <summary>
    /// Determines if lazy loading is enabled for the specified entity type within the repository's context.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="repository">The repository containing the entity.</param>
    /// <returns>
    /// <see langword="true"/> if lazy loading is enabled for the entity type, <see langword="false"/> otherwise.
    /// </returns>
    public static bool IsLazyLoadingEnabled<TEntity>(this IRepository repository) where TEntity : class
    {
        if (!repository.ChangeTracker().LazyLoadingEnabled)
            return false;

        var entityType = repository.DbContext().Model.FindEntityType(typeof(TEntity));

        return entityType?
                .GetNavigations()?
                .Any(n => !n.IsEagerLoaded &&
                          n.PropertyInfo?.GetMethod?.IsVirtual is true) == true
                ;
    }
}
