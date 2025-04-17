namespace Bw.Cqrs.Queries.Contracts;

/// <summary>
/// Factory for creating query handlers
/// </summary>
public interface IQueryHandlerFactory
{
    /// <summary>
    /// Creates a query handler for the specified query type
    /// </summary>
    /// <typeparam name="TQuery">The type of query</typeparam>
    /// <typeparam name="TResponse">The type of response</typeparam>
    IQueryHandler<TQuery, TResponse> Create<TQuery, TResponse>()
        where TQuery : IQuery<TResponse>
        where TResponse : class;
} 