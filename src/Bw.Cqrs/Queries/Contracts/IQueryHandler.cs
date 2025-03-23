namespace Bw.Cqrs.Queries.Contracts;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : class
{
    Task<TResponse> HandleAsync(TQuery query);
} 