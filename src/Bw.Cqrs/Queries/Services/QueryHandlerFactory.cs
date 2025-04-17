using Bw.Cqrs.Queries.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Queries.Services;

/// <summary>
/// Default implementation of IQueryHandlerFactory
/// </summary>
public class QueryHandlerFactory : IQueryHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryHandlerFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the QueryHandlerFactory class
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="logger">The logger</param>
    public QueryHandlerFactory(IServiceProvider serviceProvider, ILogger<QueryHandlerFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Creates a query handler for the specified query type
    /// </summary>
    /// <typeparam name="TQuery">The type of query</typeparam>
    /// <typeparam name="TResponse">The type of response</typeparam>
    /// <returns>A query handler</returns>
    public IQueryHandler<TQuery, TResponse> Create<TQuery, TResponse>()
        where TQuery : IQuery<TResponse>
        where TResponse : class
    {
        _logger.LogDebug("Creating query handler for {QueryType}", typeof(TQuery).Name);
        
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        
        if (handler == null)
        {
            _logger.LogError("No query handler found for {QueryType}", typeof(TQuery).Name);
            throw new InvalidOperationException($"No query handler found for {typeof(TQuery).Name}");
        }

        return handler;
    }
} 