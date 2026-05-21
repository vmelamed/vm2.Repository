namespace vm2.Repository.EntityFramework;

using System.Data.Common;

/// <summary>
/// Provides extension methods for <see cref="DbConnectionStringBuilder"/> to simplify access to well-known connection
/// string elements. For more information on DB-specific connection strings, see <see href="https://www.connectionstrings.com/"/>.
/// </summary>
public static class DbConnectionStringBuilderExtensions
{
    /// <summary>
    /// Retrieves the string value associated with of the specified key from the <paramref name="connectionStringBuilder"/>.
    /// </summary>
    /// <param name="connectionStringBuilder">
    /// The <see cref="DbConnectionStringBuilder"/> instance containing the connection string elements.
    /// </param>
    /// <param name="key">The key of the element to retrieve.</param>
    /// <returns>
    /// The value associated with the specified key, or <c>null</c> if the key does not exist.
    /// </returns>
    public static string? GetStringElement(this DbConnectionStringBuilder connectionStringBuilder, string key)
        => connectionStringBuilder.TryGetValue(key, out var value) ? (string)value : null;

    /// <summary>
    /// Retrieves the integer value associated with the specified key from the <paramref name="connectionStringBuilder"/>.
    /// </summary>
    /// <param name="connectionStringBuilder">
    /// The <see cref="DbConnectionStringBuilder"/> instance containing the connection string elements.
    /// </param>
    /// <param name="key">The key whose associated integer value is to be retrieved.</param>
    /// <returns>
    /// The integer value associated with the specified key, or <see langword="null"/> if the key does not exist or the value
    /// cannot be parsed as an integer.
    /// </returns>
    public static int? GetIntElement(this DbConnectionStringBuilder connectionStringBuilder, string key)
        => int.TryParse(connectionStringBuilder.GetStringElement(key), out var value) ? value : null;

    /// <summary>
    /// Retrieves the boolean value associated with the specified key from the <paramref name="connectionStringBuilder"/>.
    /// </summary>
    /// <param name="connectionStringBuilder">
    /// The <see cref="DbConnectionStringBuilder"/> instance containing the connection string elements.
    /// </param>
    /// <param name="key">The key whose associated boolean value is to be retrieved.</param>
    /// <returns>
    /// The boolean value associated with the specified key, or <see langword="null"/> if the key does not exist or the value
    /// cannot be parsed as an integer.
    /// </returns>
    public static bool? GetBoolElement(this DbConnectionStringBuilder connectionStringBuilder, string key)
        => bool.TryParse(connectionStringBuilder.GetStringElement(key), out var value) ? value : null;

