# MiF.Mediator

MiF.Mediator is a lightweight and extensible .NET library that implements the Mediator design pattern. It provides a clean and decoupled way to handle queries and commands in your application, promoting separation of concerns and reducing dependencies between components.

## Features

- **Mediator Pattern**: Centralizes the handling of queries and commands to simplify communication between components.
- **Query and Command Handlers**: Built-in abstractions for handling queries (`IQueryHandler`) and commands (`ICommandHandler`).
- **Asynchronous Support**: Fully supports asynchronous operations using `Task` and `async/await`.
- **Extensibility**: Easily extendable to fit your application's specific needs.
- **Dependency Injection Friendly**: Designed to work seamlessly with dependency injection frameworks.

## Installation

To use MiF.Mediator in your project, add the library to your solution. Ensure your project targets `.NET 9` or higher.

## Getting Started

### 1. Define a Query or Command

A query or command is a simple class that implements the `IRequest<TResponse>` interface.

```csharp
public class GetUserQuery : IRequest<User> { public int UserId { get; set; } }
```

### 2. Create a Handler

Create a handler by implementing `IQueryHandler<TQuery, TResponse>` or `ICommandHandler<TCommand>`.

```csharp
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, User> { public Task<User> HandleAsync(GetUserQuery query, CancellationToken cancellationToken) { // Simulate fetching a user return Task.FromResult(new User { Id = query.UserId, Name = "John Doe" }); } }
```

### 3. Use the Mediator

The `Mediator` class acts as the central hub for dispatching queries and commands.

```csharp
var serviceFactory = new MyServiceFactory(); 

// Your implementation of IServiceFactory 
var mediator = new Mediator(serviceFactory);

var query = new GetUserQuery { UserId = 1 }; 
var user = await mediator.SendMessageAsync(query);

Console.WriteLine($"User: {user.Name}");
```

### 4. Dependency Injection

Integrate the library with your preferred DI container by registering handlers and the `Mediator` class.

## Example Use Cases

- **CQRS (Command Query Responsibility Segregation)**: Separate read and write operations in your application.
- **Decoupled Architecture**: Reduce dependencies between components by centralizing communication.
- **Asynchronous Processing**: Handle long-running operations or external API calls efficiently.
- **Event Sourcing**: Use the mediator to handle events and commands in an event-sourced architecture.
- **Unit Testing**: Easily mock the mediator and handlers for unit tests.
- **Validation**: Implement validation logic in handlers to ensure data integrity before processing commands.

## Contributing

Contributions are welcome! Feel free to open issues or submit pull requests to improve the library.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
