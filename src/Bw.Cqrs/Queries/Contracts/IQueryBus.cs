namespace Bw.Cqrs.Queries.Contracts;

public interface IQueryBus
{
    Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query)
        where TResponse : class;
} 