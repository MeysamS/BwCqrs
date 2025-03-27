using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Configuration;

public interface ICqrsBuilder
{
    IServiceCollection Services { get; }
} 