using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bw.Cqrs.Configuration;

public interface ICqrsBuilder
{
    IServiceCollection Services { get; }
    IEnumerable<Assembly> Assemblies { get; }
} 