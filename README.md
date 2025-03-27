# BitWrite CQRS Library

A lightweight, flexible, and feature-rich CQRS (Command Query Responsibility Segregation) implementation for .NET applications. This library provides a clean and maintainable way to separate read and write operations in your application.

## Features

### Command Handling
- ✨ Strongly-typed command handling with base command types (Create, Update, Delete)
- 🔄 Command pipeline behaviors (middleware)
- ✅ Built-in validation using FluentValidation
- 📝 Comprehensive logging support
- 🔄 Delayed command processing (Outbox pattern)
- 🎯 Command result handling
- 🏭 Dependency injection support
- 🔁 Retry mechanism with configurable policies
- ⚠️ Sophisticated error handling
- 📦 Optional command versioning support
- ⏰ Advanced command scheduling

### Query Handling
- 🔍 Strongly-typed query handling
- 🚀 Async/await support
- 🎯 Clean separation of read and write operations
- 🏭 Dependency injection integration

### Common Features
- 🛠️ Builder pattern for configuration
- 📦 Easy integration with dependency injection
- 🔍 Automatic handler registration
- 🎯 Type-safe implementations
- 📝 Comprehensive logging
- ⚡ High performance

## Installation

```bash
dotnet add package BitWrite.Cqrs
```

## Quick Start

### 1. Register CQRS Services

```csharp
services.AddBwCqrs(builder =>
{
    builder
        .AddValidation()    // Add validation behavior
        .AddLogging()       // Add logging behavior
        .AddErrorHandling() // Add error handling
        .AddRetry(maxRetries: 3, delayMilliseconds: 1000); // Add retry mechanism
}, typeof(Program).Assembly);
```

### 2. Command Handling Examples

#### Create Command
```csharp
// Define the request model
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Define the command
public class CreateUserCommand : CreateCommand<CreateUserRequest>
{
    public CreateUserCommand(CreateUserRequest data) : base(data)
    {
    }
}

// Add validation
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Data.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Data.Email).NotEmpty().EmailAddress();
    }
}

// Implement the handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IResult> HandleAsync(CreateUserCommand command)
    {
        var user = new User(command.Data.Username, command.Data.Email);
        await _userRepository.AddAsync(user);
        return CommandResult.Success();
    }
}
```

#### Update Command
```csharp
public class UpdateUserRequest
{
    public string? Email { get; set; }
    public bool NewsletterSubscription { get; set; }
}

public class UpdateUserCommand : UpdateCommand<UpdateUserRequest>
{
    public UpdateUserCommand(Guid userId, UpdateUserRequest data) : base(userId, data)
    {
    }
}

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IResult> HandleAsync(UpdateUserCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.EntityId);
        if (user == null)
            return CommandResult.Failure("User not found");

        if (command.Data.Email != null)
            user.UpdateEmail(command.Data.Email);
        
        user.SetNewsletterPreference(command.Data.NewsletterSubscription);
        
        await _userRepository.UpdateAsync(user);
        return CommandResult.Success();
    }
}
```

#### Delete Command
```csharp
public class DeleteUserCommand : DeleteCommand
{
    public DeleteUserCommand(Guid userId) : base(userId)
    {
    }
}

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IResult> HandleAsync(DeleteUserCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.EntityId);
        if (user == null)
            return CommandResult.Failure("User not found");

        await _userRepository.DeleteAsync(user);
        return CommandResult.Success();
    }
}
```

### 3. Using Command Versioning

When you need to version your commands, you can implement the `IVersionedCommand` interface:

```csharp
public class CreateUserCommand : CreateCommand<CreateUserRequest>, IVersionedCommand
{
    public int Version { get; }

    public CreateUserCommand(CreateUserRequest data, int version = 1) : base(data)
    {
        Version = version;
    }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    public async Task<IResult> HandleAsync(CreateUserCommand command)
    {
        var user = new User(command.Data.Username, command.Data.Email);
        
        if (command.Version >= 2)
        {
            // Handle version 2 specific logic
            user.SetNewsletterPreference(command.Data.SubscribeToNewsletter);
        }

        await _userRepository.AddAsync(user);
        return CommandResult.Success();
    }
}
```

### 4. Error Handling and Retries

The library includes built-in error handling and retry mechanisms:

```csharp
public class UserService
{
    private readonly ICommandBus _commandBus;

    public UserService(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var command = new CreateUserCommand(request);
            // ErrorHandlingBehavior will catch and process any errors
            // RetryBehavior will automatically retry on transient failures
            await _commandBus.DispatchAsync(command);
        }
        catch (CommandRetryException ex)
        {
            // Handle after all retries have failed
        }
    }
}
```

## Best Practices

1. **Command Structure**: 
   - Use `CreateCommand<T>` for creation operations
   - Use `UpdateCommand<T>` for update operations
   - Use `DeleteCommand` for delete operations

