# BYSResults Example Templates

This folder contains reference templates showing common usage patterns for BYSResults. These files are **excluded from the build** and serve as starting points for your own implementations.

## About These Templates

These templates demonstrate **conceptual patterns** for using BYSResults in real-world scenarios. They are intentionally more complex than working examples to show how Result types can be used in production-like code.

**Important:** These templates may not compile as-is. They are meant to be **copied and adapted** to your specific use case, dependencies, and architecture.

## Available Templates

### WebApiExamples.cs
Shows how to integrate Result types with Web API controllers:
- Converting `Result<T>` to HTTP status codes and responses
- Handling CRUD operations with validation
- Authorization checks with Results
- Error mapping to appropriate HTTP responses

**Usage Pattern:**
```csharp
public IActionResult GetUser(int id)
{
    var result = userService.GetUserById(id);

    return result.Match(
        onSuccess: user => Ok(user),
        onFailure: errors => NotFound(new { errors })
    );
}
```

### DatabaseExamples.cs
Demonstrates Result usage with database operations:
- Repository pattern with error handling
- Async database operations with `MapAsync` and `BindAsync`
- Transaction management
- Batch operations with partial failure handling
- Caching strategies with fallback
- Audit trails and soft deletes

**Usage Pattern:**
```csharp
public async Task<Result<Customer>> CreateCustomerAsync(CustomerDto dto)
{
    var result = await Result<CustomerDto>.Success(dto)
        .Ensure(d => !string.IsNullOrEmpty(d.Email), "Email required")
        .MapAsync(async d => await MapToEntity(d))
        .BindAsync(async entity => await repository.SaveAsync(entity));

    return result;
}
```

### ValidationExamples.cs
Covers comprehensive validation patterns:
- Inline validation with `Ensure`
- Error aggregation - collecting all errors at once
- Multi-field validation
- Business rule validation
- Cross-field validation
- Nested object validation

**Usage Pattern:**
```csharp
public Result ValidateForm(FormData form)
{
    var errors = new List<Error>();

    if (string.IsNullOrEmpty(form.Name))
        errors.Add(new Error("NAME_REQUIRED", "Name is required"));

    if (form.Age < 18)
        errors.Add(new Error("AGE_RESTRICTION", "Must be 18+"));

    return errors.Any()
        ? Result.Failure(errors)
        : Result.Success();
}
```

### AsyncExamples.cs
Shows asynchronous patterns and external service integration:
- External API calls with error handling
- Parallel async operations
- Sequential pipelines with early exit
- Retry logic with exponential backoff
- Timeout handling
- Fallback chains for resilience

**Usage Pattern:**
```csharp
public async Task<Result<Data>> GetDataAsync(string id)
{
    var result = await Result<string>.TryAsync(
        async () => await externalApi.FetchAsync(id)
    );

    return await result
        .MapAsync(async json => await ParseAsync(json));
}
```

### ChainingExamples.cs
Demonstrates railway-oriented programming:
- Simple operation pipelines
- Complex multi-stage workflows
- Conditional logic in chains
- Error recovery with `OnSuccess` and `OnFailure`
- Result aggregation from multiple sources
- Approval workflows with multiple levels

**Usage Pattern:**
```csharp
public Result<Order> ProcessOrder(OrderRequest request)
{
    return Result<OrderRequest>.Success(request)
        .Ensure(r => r.Amount > 0, "Invalid amount")
        .Bind(ValidateInventory)
        .Bind(ApplyPricing)
        .Bind(ProcessPayment)
        .Tap(order => NotifyCustomer(order));
}
```

## How to Use These Templates

1. **Choose a template** that matches your scenario
2. **Copy the relevant code** to your project
3. **Adapt the models and types** to match your domain
4. **Replace mock implementations** with your actual services
5. **Adjust error handling** to match your requirements
6. **Test thoroughly** with your actual data and scenarios

## Common Adaptations Needed

When using these templates, you'll typically need to:

- Replace mock services with your actual implementations
- Adjust error codes and messages for your domain
- Add proper async/await handling where needed
- Integrate with your dependency injection container
- Add logging appropriate to your application
- Handle database transactions per your ORM
- Map to your specific DTOs and entities
- Implement your authorization logic

## Working Example

For a complete, runnable example, see **`SimpleExamples.cs`** in the parent directory. That file demonstrates all core BYSResults features with working, compilable code.

## Additional Resources

- [BYSResults README](../../README.md) - Full API documentation
- [Examples README](../README.md) - Overview of all examples
- [NuGet Package](https://www.nuget.org/packages/BYSResults) - Install BYSResults
