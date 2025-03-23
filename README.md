# BitWrite CQRS Library

A lightweight, flexible, and feature-rich CQRS (Command Query Responsibility Segregation) implementation for .NET applications. This library provides a clean and maintainable way to separate read and write operations in your application.

## Features

### Command Handling
- ✨ Strongly-typed command handling
- 🔄 Command pipeline behaviors (middleware)
- ✅ Built-in validation using FluentValidation
- 📝 Comprehensive logging support
- 🔄 Delayed command processing (Outbox pattern)
- 🎯 Command result handling
- 🏭 Dependency injection support

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
services.AddCqrs(options =>
{
    options.AddValidation()  // Add validation behavior
           .AddLogging();    // Add logging behavior
}, typeof(Program).Assembly);
```

### 2. Command Handling Example

```csharp
// Define a command
public class CreateUserCommand : CommandBase
{
    public string Username { get; set; }
    public string Email { get; set; }
}

// Define command validation
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

// Implement command handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IResult> HandleAsync(CreateUserCommand command)
    {
        var user = new User(command.Username, command.Email);
        await _userRepository.AddAsync(user);
        return CommandResult.Success();
    }
}

// Use in your application
public class UserService
{
    private readonly ICommandBus _commandBus;

    public UserService(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    public async Task CreateUserAsync(string username, string email)
    {
        var command = new CreateUserCommand 
        { 
            Username = username, 
            Email = email 
        };
        
        await _commandBus.DispatchAsync(command);
    }
}
```

### 3. Query Handling Example

```csharp
// Define a query
public class GetUserByIdQuery : IQuery<UserDto>
{
    public Guid UserId { get; set; }
}

// Implement query handler
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> HandleAsync(GetUserByIdQuery query)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId);
        return user?.ToDto();
    }
}

// Use in your application
public class UserService
{
    private readonly IQueryBus _queryBus;

    public UserService(IQueryBus queryBus)
    {
        _queryBus = queryBus;
    }

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        return await _queryBus.SendAsync(query);
    }
}
```

### 4. Delayed Command Processing (Outbox Pattern)

```csharp
// Define an internal command
public class SendWelcomeEmailCommand : InternalCommandBase
{
    public string UserEmail { get; set; }
    public string Username { get; set; }
}

// Implement handler
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

// Schedule command for later processing
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

        var command = new SendWelcomeEmailCommand
        {
            Username = username,
            UserEmail = email
        };

        await _commandBus.ScheduleAsync(command);
    }
}
```

## Advanced Features

### Custom Pipeline Behaviors

```csharp
public class TransactionBehavior<TCommand> : ICommandPipelineBehavior<TCommand, IResult>
    where TCommand : ICommand
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken,
        CommandHandlerDelegate<IResult> next)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await next();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

// Register in startup
services.AddCqrs(options =>
{
    options.AddCustomBehavior<TransactionBehavior<CreateUserCommand>, CreateUserCommand, IResult>();
});
```

## Best Practices

1. **Command Naming**: Use imperative verb phrases (e.g., `CreateUser`, `UpdateProfile`)
2. **Query Naming**: Use noun phrases (e.g., `UserById`, `ActiveUsers`)
3. **Validation**: Always validate commands before processing
4. **Error Handling**: Use `CommandResult` for consistent error handling
5. **Logging**: Enable logging behavior for better debugging
6. **Testing**: Write unit tests for command and query handlers

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 