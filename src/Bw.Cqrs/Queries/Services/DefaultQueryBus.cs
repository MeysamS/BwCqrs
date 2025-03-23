using Bw.Cqrs.Queries.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Queries.Services;

public class DefaultQueryBus : IQueryBus
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultQueryBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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