# UsecatR

![CI](https://github.com/FcoJunior/UsecatR/actions/workflows/ci.yml/badge.svg)
![Coverage](https://img.shields.io/codecov/c/github/FcoJunior/UsecatR)
![NuGet](https://img.shields.io/nuget/v/UsecatR)
![NuGet Downloads](https://img.shields.io/nuget/dt/UsecatR)
![License](https://img.shields.io/github/license/FcoJunior/UsecatR)

**UsecatR** is a lightweight **Use Case (Interactor) execution pipeline** for .NET, designed to fit naturally into **Clean Architecture** applications.

It provides a simple and explicit way to execute Use Cases, with support for **pipeline behaviors** such as logging, validation, transactions, caching, and more — without heavy frameworks or magic conventions.

Think of UsecatR as a **clean, minimal alternative to MediatR**, focused exclusively on the **Application Layer**.

---

## Installation

UsecatR is distributed as a **single NuGet package**.

```bash
dotnet add package UsecatR
```

---

## Core Concepts

| Concept | Description |
|------|-------------|
| `IUseCaseRequest<TResult>` | Input model of a Use Case |
| `IUseCaseHandler<TRequest, TResult>` | Executes the Use Case |
| `IUseCaseBehavior<TRequest, TResult>` | Cross-cutting pipeline behavior |
| `IUsecatR` | Entry point used to execute Use Cases |

---

## Basic Usage

### 1. Define a Use Case request

```csharp
using UsecatR.Abstractions;

public sealed record CreateCustomer(
    string Name,
    string Email
) : IUseCaseRequest<Guid>;
```

---

### 2. Implement the Use Case handler

```csharp
using UsecatR.Abstractions;

public sealed class CreateCustomerHandler
    : IUseCaseHandler<CreateCustomer, Guid>
{
    public Task<Guid> HandleAsync(CreateCustomer request, CancellationToken ct)
    {
        // Application logic goes here
        return Task.FromResult(Guid.NewGuid());
    }
}
```

---

### 3. Register UsecatR and handlers

```csharp
using UsecatR.DependencyInjection;

builder.Services.AddUsecatR(
    typeof(CreateCustomerHandler).Assembly
);
```

---

### 4. Execute the Use Case

```csharp
using UsecatR.Abstractions;

public sealed class CustomersController : ControllerBase
{
    private readonly IUsecatR _UsecatR;

    public CustomersController(IUsecatR UsecatR)
    {
        _UsecatR = UsecatR;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomer input, CancellationToken ct)
    {
        var id = await _UsecatR.ExecuteAsync<CreateCustomer, Guid>(input, ct);
        return Ok(id);
    }
}
```

---

## Pipeline Behaviors

Pipeline behaviors allow you to add cross-cutting concerns around Use Case execution.

### Behavior contract

```csharp
public interface IUseCaseBehavior<in TRequest, TResult>
    where TRequest : IUseCaseRequest<TResult>
{
    Task<TResult> HandleAsync(
        TRequest request,
        UseCaseHandlerDelegate<TResult> next,
        CancellationToken ct);
}
```

---

### Example: Logging behavior

```csharp
using UsecatR.Abstractions;

public sealed class LoggingBehavior<TRequest, TResult>
    : IUseCaseBehavior<TRequest, TResult>
    where TRequest : IUseCaseRequest<TResult>
{
    public async Task<TResult> HandleAsync(
        TRequest request,
        UseCaseHandlerDelegate<TResult> next,
        CancellationToken ct)
    {
        Console.WriteLine($"Starting {typeof(TRequest).Name}");
        var result = await next();
        Console.WriteLine($"Finished {typeof(TRequest).Name}");
        return result;
    }
}
```

---

## Clean Architecture Alignment

UsecatR is designed to live in the **Application Layer**.

---

## License

MIT
