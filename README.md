# BitWrite CQRS Library

A lightweight, flexible, and feature-rich CQRS (Command Query Responsibility Segregation) framework built for .NET 9, designed to help you build scalable and maintainable applications. This library provides a clean, modern, and high-performance foundation for implementing the CQRS pattern with a focus on developer experience, performance, and enterprise-grade features.

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
- 📊 Internal command processing with:
  - Background service for processing
  - Configurable retry policies
  - Automatic cleanup of old commands
  - Command status tracking
  - Command statistics
  - Extensible storage providers

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

### Event Handling
- 🔔 Simple and lightweight event publishing
- 👥 Multiple event handlers support
- 🔄 Automatic handler registration
- 📝 Built-in logging
- ⚡ Asynchronous event processing
- 🛡️ Error handling and resilience

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

### 3. Event Handling Examples

#### Define an Event
```csharp
public class UserCreatedEvent : Event
{
    public string Username { get; }
    public string Email { get; }

    public UserCreatedEvent(string username, string email)
    {
        Username = username;
        Email = email;
    }
}
```

#### Implement Event Handler
```csharp
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public SendWelcomeEmailHandler(
        IEmailService emailService,
        ILogger<SendWelcomeEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            await _emailService.SendWelcomeEmailAsync(@event.Email, @event.Username);
            _logger.LogInformation(
                "Welcome email sent to user {Username} at {Email}", 
                @event.Username, 
                @event.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send welcome email to user {Username}", 
                @event.Username);
            throw;
        }
    }
}
```

#### Multiple Handlers for Same Event
```csharp
public class NotifyAdminHandler : IEventHandler<UserCreatedEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyAdminHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        await _notificationService.NotifyAdminAsync($"New user registered: {@event.Username}");
    }
}
```

#### Publishing Events
```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IEventBus eventBus)
    {
        _userRepository = userRepository;
        _eventBus = eventBus;
    }

    public async Task<IResult> HandleAsync(CreateUserCommand command)
    {
        var user = new User(command.Data.Username, command.Data.Email);
        await _userRepository.AddAsync(user);

        // Publish the event
        var @event = new UserCreatedEvent(user.Username, user.Email);
        await _eventBus.PublishAsync(@event);

        return CommandResult.Success();
    }
}
```

### 4. Register Event Handling

```csharp
services.AddBwCqrs(builder =>
{
    builder
        .AddValidation()
        .AddLogging()
        .AddErrorHandling()
        .AddRetry()
        .AddEventHandling(); // Enable event handling support
}, typeof(Program).Assembly);
```

### 5. Internal Commands

Internal commands allow you to schedule commands for later execution. They are processed by a background service and support retry policies and status tracking.

```csharp
// Define an internal command
public class SendEmailCommand : InternalCommand
{
    public string To { get; }
    public string Subject { get; }
    public string Body { get; }

    public SendEmailCommand(string to, string subject, string body)
    {
        To = to;
        Subject = subject;
        Body = body;
    }
}

// Configure internal commands with custom options
services.AddBwCqrs(builder =>
{
    builder
        .AddInternalCommands(options =>
        {
            options.MaxRetries = 3;
            options.RetryDelaySeconds = 60;
            options.ProcessingIntervalSeconds = 10;
            options.RetentionDays = 7;
        })
        .UseInMemory(); // Use in-memory storage
}, typeof(Program).Assembly);

// Use in your code
public class EmailService
{
    private readonly ICommandBus _commandBus;

    public EmailService(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    public async Task ScheduleEmailAsync(string to, string subject, string body)
    {
        var command = new SendEmailCommand(to, subject, body);
        await _commandBus.ScheduleAsync(command);
    }
}
```

### Storage Providers

The library supports different storage providers for internal commands. By default, it uses in-memory storage, but you can add support for other databases by installing additional packages:

```csharp
// Using in-memory storage (default)
builder.AddInternalCommands().UseInMemory();

// Using PostgreSQL (requires Bw.Cqrs.Commands.Postgres package)
builder.AddInternalCommands().UsePostgres(options => 
{
    options.ConnectionString = "your_connection_string";
});

// Using MongoDB (requires Bw.Cqrs.Commands.Mongo package)
builder.AddInternalCommands().UseMongo(options => 
{
    options.ConnectionString = "your_connection_string";
    options.DatabaseName = "your_database";
});
```

### Configuration Options

#### Internal Command Options

