using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bw.Cqrs.Configuration;

/// <summary>
/// Represents a builder for the CQRS framework
/// </summary>
public interface ICqrsBuilder
{
    /// <summary>
    /// Gets the service collection
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the assemblies
    /// </summary>
    IEnumerable<Assembly> Assemblies { get; }
} 