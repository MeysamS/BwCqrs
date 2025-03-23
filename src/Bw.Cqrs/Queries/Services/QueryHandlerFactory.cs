using Bw.Cqrs.Queries.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Queries.Services;

public class QueryHandlerFactory : IQueryHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryHandlerFactory> _logger;

    public QueryHandlerFactory(IServiceProvider serviceProvider, ILogger<QueryHandlerFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

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