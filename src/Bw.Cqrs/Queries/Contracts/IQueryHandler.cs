namespace Bw.Cqrs.Queries.Contracts;

/// <summary>
/// Represents a query handler that can handle a query and return a response
/// </summary>
/// <typeparam name="TQuery">The type of query</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : class
{
    /// <summary>
    /// Handles the query and returns a response
    /// </summary>
    /// <param name="query">The query to handle</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<TResponse> HandleAsync(TQuery query);
} 