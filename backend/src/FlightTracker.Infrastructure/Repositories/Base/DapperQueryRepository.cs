using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FlightTracker.Infrastructure.Repositories.Base;

/// <summary>
/// Base class for Dapper-based read operations with performance optimizations
/// </summary>
public abstract class DapperQueryRepository
{
    protected readonly string _connectionString;
    protected readonly ILogger _logger;

    protected DapperQueryRepository(string connectionString, ILogger logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute a query and return results
    /// </summary>
    protected async Task<IEnumerable<T>> QueryAsync<T>(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            var result = await connection.QueryAsync<T>(sql, parameters);
            
            _logger.LogDebug("Executed query returning {Count} results", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {Sql}", sql);
            throw;
        }
    }

    /// <summary>
    /// Execute a query and return a single result
    /// </summary>
    protected async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            var result = await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
            
            _logger.LogDebug("Executed single query");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing single query: {Sql}", sql);
            throw;
        }
    }

    /// <summary>
    /// Execute multi-mapping query for complex joins
    /// </summary>
    protected async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        string sql,
        Func<TFirst, TSecond, TReturn> map,
        object? parameters = null,
        string splitOn = "Id",
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            var result = await connection.QueryAsync<TFirst, TSecond, TReturn>(
                sql, map, parameters, splitOn: splitOn);
            
            _logger.LogDebug("Executed multi-mapping query returning {Count} results", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing multi-mapping query: {Sql}", sql);
            throw;
        }
    }

    /// <summary>
    /// Execute multi-mapping query for three tables
    /// </summary>
    protected async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
        string sql,
        Func<TFirst, TSecond, TThird, TReturn> map,
        object? parameters = null,
        string splitOn = "Id",
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            var result = await connection.QueryAsync<TFirst, TSecond, TThird, TReturn>(
                sql, map, parameters, splitOn: splitOn);
            
            _logger.LogDebug("Executed three-table multi-mapping query returning {Count} results", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing three-table multi-mapping query: {Sql}", sql);
            throw;
        }
    }

    /// <summary>
    /// Execute a scalar query (count, sum, etc.)
    /// </summary>
    protected async Task<T> ExecuteScalarAsync<T>(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            var result = await connection.ExecuteScalarAsync<T>(sql, parameters);
            
            _logger.LogDebug("Executed scalar query");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scalar query: {Sql}", sql);
            throw;
        }
    }

    /// <summary>
    /// Execute a command (insert, update, delete)
    /// </summary>
    protected async Task<int> ExecuteAsync(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            var result = await connection.ExecuteAsync(sql, parameters);
            
            _logger.LogDebug("Executed command affecting {RowCount} rows", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Sql}", sql);
            throw;
        }
    }

    /// <summary>
    /// Execute multiple queries in a single connection
    /// </summary>
    protected async Task<TResult> QueryMultipleAsync<TResult>(
        string sql,
        Func<SqlMapper.GridReader, Task<TResult>> map,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            using var multi = await connection.QueryMultipleAsync(sql, parameters);
            var result = await map(multi);
            
            _logger.LogDebug("Executed multiple queries");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing multiple queries: {Sql}", sql);
            throw;
        }
    }
}
