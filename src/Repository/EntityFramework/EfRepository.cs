namespace vm2.Repository.EntityFramework;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

/// <summary>
/// The class <see cref="EfRepository"/> is <see cref="DbContext"/> that implements explicitly <see cref="IRepository"/>.
/// </summary>
/// <remarks>
/// Note that the <see cref="IRepository"/> is implemented explicitly by the <see cref="EfRepository"/> class. Therefore, in
/// order to access the interface methods, your <see cref="DbContext"/> that inherits from <see cref="EfRepository"/>, can be
/// passed in as <see cref="IRepository"/> to the clients. The best approach would be to use dependency injection that resolves
/// <see cref="IRepository"/> to the concrete <see cref="EfRepository"/> descendant.<para/>
/// <see cref="IRepository"/> does not claim, nor tries to cover the full functionality of <see cref="DbContext"/>. To access
/// the full functionality of <see cref="DbContext"/>, you can use the extension method <see cref="EfRepositoryExtensions.DbContext"/>.
/// </remarks>
public partial class EfRepository : DbContext, IRepository
{
    IRepository ThisRepo => this;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfRepository"/> class using the specified options.
    /// </summary>
    /// <param name="options">The <see cref="DbContextOptions"/> used to configure the context.</param>
    public EfRepository(DbContextOptions options)
        : base(options)
    { }

    /// <summary>
    /// Represents an abstract collection of domain objects (entities) of type <typeparamref name="T"/>. Since the entity set is
    /// represented as <see cref="IQueryable{T}"/>, the <c>IRepository</c>'s clients can declaratively construct LINQ queries.
    /// Entities can be added to the set (<see cref="IRepository.AddAsync"/>), and removed from the set (<see cref="IRepository.Remove"/>).
    /// </summary>
    /// <typeparam name="T">The type of the entity in the set.</typeparam>
    /// <returns><see cref="IQueryable{T}"/>.</returns>
    /// <remarks>
    /// Note that the returned <see cref="IQueryable"/> is the Entity Framework's <see cref="DbSet{TEntity}"/>. Some methods may
    /// be executed first on the internal in-memory entity change-tracker and then, if needed, on the underlying physical store,
    /// e.g. <see cref="DbSet{TEntity}.FindAsync(object[])"/> and <see cref="DbSet{TEntity}.AddAsync"/>.
    /// </remarks>
    IQueryable<T> IRepository.Set<T>() => base.Set<T>();

    /// <summary>
    /// Finds an entity by the value(s) of its unique, possibly composite, primary store key. The method searches first in the<br/>
    /// change-tracker and if not found, searches for it in the underlying physical store. Since the search is done by<br/>
    /// the primary key(s), the operation is usually faster and cheaper than other queries.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="keyValues">
    /// The primary key(s). Note that the order of the key values in a composite key is important.<br/>
    /// Note that the order of the key values in a composite key is important.
    /// </param>
    /// <param name="ct">Can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns><see cref="Task{T}"/> which contains the found instance or <see langword="null"/> if not found.</returns>
    /// <remarks>Note that the method is asynchronous.</remarks>
    public ValueTask<T?> FindAsync<T>(
        IEnumerable<object?>? keyValues,
        CancellationToken ct = default) where T : class
        => ThisRepo.Set<T>().FindAsync(keyValues, ct);

    /// <summary>
    /// Adds the <paramref name="entity"/> to the change-tracker (in memory) in <see cref="EntityState.Added"/> state.
    /// The method usually finishes synchronously. However, if a whole graph of entities is added and some of the primary keys
    /// are generated at the store, EF may need to add some of the entities to the store now, to obtain the primary keys and
    /// fix-up the values of some of the foreign keys of the dependent entities.  The entity is added to the data store on
    /// <see cref="IRepository.CommitAsync"/> (<see cref="DbContext.SaveChangesAsync(CancellationToken)"/>).
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity instance to add.</param>
    /// <param name="ct">Can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns><see cref="ValueTask{T}"/> which contains the added instance.</returns>
    /// <remarks>Note that the method is asynchronous.</remarks>
    async ValueTask<T> IRepository.AddAsync<T>(
        T entity,
        CancellationToken ct) where T : class
        => await ThisRepo.Set<T>().AddAsync(entity, ct).ConfigureAwait(false);

