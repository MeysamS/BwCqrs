using Bw.Cqrs.Queries.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Queries.Services;

/// <summary>
/// Default implementation of IQueryBus
/// </summary>
public class DefaultQueryBus : IQueryBus
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the DefaultQueryBus class
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public DefaultQueryBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Sends a query to the query bus
    /// </summary>
    /// <typeparam name="TResponse">The type of response</typeparam>
    /// <param name="query">The query</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query)
        where TResponse : class
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync");
        if (method is null)
            throw new InvalidOperationException($"Method HandleAsync not found on handler type {handlerType.Name}");

        var result = await (Task<TResponse>)method.Invoke(handler, new object[] { query })!;
        return result;
    }
} 