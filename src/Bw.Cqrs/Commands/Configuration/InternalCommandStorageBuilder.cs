using Microsoft.Extensions.DependencyInjection;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Services;
using Bw.Cqrs.Configuration;

namespace Bw.Cqrs.Commands.Configuration;

/// <summary>
/// Builder for configuring internal command storage providers
/// </summary>
public class InternalCommandStorageBuilder
{
    private readonly IServiceCollection _services;
    private readonly ICqrsBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the InternalCommandStorageBuilder class
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="builder">The CQRS builder</param>
    public InternalCommandStorageBuilder(IServiceCollection services, ICqrsBuilder builder)
    {
        _services = services;
        _builder = builder;
    }

    /// <summary>
    /// Gets the service collection for registering services
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// Gets the CQRS builder for method chaining
    /// </summary>
    public ICqrsBuilder Builder => _builder;

    /// <summary>
    /// Uses the default in-memory storage provider
    /// </summary>
    public ICqrsBuilder UseInMemory()
    {
        _services.AddScoped<IInternalCommandStore, InMemoryInternalCommandStore>();
        return _builder;
    }
} 