2. **Command Naming**: 
   - Use imperative verb phrases (e.g., `CreateUser`, `UpdateProfile`)
   - Keep command names clear and descriptive

3. **Request Models**:
   - Create separate request models for commands
   - Keep request models immutable when possible
   - Include only necessary data

4. **Validation**:
   - Always validate commands before processing
   - Use FluentValidation for complex validation rules
   - Validate at the command level, not just the request model

5. **Error Handling**:
   - Use the built-in error handling behavior
   - Return appropriate CommandResults
   - Log errors with proper context

6. **Versioning**:
   - Implement IVersionedCommand only when needed
   - Handle version-specific logic in command handlers
   - Document version changes

7. **Testing**:
   - Write unit tests for command handlers
   - Test validation rules
   - Test different versions if using versioning

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## PostgreSQL Integration

### PostgreSQL Outbox Pattern Support
The library provides built-in support for storing and processing internal commands using PostgreSQL as a reliable outbox storage.

### Installation

```bash
dotnet add package BitWrite.Cqrs.Postgres
```

### Configuration

Add PostgreSQL support to your CQRS setup:

```csharp
// In Program.cs or Startup.cs
services.AddCqrs(options =>
{
    options.AddValidation()
           .AddLogging();
}, typeof(Program).Assembly);

// Add PostgreSQL support with your connection string
services.AddPostgresCqrs("Host=localhost;Database=your_db;Username=your_user;Password=your_password");
```

### Usage Example

```csharp
// Define an internal command that needs to be processed later
public class SendWelcomeEmailCommand : InternalCommandBase
{
    public string UserEmail { get; set; }
    public string Username { get; set; }
}

// Schedule the command for later processing
public class UserRegistrationService
{
    private readonly ICommandBus _commandBus;

    public UserRegistrationService(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    public async Task RegisterUserAsync(string username, string email)
    {
        // ... register user ...

        // Schedule welcome email to be sent later
        var command = new SendWelcomeEmailCommand
        {
            Username = username,
            UserEmail = email
        };

        // Command will be stored in PostgreSQL and processed by background service
        await _commandBus.ScheduleAsync(command);
    }
}

// Implement the command handler
public class SendWelcomeEmailCommandHandler : ICommandHandler<SendWelcomeEmailCommand>
{
    private readonly IEmailService _emailService;

    public SendWelcomeEmailCommandHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<IResult> HandleAsync(SendWelcomeEmailCommand command)
    {
        await _emailService.SendWelcomeEmailAsync(command.UserEmail, command.Username);
        return CommandResult.Success();
    }
}
```

### Features

- 🗄️ Reliable storage of internal commands in PostgreSQL
- 📊 Command state tracking (Scheduled, Processed, Failed)
- 🔄 Automatic processing through background service
- 📝 Error logging and handling
- 🔍 Easy querying of command status
- ⚡ High performance with Entity Framework Core
- 🧪 Integration tests with Testcontainers

### Database Schema

The PostgreSQL integration creates the following table:

```sql
CREATE TABLE internal_commands (
    id UUID PRIMARY KEY,
    type VARCHAR(500) NOT NULL,
    data TEXT NOT NULL,
    scheduled_on TIMESTAMP NOT NULL,
    processed_on TIMESTAMP NULL,
    error VARCHAR(2000) NULL
);
```

### Advanced Usage

#### Custom Command Processing Retry Logic

```csharp
public class CustomInternalCommandProcessor : BackgroundService
{
    private readonly IInternalCommandStore _store;
    private readonly ICommandBus _commandBus;
    private readonly ILogger<CustomInternalCommandProcessor> _logger;

    public CustomInternalCommandProcessor(
        IInternalCommandStore store,
        ICommandBus commandBus,
        ILogger<CustomInternalCommandProcessor> logger)
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
                        await _store.MarkAsProcessedAsync(command.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process command {CommandId}", command.Id);
                        await _store.MarkAsFailedAsync(command.Id, ex);
                        // Implement your retry logic here
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in command processor");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
```

#### Monitoring Command Status

```csharp
public class CommandMonitoringService
{
    private readonly CqrsDbContext _dbContext;

    public CommandMonitoringService(CqrsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommandStats> GetCommandStatsAsync()
    {
        var stats = new CommandStats
        {
            TotalCommands = await _dbContext.InternalCommands.CountAsync(),
            PendingCommands = await _dbContext.InternalCommands
                .CountAsync(x => x.ProcessedOn == null),
            FailedCommands = await _dbContext.InternalCommands
                .CountAsync(x => x.Error != null)
        };

        return stats;
    }
}
```

### Best Practices

1. **Command Idempotency**: Ensure your command handlers are idempotent as commands might be processed multiple times.
2. **Error Handling**: Always implement proper error handling in your command handlers.
3. **Monitoring**: Set up monitoring for failed commands and processing delays.
4. **Database Maintenance**: Implement a cleanup strategy for processed commands.
5. **Performance**: Index the `processed_on` column for better query performance. 
