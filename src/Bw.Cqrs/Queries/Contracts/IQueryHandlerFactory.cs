namespace Bw.Cqrs.Queries.Contracts;

public interface IQueryHandlerFactory
{
    IQueryHandler<TQuery, TResponse> Create<TQuery, TResponse>()
        where TQuery : IQuery<TResponse>
        where TResponse : class;
} 