    /// <summary>
    /// Gets the server or host name from the connection string.  DBs: SQL Server
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The server or host name, or <c>null</c> if not found.</returns>
    public static string? GetServer(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("Server")
                ?? connectionStringBuilder.GetStringElement("Host");

    /// <summary>
    /// Gets the account endpoint from the connection string.  DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The account endpoint, or <c>null</c> if not found.</returns>
    public static string? GetAccountEndpoint(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("AccountEndpoint");

    /// <summary>
    /// Gets the account key from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The account key, or <c>null</c> if not found.</returns>
    public static string? GetAccountKey(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("GetAccountKey");

    /// <summary>
    /// Gets the database name from the connection string. DBs: SQL Server, SQLite
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The database name, or <c>null</c> if not found.</returns>
    public static string? GetDatabase(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("Database")
            ?? connectionStringBuilder.GetStringElement("Initial Catalog")
                ?? connectionStringBuilder.GetStringElement("Data ActivitySource");

    /// <summary>
    /// Gets the user ID from the connection string. DBs: SQL Server
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The user ID, or <c>null</c> if not found.</returns>
    public static string? GetUserId(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("User Id")
                ?? connectionStringBuilder.GetStringElement("UID");

    /// <summary>
    /// Gets the password from the connection string. DBs: SQL Server
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The password, or <c>null</c> if not found.</returns>
    public static string? GetPassword(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("Password")
                ?? connectionStringBuilder.GetStringElement("PWD");

    /// <summary>
    /// Gets the attached database filename from the connection string. DBs: SQL Server
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The attached database filename, or <c>null</c> if not found.</returns>
    public static string? GetAttachDbFilename(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("AttachDbFilename");

    /// <summary>
    /// Gets the application name from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The application name, or <c>null</c> if not found.</returns>
    public static string? GetApplication(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("Application Name");

    /// <summary>
    /// Gets the failover partner from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The failover partner, or <c>null</c> if not found.</returns>
    public static string? GetFailoverPartner(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("Failover Partner");

    /// <summary>
    /// Gets the SSL mode from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The SSL mode, or <c>null</c> if not found.</returns>
    public static string? GetSslMode(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetStringElement("SslMode");

    /// <summary>
    /// Gets the asynchronous processing setting from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns><c>true</c> if asynchronous processing is enabled; <c>false</c> if disabled; <c>null</c> if not set.</returns>
    public static bool? GetAsynchronousProcessing(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetBoolElement("Asynchronous Processing");

    /// <summary>
    /// Gets the trusted connection setting from the connection string. DBs: SQL Server
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns><c>true</c> if trusted connection is enabled; <c>false</c> if disabled; <c>null</c> if not set.</returns>
    public static bool? GetTrustedConnection(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetBoolElement("Trusted_Connection");

    /// <summary>
    /// Gets the multiple active result sets setting from the connection string. DBs: SQL Server
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns><c>true</c> if multiple active result sets are enabled; <c>false</c> if disabled; <c>null</c> if not set.</returns>
    public static bool? GetMultipleActiveResultSets(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetBoolElement("MultipleActiveResultSets");

    /// <summary>
    /// Gets the integrated security setting from the connection string. DBs: SQL Server
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns><c>true</c> if integrated security is enabled; <c>false</c> if disabled; <c>null</c> if not set.</returns>
    public static bool? GetIntegratedSecurity(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetBoolElement("Integrated Security");

    /// <summary>
    /// Gets the trust server certificate setting from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns><c>true</c> if trust server certificate is enabled; <c>false</c> if disabled; <c>null</c> if not set.</returns>
    public static bool? GetTrustServerCertificate(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetBoolElement("trustservercertificate");

    /// <summary>
    /// Gets the SSL setting from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns><c>true</c> if SSL is enabled; <c>false</c> if disabled; <c>null</c> if not set.</returns>
    public static bool? GetSsl(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetBoolElement("SSL");

    /// <summary>
    /// Gets the pooling setting from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns><c>true</c> if pooling is enabled; <c>false</c> if disabled; <c>null</c> if not set.</returns>
    public static bool? GetPooling(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetBoolElement("Pooling");

    /// <summary>
    /// Gets the port number from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The port number, or <c>null</c> if not found or invalid.</returns>
    public static int? GetPort(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetIntElement("Port");

    /// <summary>
    /// Gets the minimum pool size from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The minimum pool size, or <c>null</c> if not found or invalid.</returns>
    public static int? GetMinPoolSize(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetIntElement("Min Pool Size");

    /// <summary>
    /// Gets the maximum pool size from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The maximum pool size, or <c>null</c> if not found or invalid.</returns>
    public static int? GetMaxPoolSize(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetIntElement("Max Pool Size");

    /// <summary>
    /// Gets the connection lifetime from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The connection lifetime, or <c>null</c> if not found or invalid.</returns>
    public static int? GetConnectionLifetime(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetIntElement("Connection Lifetime");

    /// <summary>
    /// Gets the command timeout from the connection string. DBs:
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder instance.</param>
    /// <returns>The command timeout, or <c>null</c> if not found or invalid.</returns>
    public static int? GetCommandTimeout(this DbConnectionStringBuilder connectionStringBuilder)
        => connectionStringBuilder.GetIntElement("CommandTimeout");
}