```csharp
builder.AddInternalCommands(options =>
{
    options.MaxRetries = 3;                    // Maximum number of retry attempts
    options.RetryDelaySeconds = 60;            // Delay between retry attempts
    options.ProcessingIntervalSeconds = 10;     // How often to check for pending commands
    options.RetentionDays = 7;                 // How long to keep processed commands
});
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

### Event Handling Best Practices

1. **Event Naming**:
   - Use past tense for event names (e.g., `UserCreated`, `OrderPlaced`)
   - Make names descriptive and meaningful
   - Follow the `[Entity][Action]Event` pattern

2. **Event Design**:
   - Keep events immutable
   - Include only necessary data
   - Consider versioning needs
   - Make events self-contained

3. **Event Handlers**:
   - Follow Single Responsibility Principle
   - Handle errors appropriately
   - Keep handlers independent
   - Add proper logging

4. **Event Publishing**:
   - Publish events after successful operations
   - Consider transactional boundaries
   - Handle publishing failures gracefully

5. **Testing**:
   - Test event handlers in isolation
   - Mock event bus in command handlers
   - Verify event publishing
   - Test error scenarios

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## PostgreSQL Integration

### PostgreSQL Outbox Pattern Support
The library provides built-in support for storing and processing internal commands using PostgreSQL as a reliable outbox storage.

### Installation

```bash
dotnet add package Bw.Cqrs.InternalCommand.Postgres
```

### Configuration

Add PostgreSQL support to your CQRS setup:

```csharp
services.AddBwCqrs(builder =>
{
    builder
        .AddValidation()
        .AddLogging()
        .AddInternalCommands()
        .UsePostgres(options =>
        {
            options.ConnectionString = "Host=localhost;Database=your_db;Username=your_user;Password=your_password";
            options.CommandTimeout = TimeSpan.FromSeconds(30);
            options.EnableDetailedErrors = true;
            options.EnableSensitiveDataLogging = false;
        });
}, typeof(Program).Assembly);
```

### Features

- 🗄️ Reliable storage of internal commands in PostgreSQL
- 📊 Command state tracking (Scheduled, Processing, Processed, Failed, Cancelled)
- 🔄 Automatic processing through background service
- 📝 Error logging and handling
- 🔍 Easy querying of command status
- ⚡ High performance with Entity Framework Core
- 🧪 Integration tests with Testcontainers
- 🔒 Configurable command timeout and retry policies
- 📈 Built-in command statistics and monitoring

### Database Schema

The PostgreSQL integration creates the following table:

```sql
CREATE TABLE internal_commands (
    id UUID PRIMARY KEY,
    type VARCHAR(500) NOT NULL,
    data TEXT NOT NULL,
    scheduled_on TIMESTAMP NOT NULL,
    processed_on TIMESTAMP NULL,
    retry_count INT NOT NULL DEFAULT 0,
    error VARCHAR(2000) NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    last_retry_at TIMESTAMP NULL
);
```

### Usage Example

```csharp
// Define an internal command that needs to be processed later
public class SendWelcomeEmailCommand : InternalCommand
{
    public string UserEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
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
                var pendingCommands = await _store.GetCommandsToExecuteAsync();
                
                foreach (var command in pendingCommands)
                {
                    try
                    {
                        await _store.UpdateStatusAsync(command.Id, InternalCommandStatus.Processing);
                        await _commandBus.DispatchAsync(command);
                        await _store.UpdateStatusAsync(command.Id, InternalCommandStatus.Processed);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process command {CommandId}", command.Id);
                        await _store.UpdateStatusAsync(command.Id, InternalCommandStatus.Failed, ex.Message);
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
    private readonly IInternalCommandStore _store;

    public CommandMonitoringService(IInternalCommandStore store)
    {
        _store = store;
    }

    public async Task<InternalCommandStats> GetCommandStatsAsync()
    {
        return await _store.GetStatsAsync();
    }
}
```

### Best Practices

1. **Command Idempotency**: Ensure your command handlers are idempotent as commands might be processed multiple times.
2. **Error Handling**: Always implement proper error handling in your command handlers.
3. **Monitoring**: Set up monitoring for failed commands and processing delays.
4. **Database Maintenance**: Implement a cleanup strategy for processed commands.
5. **Performance**: Index the `status` and `scheduled_on` columns for better query performance.
6. **Configuration**: Use appropriate timeouts and retry policies based on your application needs.
7. **Testing**: Write both unit tests and integration tests for your command handlers and storage implementation.
