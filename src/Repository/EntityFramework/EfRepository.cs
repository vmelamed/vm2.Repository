// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Repository.EntityFramework;

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
    /// <summary>
    /// Initializes a new instance of the <see cref="EfRepository"/> class.
    /// </summary>
    public EfRepository()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EfRepository"/> class using the specified options.
    /// </summary>
    /// <param name="options">The <see cref="DbContextOptions"/> used to configure the context.</param>
    public EfRepository(DbContextOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// Shortcut method to throw <see cref="NotSupportedException"/> for the synchronous methods of the repository.
    /// The message of the exception will contain the name of the asynchronous method that should be used instead.
    /// </summary>
    /// <param name="asyncMethodName"></param>
    /// <exception cref="NotSupportedException"></exception>
    [DoesNotReturn]
    protected void SyncNotImplemented<T>(string asyncMethodName)
        => throw new NotSupportedException($"The repository does not support synchronous methods. Use {asyncMethodName} instead.");
}
