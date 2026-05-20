namespace vm2.Repository.Abstractions;

/// <summary>
/// Represents the behavior of abstract objects that implement the popular repository pattern the way it was meant by <br/>
/// M.Fowler (see Patterns of Enterprise Application Architecture).
/// <list type="bullet"><item>
/// "A layer that isolates domain objects from the details of the database access code."
/// </item><item>
/// "Has another layer of abstraction over the mapping layer where query construction code is concentrated.<br/>
/// This becomes more important when there are a large number of domain classes or heavy querying."
/// </item><item>
/// "Mediates between the domain and data mapping layers, acting like an in-memory domain object collection."
/// </item><item>"Client objects construct query specifications declaratively and submit them to Repository for
/// satisfaction."
/// </item><item>
/// "Objects can be added to and removed from the Repository, as they can from a simple collection of objects, <br/>
/// and the mapping code encapsulated by the Repository will carry out the appropriate operations behind the scenes."
/// </item><item>
/// "Clean separation and one-way dependency between the domain and data mapping layers."
/// </item></list>
/// "Conceptually, a Repository encapsulates the set of objects persisted in a data store and the operations performed<br/>
/// over them, providing a more object-oriented view of the persistence layer."<br/>
/// This repository interface borrows heavily from the behavior of Entity Framework's <see cref="DbContext"/> and LINQ, <br/>
/// however it is conceivable that it can be implemented with other ORM, e.g. NHibernate.
/// </summary>
public interface IRepository : IAsyncDisposable
{
    /// <summary>
    /// Represents an abstract collection of domain objects (entities) of type <typeparamref name="T"/>. Since the entity set is
    /// represented as <see cref="IQueryable{T}"/>, the <c>IRepository</c>'s clients can declaratively construct LINQ queries.
    /// Entities can be added to the set (<see cref="AddAsync"/>), and removed from the set (<see cref="Remove"/>).
    /// </summary>
    /// <typeparam name="T">The type of the entity in the set.</typeparam>
    /// <returns><see cref="IQueryable{T}"/>.</returns>
    /// <remarks>
    /// Note that the returned <see cref="IQueryable"/> for this implementation of the pattern should be supported not only by
    /// <see cref="IEnumerable{T}"/> and <see cref="IQueryProvider"/> but also by <see cref="IAsyncEnumerable{T}"/>, and
    /// <see cref="IAsyncQueryProvider"/> from <see cref="Microsoft.EntityFrameworkCore.Query"/>. These would allow the caller
    /// to call asynchronously immediate execution LINQ methods like <see cref="EntityFrameworkQueryableExtensions.ToListAsync{T}"/> and
    /// <see cref="EntityFrameworkQueryableExtensions.CountAsync{T}(IQueryable{T}, CancellationToken)"/>.<br/>
    /// If the returned <see cref="IQueryable"/> object is the Entity Framework's <see cref="DbSet{TEntity}"/> some methods may
    /// be executed first on the internal in-memory entity change-tracker and then, if needed, in the underlying physical store,
    /// e.g. <see cref="DbSet{TEntity}.FindAsync(object[])"/> and <see cref="DbSet{TEntity}.AddAsync"/>.
    /// </remarks>
    IQueryable<T> Set<T>() where T : class;

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
    ValueTask<T?> FindAsync<T>(IEnumerable<object?>? keyValues, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Adds the <paramref name="entity"/> to the change-tracker (in memory) in "Added" state. The method usually finishes
    /// synchronously. However, if a whole graph of entities is added and some of the primary keys are generated at the store,
    /// the ORM may need to add some of the entities to the store now, to obtain the primary keys and fix-up the values of some
    /// of the foreign keys of the dependent entities.  The entity is added to the data store on <see cref="IRepository.CommitAsync"/>.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to add.</param>
    /// <param name="ct">Can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns><see cref="ValueTask{T}"/> which contains the added instance.</returns>
    /// <remarks>Note that the method is asynchronous.</remarks>
    ValueTask<T> AddAsync<T>(T entity, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Attaches the specified entity to the internal object change-tracker in a "Modified" state. At the next
    /// <see cref="IRepository.CommitAsync"/> the ORM will update the entities in "Modified" state in the DB.
    /// <para>
    /// Use with caution! The use of this method implies that the code "knows" what is the current state of the entity better
    /// than the DB. In effect the DB stops being the ultimate source of truth. This is a strategy sometimes known as
    /// "client-wins" vs. "store-wins".
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The modified entity instance.</param>
    /// <returns>The added entity.</returns>
    T Update<T>(T entity) where T : class;

    /// <summary>
    /// Attaches the specified entity to the change-tracker in a "Unmodified" state. If any changes are done to the entity, its<br/>
    /// state will transition to "Modified". At the next <see cref="IRepository.CommitAsync"/> the ORM will update the entities in<br/>
    /// "Modified" state in the DB and will ignore the "Unmodified" entities.<br/>
    /// <para>
    /// Use with caution! The use of this method implies that the code "knows" what is the current state of the entity better<br/>
    /// than the repository or the DB. In effect the DB stops being the ultimate source of truth. This is a strategy sometimes<br/>
    /// known as "client-wins" (vs. "store-wins").
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to be attached.</param>
    /// <returns>The entity.</returns>
    T Attach<T>(T entity) where T : class;

    /// <summary>
    /// If the <paramref name="entity"/> is not in the tracker yet, the method will add it in "Deleted" state.<br/>
    /// If it is already there, the tracker will make sure that the entity's state is "Deleted".<br/>
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
    /// async Task Delete(Id entityId)
    /// {
    ///     ...
    ///     Entity entity = new() { Id = entityId };
    ///     // NO trip to the DB:
    ///     _repository.Remove(entity);
    ///     ...
    ///     // trip to the DB:
    ///     await _repository.CommitAsync();
    /// }
    /// ]]></code>instead of:<code><![CDATA[
    /// async Task Delete(Id entityId)
    /// {
    ///     ...
    ///     // trip to the DB!
    ///     Entity entity = await _repository.FindAsync(entityId);
    ///     _repository.Remove(entity);
    ///     ...
    ///     // second trip to the DB
    ///     await _repository.CommitAsync();
    /// }
    /// ]]></code>
    /// </remarks>
    T Remove<T>(T entity) where T : class;

    /// <summary>
    /// Commits the added, modified and deleted entities in the change-tracker to the physical store invoking the respective <br/>
    /// back-end actions. For some DB-s  the action is a single transaction, for others it is a sequence of transactions.
    /// </summary>
    /// <param name="ct">Can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns><see cref="Task{Int32}"/> which contains the number of entities that were affected by the commit.</returns>
    /// <remarks>Note that the method is asynchronous.</remarks>
    Task<int> CommitAsync(CancellationToken ct = default);
}
