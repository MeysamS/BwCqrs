namespace Bw.Cqrs.Queries.Contracts;

/// <summary>
/// Represents a query bus that can send queries and receive responses
/// </summary>
public interface IQueryProcessor
{
    /// <summary>
    /// Sends a query to the query bus and returns a response
    /// </summary>
    /// <typeparam name="TResponse">The type of response</typeparam>
    /// <param name="query">The query to send</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query)
        where TResponse : class;
} 