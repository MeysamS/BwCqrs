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

### PostgreSQL Integration

The library provides built-in support for storing and processing internal commands using PostgreSQL as a reliable outbox storage.

#### Installation

```bash
dotnet add package Bw.Cqrs.InternalCommands.Postgres
```

#### Database Setup

1. Create a `Scripts` folder in your infrastructure project
2. Add the following SQL script as `CreateInternalCommandsTable.sql`:

```sql
-- Create internal_commands table
CREATE TABLE IF NOT EXISTS internal_commands (
    "Id" UUID PRIMARY KEY,
    "Type" VARCHAR(500) NOT NULL,
    "Data" TEXT NOT NULL,
    "ScheduledOn" TIMESTAMP NOT NULL,
    "ProcessedOn" TIMESTAMP,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "Error" VARCHAR(2000),
    "Status" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "LastRetryAt" TIMESTAMP
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_internal_commands_status ON internal_commands("Status");
CREATE INDEX IF NOT EXISTS idx_internal_commands_scheduled_on ON internal_commands("ScheduledOn");
CREATE INDEX IF NOT EXISTS idx_internal_commands_processed_on ON internal_commands("ProcessedOn");
```

3. Add the script to your project file:

```xml
<ItemGroup>
    <Content Include="Scripts\CreateInternalCommandsTable.sql">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

4. Execute the script during application startup:

```csharp
// In Program.cs or Startup.cs
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<YourDbContext>();
    
    // Execute internal commands table creation script
    var scriptPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "CreateInternalCommandsTable.sql");
    if (File.Exists(scriptPath))
    {
        var script = File.ReadAllText(scriptPath);
        dbContext.Database.ExecuteSqlRaw(script);
    }
}
```

#### Configuration

Add PostgreSQL support to your CQRS setup:

```csharp
services.AddBwCqrs(builder =>
{
    builder
        .AddValidation()
        .AddLogging()
        .AddErrorHandling()
        .AddRetry(maxRetries: 3, delayMilliseconds: 1000)
        .AddEventHandling()
        .AddInternalCommands(options =>
        {
            options.MaxRetries = 3;
            options.RetryDelaySeconds = 60;
            options.ProcessingIntervalSeconds = 10;
            options.RetentionDays = 7;
        })
        .UsePostgres(options =>
        {
            options.ConnectionString = connectionString;
            options.CommandTimeout = TimeSpan.FromSeconds(30);
            options.EnableDetailedErrors = true;
            options.EnableSensitiveDataLogging = false;
        });
}, typeof(Program).Assembly);
```

#### Important Notes

1. The `internal_commands` table must be created in your database before using the package
2. Make sure your connection string has the necessary permissions to create tables and indexes
3. The package uses the same connection string as your main application database
4. Internal commands are processed asynchronously by a background service
5. **Column Naming**: PostgreSQL column names are case-sensitive when quoted. The `InternalCommandProcessor` expects column names with specific casing (e.g., "Id", "Type", "Data"). Always use the exact column names as shown in the SQL script to avoid errors like `column i.Id does not exist`.

#### Troubleshooting

If you encounter any issues:

1. Check if the `internal_commands` table exists in your database
2. Verify that your connection string is correct
3. Ensure all required indexes are created
4. Check the application logs for any error messages
5. If you see errors like `column i.Id does not exist`, make sure your column names match exactly with the ones in the SQL script (including casing and quotes)

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

## Sample Project: OrderManagement

The library includes a sample project called `OrderManagement` that demonstrates how to use Bw.Cqrs in a real-world application. This project implements a simple order management system with the following features:

### Project Structure

```
OrderManagement/
├── OrderManagement.API/              # API layer with controllers and configuration
├── OrderManagement.Application/      # Application layer with commands, queries, and handlers
├── OrderManagement.Domain/           # Domain layer with entities and interfaces
├── OrderManagement.Infrastructure/   # Infrastructure layer with repositories and persistence
└── OrderManagement.IntegrationTests/ # Integration tests
```

### Key Components

1. **Domain Layer**:
   - `Order` and `OrderItem` entities
   - Repository interfaces (`IOrderRepository`)

2. **Application Layer**:
   - Commands: `CreateOrderCommand`, `UpdateOrderCommand`, `DeleteOrderCommand`
   - Queries: `GetOrderByIdQuery`, `GetAllOrdersQuery`
   - Command and Query handlers
   - Validators using FluentValidation

3. **Infrastructure Layer**:
   - Entity Framework Core implementation of repositories
   - PostgreSQL database configuration
   - Internal commands setup with PostgreSQL storage

4. **API Layer**:
   - RESTful controllers for orders
   - CQRS configuration with validation, logging, and error handling
   - Swagger documentation

### CQRS Implementation

The sample project demonstrates:

1. **Command Handling**:
   ```csharp
   // CreateOrderCommand
   public class CreateOrderCommand : CreateCommand<CreateOrderRequest>
   {
       public CreateOrderCommand(CreateOrderRequest data) : base(data)
       {
       }
   }

   // CreateOrderCommandHandler
   public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
   {
       private readonly IOrderRepository _orderRepository;
       private readonly IEventBus _eventBus;

       public CreateOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
       {
           _orderRepository = orderRepository;
           _eventBus = eventBus;
       }

       public async Task<IResult> HandleAsync(CreateOrderCommand command)
       {
           var order = new Order(command.Data.CustomerName);
           foreach (var item in command.Data.Items)
           {
               order.AddItem(item.ProductName, item.Quantity, item.UnitPrice);
           }
           
           await _orderRepository.AddAsync(order);
           
           // Publish event
           await _eventBus.PublishAsync(new OrderCreatedEvent(order.Id, order.CustomerName));
           
           return CommandResult.Success(order.Id);
       }
   }
   ```

2. **Query Handling**:
   ```csharp
   // GetOrderByIdQuery
   public class GetOrderByIdQuery : IQuery<OrderDto?>
   {
       public Guid OrderId { get; }

       public GetOrderByIdQuery(Guid orderId)
       {
           OrderId = orderId;
       }
   }

   // GetOrderByIdQueryHandler
   public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto?>
   {
       private readonly IOrderRepository _orderRepository;

       public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
       {
           _orderRepository = orderRepository;
       }

       public async Task<OrderDto?> HandleAsync(GetOrderByIdQuery query)
       {
           var order = await _orderRepository.GetByIdAsync(query.OrderId);
           return order == null ? null : new OrderDto(order);
       }
   }
   ```

3. **Validation**:
   ```csharp
   // CreateOrderCommandValidator
   public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
   {
       public CreateOrderCommandValidator()
       {
           RuleFor(x => x.Data.CustomerName).NotEmpty().MaximumLength(100);
           RuleFor(x => x.Data.Items).NotEmpty().WithMessage("Order must have at least one item");
           
           RuleForEach(x => x.Data.Items).ChildRules(item =>
           {
               item.RuleFor(x => x.ProductName).NotEmpty().MaximumLength(100);
               item.RuleFor(x => x.Quantity).GreaterThan(0);
               item.RuleFor(x => x.UnitPrice).GreaterThan(0);
           });
       }
   }
   ```

4. **Event Handling**:
   ```csharp
   // OrderCreatedEvent
   public class OrderCreatedEvent : Event
   {
       public Guid OrderId { get; }
       public string CustomerName { get; }

       public OrderCreatedEvent(Guid orderId, string customerName)
       {
           OrderId = orderId;
           CustomerName = customerName;
       }
   }

   // OrderCreatedEventHandler
   public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
   {
       private readonly ILogger<OrderCreatedEventHandler> _logger;

       public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
       {
           _logger = logger;
       }

       public Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Order {OrderId} created for customer {CustomerName}", 
               @event.OrderId, @event.CustomerName);
           return Task.CompletedTask;
       }
   }
   ```

5. **Internal Commands with PostgreSQL**:
   ```csharp
   // In Program.cs
   services.AddBwCqrs(builder =>
   {
       builder
           .AddValidation()
           .AddLogging()
           .AddErrorHandling()
           .AddRetry(maxRetries: 3, delayMilliseconds: 1000)
           .AddEventHandling()
           .AddInternalCommands(options =>
           {
               options.MaxRetries = 3;
               options.RetryDelaySeconds = 60;
               options.ProcessingIntervalSeconds = 10;
               options.RetentionDays = 7;
           })
           .UsePostgres(options =>
           {
               options.ConnectionString = connectionString;
           });
   }, typeof(OrderManagementApplicationInfo).Assembly);
   ```

### Running the Sample

1. Clone the repository
2. Navigate to the OrderManagement.API directory
3. Update the connection string in `appsettings.json`
4. Run the application:
   ```bash
   dotnet run
   ```
5. Access the Swagger UI at `https://localhost:5001/swagger`

This sample project provides a complete example of how to implement CQRS using the BitWrite CQRS Library in a real-world application.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