    /// <summary>
    /// Attaches the specified entity to the internal object change-tracker in a <see cref="EntityState.Modified"/> state.
    /// At the next <see cref="IRepository.CommitAsync"/> EF will update the entities in <see cref="EntityState.Modified"/> state in
    /// the DB.
    /// <para>
    /// Use with caution! The use of this method implies that the code "knows" what is the current state of the entity better
    /// than the DB. In effect the DB stops being the ultimate source of truth. This is a strategy sometimes known as
    /// "client-wins" vs. "store-wins".
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The modified entity instance.</param>
    /// <returns>The added entity.</returns>
    T IRepository.Update<T>(T entity)
        => ThisRepo.Set<T>().Update(entity);

    /// <summary>
    /// If the <paramref name="entity"/> is not in the tracker yet, the method will add it in <see cref="EntityState.Deleted"/> state.<br/>
    /// If it is already there, the tracker will make sure that the entity's state is <see cref="EntityState.Deleted"/>.<br/>
    /// At the next <see cref="IRepository.CommitAsync"/> the entity will be deleted from the DB as well.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to be deleted.</param>
    /// <returns>The entity to be removed.</returns>
    /// <remarks>
    /// Note that the entity doesn't need to be loaded in the change-tracker with a previous query. If the entity is not in the<br/>
    /// tracker, the method will add it as is in "Deleted" state. The only important thing for the entity is that it contains<br/>
    /// valid keys in the proper order. This warrants the following pattern:
    /// <code><![CDATA[
    ///     Entity entity = new() { Id = entityId };
    ///     _repository.Remove(entity);                             // NO trip to the DB
    ///     await _repository.CommitAsync();                        // TRIP to the DB
    /// ]]></code>instead of:<code><![CDATA[
    ///     Entity entity = await _repository.FindAsync(entityId);  // FIRST trip to the DB!
    ///     _repository.Remove(entity);
    ///     await _repository.CommitAsync();                        // SECOND trip to the DB
    /// ]]></code>
    /// </remarks>
    T IRepository.Remove<T>(T entity)
        => ThisRepo.Set<T>().Remove(entity);

    /// <summary>
    /// Attaches the specified entity to the change-tracker in a <see cref="EntityState.Unchanged"/> state. If any changes are done to the entity, its<br/>
    /// state will transition to <see cref="EntityState.Modified"/>. At the next <see cref="IRepository.CommitAsync"/> EF will update the entities in<br/>
    /// <see cref="EntityState.Modified"/> state in the DB and will ignore the <see cref="EntityState.Unchanged"/> entities.<br/>
    /// <para>
    /// Use with caution! The use of this method implies that the code "knows" what is the current state of the entity better<br/>
    /// than the repository or the DB. In effect the DB stops being the ultimate source of truth. This is a strategy sometimes<br/>
    /// known as "client-wins" (vs. "store-wins").
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to be attached.</param>
    /// <returns>The entity.</returns>
    T IRepository.Attach<T>(T entity)
        => ThisRepo.Set<T>().Attach(entity);

    /// <summary>
    /// Commits the added, modified and deleted entities in the change-tracker to the physical store invoking the respective <br/>
    /// back-end actions. For some DB-s  the action is a single transaction, for others it might be a sequence of transactions.
    /// </summary>
    /// <param name="ct">Can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns><see cref="Task"/></returns>
    /// <remarks>Note that the method is asynchronous.</remarks>
    public async Task<int> CommitAsync(CancellationToken ct = default)
        => await SaveChangesAsync(ct).ConfigureAwait(false);
}
