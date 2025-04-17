namespace Bw.Cqrs.Queries.Contracts;

/// <summary>
/// Represents a query that can be sent to the query bus
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IQuery<TResponse>
    where TResponse : class
{
} 