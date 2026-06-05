// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Repository.Abstractions;

/// <summary>
/// Provides extension methods to <see cref="IRepository"/>.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Finds an entity by the value(s) of its unique, possibly composite, primary store key.<br/>
    /// The method searches first in the change-tracker and if not found - in the underlying physical store.<br/>
    /// Since the search is done by the primary key(s), the operation is usually faster and cheaper than other queries.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="repository">The repository instance to search.</param>
    /// <param name="keyValues">
    /// The primary key(s) as a variable list of parameters - <see langword="params"/>.<br/>
    /// Note that the order of the key values in a composite key is important.<br/>
    /// After the last key value you can also pass a <see cref="CancellationToken"/> to allow for cancellation of the operation.
    /// </param>
    /// <returns><see cref="Task{T}"/> which contains the found instance or <see langword="null"/> if not found.</returns>
    /// <remarks>Note that the method is asynchronous.</remarks>
    public static ValueTask<T?> FindAsync<T>(this IRepository repository, params object?[] keyValues)
        where T : class
    {
        ThrowIfNullOrEmpty(nameof(keyValues));

        var ct = default(CancellationToken);

        if (keyValues?[^1] is CancellationToken)
        {
            ct = keyValues[^1];
            keyValues = keyValues[..^1];
        }
        
        return repository.FindAsync<T>(keyValues, ct);
    }

    /// <summary>
    /// Finds an entity with the same primary key property value(s) as the keys in the <see cref="IFindable.KeyValues"/> from the
    /// <paramref name="findable"/>. All other properties of <paramref name="findable"/> are ignored.<br/>
    /// The method searches first in the change-tracker and if not found - in the underlying physical store.<br/>
    /// Since the search is done by the primary key(s), the operation is usually faster and cheaper than other queries.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="repository">The repository instance to search.</param>
    /// <param name="findable">An object containing the key values used to locate the entity.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns><see cref="ValueTask{T}"/> which contains the found instance or <see langword="null"/> if not found.</returns>
    /// <remarks>Note that the method is asynchronous.</remarks>
    public static async ValueTask<T?> FindAsync<T>(this IRepository repository, IFindable findable, CancellationToken ct = default)
        where T : class
    {
        await findable.ValidateFindableAsync(repository, ct).ConfigureAwait(false);
        return await repository.FindAsync<T>(findable.KeyValues, ct).ConfigureAwait(false);
    }
}