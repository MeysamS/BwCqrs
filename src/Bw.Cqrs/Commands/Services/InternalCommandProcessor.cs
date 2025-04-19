using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Configuration;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bw.Cqrs.Commands.Services;

/// <summary>
/// Background service that processes scheduled internal commands
/// </summary>
public class InternalCommandProcessor : BackgroundService
{
    // private readonly IInternalCommandStore? _store;
    // private readonly ICommandBus? _commandBus;
    private readonly ILogger<InternalCommandProcessor> _logger;
    private readonly InternalCommandOptions _options;
    private readonly IServiceProvider _serviceProvider;
    /// <summary>
    /// Initializes a new instance of the InternalCommandProcessor class    
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="options">The internal command options</param>
    /// <param name="logger">The logger</param>
    public InternalCommandProcessor(
        IServiceProvider serviceProvider,
        IOptions<InternalCommandOptions> options,
        ILogger<InternalCommandProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }
    /// <summary>
    /// Executes the internal command processor background service 
    /// </summary>
    /// <param name="stoppingToken">The cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Internal Command Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingCommandsAsync(stoppingToken);
                await CleanupOldCommandsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(_options.ProcessingIntervalSeconds), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in command processor loop");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task ProcessPendingCommandsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IInternalCommandStore>();
        var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();

        var commands = await store.GetCommandsToExecuteAsync(stoppingToken);

        foreach (var command in commands)
        {
            try
            {
                await ProcessCommandAsync(command, store, commandProcessor, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing command {CommandId}", ((CommandBase)command).Id);
            }
        }
    }

    private async Task ProcessCommandAsync(
        InternalCommand command,
        IInternalCommandStore store,
        ICommandProcessor commandProcessor,
        CancellationToken stoppingToken)
    {
        try
        {
            await store.UpdateStatusAsync(command.Id, InternalCommandStatus.Processing, cancellationToken: stoppingToken);

            await commandProcessor.DispatchAsync(command);

            await store.UpdateStatusAsync(command.Id, InternalCommandStatus.Processed, cancellationToken: stoppingToken);

            _logger.LogInformation(
                "Successfully processed command {CommandId} of type {CommandType}",
                command.Id,
                command.GetType().Name);
        }
        catch (Exception ex)
        {
            var error = ex.InnerException?.Message ?? ex.Message;
            await store.UpdateStatusAsync(command.Id, InternalCommandStatus.Failed, error, stoppingToken);

            if (command.RetryCount < _options.MaxRetries)
            {
                _logger.LogWarning(
                    "Command {CommandId} failed (Attempt {RetryCount} of {MaxRetries}): {Error}",
                    command.Id,
                    command.RetryCount + 1,
                    _options.MaxRetries,
                    error);
            }
            else
            {
                _logger.LogError(
                    "Command {CommandId} failed permanently after {MaxRetries} retries: {Error}",
                    command.Id,
                    _options.MaxRetries,
                    error);
            }
        }
    }

    private async Task CleanupOldCommandsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IInternalCommandStore>();

            var cutoffDate = DateTime.UtcNow.AddDays(-_options.RetentionDays);
            await store.CleanupAsync(cutoffDate, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old commands");
        }
    }
}