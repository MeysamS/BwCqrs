using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Base;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Commands.Services;

public class InternalCommandProcessor : BackgroundService
{
    private readonly IInternalCommandStore _store;
    private readonly ICommandBus _commandBus;
    private readonly ILogger<InternalCommandProcessor> _logger;

    public InternalCommandProcessor(
        IInternalCommandStore store,
        ICommandBus commandBus,
        ILogger<InternalCommandProcessor> logger)
    {
        _store = store;
        _commandBus = commandBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pendingCommands = await _store.GetPendingCommandsAsync();
                
                foreach (var command in pendingCommands)
                {
                    try
                    {
                        await _commandBus.DispatchAsync(command);
                        await _store.MarkAsProcessedAsync(((CommandBase)command).Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing internal command {CommandId}", ((CommandBase)command).Id);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in internal command processor");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
} 