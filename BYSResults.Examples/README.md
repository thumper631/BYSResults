# BYSResults Examples

This project contains working examples and reference templates demonstrating how to use the BYSResults library.

## Running the Examples

To run the working examples:

```bash
cd BYSResults.Examples
dotnet run
```

## Working Examples

### Simple Examples (`SimpleExamples.cs`)

Demonstrates core BYSResults features with runnable code:

1. **Basic Success and Failure** - Creating and checking Result states
2. **Validation with Ensure** - Inline validation with error messages
3. **Chaining with Map and Bind** - Transforming and chaining operations
4. **Pattern Matching** - Handling success/failure cases elegantly
5. **Error Handling** - Using TapOnFailure for logging
6. **Combining Results** - Aggregating multiple validation results
7. **Fallback with OrElse** - Providing alternative values on failure
8. **Try/Catch Wrapper** - Safely wrapping exception-throwing code

## Reference Templates

The `Templates/` folder contains example patterns for common scenarios. These are reference implementations that demonstrate usage patterns but require adaptation for your specific use case:

- **WebApiExamples.cs** - Converting Result to HTTP responses, CRUD operations
- **DatabaseExamples.cs** - Repository pattern, transactions, batch operations
- **ValidationExamples.cs** - Form validation, error aggregation, business rules
- **AsyncExamples.cs** - External APIs, parallel operations, retry logic
- **ChainingExamples.cs** - Railway-oriented programming, complex workflows

**Note:** Templates in the `Templates/` folder are excluded from the build and serve as reference patterns. You can copy and adapt them for your specific needs.

## Key Concepts Demonstrated

### Railway-Oriented Programming
Examples show how to chain operations where success flows through the pipeline and failures exit early:

```csharp
return Result<Order>.Success(order)
    .Ensure(o => o.Amount > 0, "Amount must be positive")
    .Bind(ValidateInventory)
    .Bind(ProcessPayment)
    .Tap(SendConfirmation);
```

### Error Aggregation
Multiple validations can be combined to collect all errors:

```csharp
var combined = Result.Combine(
    ValidateName(name),
    ValidateEmail(email),
    ValidateAge(age)
);
```

### Async Composition
Async operations can be chained elegantly:

```csharp
return await Result<UserId>.Success(userId)
    .MapAsync(GetUserAsync)
    .BindAsync(ValidateUserAsync)
    .TapAsync(SendNotificationAsync);
```

### Error Recovery
Fallback strategies can be implemented:

```csharp
return await LoadFromDatabase()
    .OrElse(async () => await LoadFromCache())
    .OrElse(() => GetDefault());
```

## Learning Path

1. **Start with Web API Examples** - See practical HTTP response patterns
2. **Move to Validation Examples** - Understand error collection
3. **Study Database Examples** - Learn transaction patterns
4. **Explore Async Examples** - Master async composition
5. **Deep dive into Chaining Examples** - Understand railway-oriented programming

## Common Patterns

### Pattern: Validate-Transform-Process
```csharp
Result<Output>.Success(input)
    .Ensure(validation rules...)
    .Map(transform)
    .Bind(process)
    .Tap(side effects)
```

### Pattern: Try-Catch with Result
```csharp
var result = Result<Data>.Try(() => RiskyOperation());
// or async
var result = await Result<Data>.TryAsync(async () => await RiskyAsync());
```

### Pattern: Multiple Validations
```csharp
var errors = new List<Error>();
// collect all errors
if (errors.Any())
    return Result.Failure(errors);
```

## Additional Resources

- [Main Documentation](../README.md)
- [API Reference](../README.md#api-reference)
- [NuGet Package](https://www.nuget.org/packages/BYSResults)
- [GitHub Repository](https://github.com/Thumper631/BYSResults)
