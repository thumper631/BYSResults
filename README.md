# BYSResults

[![NuGet version](https://img.shields.io/nuget/v/BYSResults.svg)](https://www.nuget.org/packages/BYSResults)  
[![Build Status](https://github.com/Thumper631/BYSResults/actions/workflows/ci.yml/badge.svg)](https://github.com/Thumper631/BYSResults/actions)  
[![Coverage Status](https://coveralls.io/repos/github/Thumper631/BYSResults/badge.svg?branch=main)](https://coveralls.io/github/Thumper631/BYSResults?branch=main)  
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Lightweight result types for explicit success/failure handling in .NET applications.

---

## Table of Contents

1. [Features](#features)
2. [Installation](#installation)
3. [Quick Start / Usage Examples](#quick-start--usage-examples)
4. [Real-World Examples](#real-world-examples)
5. [API Reference](#api-reference)
   - [Result](#result)
   - [Result&lt;T&gt;](#resultt)
   - [Error](#error)
6. [Advanced Usage](#advanced-usage)
7. [Revision History](#revision-history)
8. [Contributing](#contributing)
9. [License](#license)
10. [Authors & Acknowledgments](#authors--acknowledgments)
11. [Links](#links)  

---

## Features

- **No exceptions for control flow** — all outcomes are explicit
- **Fluent chaining** via `Bind`, `Map`, `MapAsync`, `BindAsync`, etc.
- **Pattern matching** with `Match` for elegant error handling
- **Exception safety** with `Try` and `TryAsync` factory methods
- **Value extraction** with `GetValueOr` and `OrElse` for fallback handling
- **Side effects** via `Tap`, `TapAsync`, `OnSuccess`, `OnFailure` without breaking chains
- **Validation** with `Ensure` for inline condition checking
- **Async support** for modern .NET applications
- **Easy combination** with `Result.Combine(...)`
- **Error aggregation** and inspection (`.Errors`, `.FirstError`)
- **Generic and non-generic** variants (`Result` vs. `Result<T>`)  

---

## Installation

Install via **.NET CLI**:

```bash
dotnet add package BYSResults
```

Or via **Package Manager Console**:

```powershell
Install-Package BYSResults
```

---

## Quick Start / Usage Examples

```csharp
using BYSResults;

// Simple success/failure without a value
var r1 = Result.Success();
var r2 = Result.Failure("E001", "Something went wrong");

if (r2.IsFailure)
{
    Console.WriteLine(r2.FirstError?.Message);
}

// Generic result with a value
var r3 = Result<int>.Success(42);
var r4 = Result<string>.Failure("Missing data");

if (r3.IsSuccess)
{
    Console.WriteLine($"Value is {r3.Value}");
}

// Inspect errors
foreach (var err in r4.Errors)
{
    Console.WriteLine(err);
}
```

---

## Real-World Examples

For comprehensive, runnable examples demonstrating real-world usage patterns, see the [**BYSResults.Examples**](BYSResults.Examples/) project.

### Example Categories

The examples project includes:

1. **[Web API Examples](BYSResults.Examples/WebApiExamples.cs)** - Converting Result to HTTP responses, CRUD operations
2. **[Database Examples](BYSResults.Examples/DatabaseExamples.cs)** - Repository pattern, transactions, batch operations
3. **[Validation Examples](BYSResults.Examples/ValidationExamples.cs)** - Form validation, error aggregation, business rules
4. **[Async Examples](BYSResults.Examples/AsyncExamples.cs)** - External APIs, parallel operations, retry logic
5. **[Chaining Examples](BYSResults.Examples/ChainingExamples.cs)** - Railway-oriented programming, complex workflows

### Web API Integration

```csharp
// Converting Result to HTTP-style responses
public ApiResponse<User> GetUser(int id)
{
    var result = userService.GetUserById(id);

    return result.Match(
        onSuccess: user => new ApiResponse<User>
        {
            StatusCode = 200,
            Data = user
        },
        onFailure: errors => new ApiResponse<User>
        {
            StatusCode = 404,
            Errors = errors.Select(e => e.Message).ToList()
        }
    );
}
```

### Database Operations

```csharp
// Repository pattern with validation and side effects
public async Task<Result<Customer>> CreateCustomerAsync(CustomerDto dto)
{
    return await Result<CustomerDto>.Success(dto)
        .Ensure(d => !string.IsNullOrEmpty(d.Email), "Email is required")
        .Ensure(d => d.Email.Contains("@"), "Valid email is required")
        .Ensure(d => d.Age >= 18, new Error("AGE_RESTRICTION", "Must be 18 or older"))
        .MapAsync(async d => new Customer
        {
            Name = d.Name,
            Email = d.Email,
            Age = d.Age,
            CreatedAt = DateTime.UtcNow
        })
        .BindAsync(async customer => await repository.SaveAsync(customer))
        .TapAsync(async customer => await SendWelcomeEmailAsync(customer));
}
```

### Validation Patterns

```csharp
// Collecting all validation errors at once
public Result<RegistrationForm> ValidateRegistration(RegistrationForm form)
{
    var errors = new List<Error>();

    if (string.IsNullOrWhiteSpace(form.Name))
        errors.Add(new Error("NAME_REQUIRED", "Name is required"));

    if (string.IsNullOrWhiteSpace(form.Email))
        errors.Add(new Error("EMAIL_REQUIRED", "Email is required"));
    else if (!form.Email.Contains("@"))
        errors.Add(new Error("EMAIL_INVALID", "Email must be valid"));

    if (form.Password.Length < 8)
        errors.Add(new Error("PASSWORD_TOO_SHORT", "Password must be 8+ characters"));

    return errors.Any()
        ? Result<RegistrationForm>.Failure(errors)
        : Result<RegistrationForm>.Success(form);
}
```

### Railway-Oriented Programming

```csharp
// Complex multi-stage workflow with early exit on failure
public async Task<Result<ProcessedOrder>> ProcessOrderAsync(OrderRequest request)
{
    return await Result<OrderRequest>.Success(request)
        .BindAsync(async r => await ValidateOrderAsync(r))
        .BindAsync(async r => await CheckInventoryAsync(r))
        .BindAsync(async r => await CalculatePricingAsync(r))
        .BindAsync(async r => await ProcessPaymentAsync(r))
        .BindAsync(async r => await CreateShipmentAsync(r))
        .TapAsync(async o => await SendConfirmationEmailAsync(o))
        .TapAsync(async o => await LogOrderAsync(o));
}
```

### External API Integration

```csharp
// Handling external API calls with error handling
public async Task<Result<WeatherData>> GetWeatherAsync(string city)
{
    return await Result<string>.Success(city)
        .Ensure(c => !string.IsNullOrWhiteSpace(c), "City is required")
        .MapAsync(async c => await FetchWeatherJsonAsync(c))
        .MapAsync(async json => await DeserializeWeatherAsync(json))
        .Ensure(data => data != null, "Failed to parse weather data")
        .Ensure(data => data.Temperature > -100 && data.Temperature < 100,
            "Invalid temperature reading");
}
```

### Fallback Chains

```csharp
// Try primary source, fall back to cache, then default
public async Task<Result<Configuration>> LoadConfigurationAsync()
{
    return await LoadFromRemoteAsync()
        .OrElse(async () => await LoadFromDatabaseAsync())
        .OrElse(async () => await LoadFromFileAsync())
        .OrElse(() => Result<Configuration>.Success(GetDefaultConfiguration()));
}
```

### Retry Logic

```csharp
// Automatic retry with exponential backoff
public async Task<Result<T>> RetryAsync<T>(
    Func<Task<Result<T>>> operation,
    int maxRetries = 3)
{
    Result<T>? lastResult = null;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        lastResult = await operation();

        if (lastResult.IsSuccess)
            return lastResult;

        Console.WriteLine($"Attempt {attempt} failed. Retrying...");
        await Task.Delay(TimeSpan.FromSeconds(attempt)); // Exponential backoff
    }

    return lastResult!.AddError(
        new Error("MAX_RETRIES", $"Failed after {maxRetries} attempts"));
}
```

### Conditional Processing

```csharp
// Multi-level approval workflow
public Result<ApprovalResult> ProcessApprovalWorkflow(ApprovalRequest request)
{
    return Result<ApprovalRequest>.Success(request)
        .Ensure(r => r.Amount > 0, "Amount must be positive")
        .Bind(r => r.Amount < 1000
            ? Result<ApprovalRequest>.Success(r)  // Auto-approve
            : GetManagerApproval(r))
        .Bind(r => r.Amount < 5000
            ? Result<ApprovalRequest>.Success(r)
            : GetDirectorApproval(r))
        .Map(r => new ApprovalResult
        {
            RequestId = r.RequestId,
            Approved = true
        });
}
```

### Running the Examples

```bash
cd BYSResults.Examples
dotnet run
```

See the [Examples README](BYSResults.Examples/README.md) for detailed explanations of each pattern.

---

## API Reference

### Result

#### Core Properties & Status
* **bool IsSuccess** - True if operation succeeded.
* **bool IsFailure** - True if operation failed.
* **IReadOnlyList<Error> Errors** - List of all errors (empty if success).
* **Error? FirstError** - Shortcut to the first error, or null if none.

#### Factory Methods
* **static Result Success()** - Create a successful `Result`.
* **static Result Failure(...)** - Create a failure `Result` (overloads: `Error`, `IEnumerable<Error>`, `string`, `(code, message)`).
* **static Result Try(Action)** - Execute an action and return success, or failure if exception is thrown.
* **static Result Combine(...)** - Combine multiple `Result` instances into one, aggregating errors.

#### Error Management
* **Result AddError(Error)** - Add an error to an existing `Result`.
* **Result AddError(Exception)** - Add an exception as an error to an existing `Result`.
* **Result AddErrors(...)** - Add multiple errors to an existing `Result`.

#### Control Flow & Pattern Matching
* **void Match(Action onSuccess, Action<IReadOnlyList<Error>> onFailure)** - Execute one of two actions based on result state.
* **TReturn Match<TReturn>(...)** - Execute one of two functions and return a value based on result state.
* **Result OnSuccess(Func<Result>)** - Execute a function and return its result if successful.
* **Result OnFailure(Func<IReadOnlyList<Error>, Result>)** - Execute a function and return its result if failed.

#### Side Effects & Debugging
* **Result Tap(Action)** - Execute an action without modifying the result (useful for logging).
* **Result TapOnSuccess(Action)** - Execute an action only if successful.
* **Result TapOnFailure(Action<IReadOnlyList<Error>>)** - Execute an action only if failed.

#### Validation
* **Result Ensure(Func<bool>, Error)** - Validate a condition, adding an error if false.
* **Result Ensure(Func<bool>, string)** - Validate a condition, adding an error message if false.

---

### Result<T>

#### Core Properties
* **T? Value** - The value if successful (default if failure).

#### Factory Methods
* **static Result<T> Success(T value)** - Create a successful `Result<T>` with the given value.
* **static Result<T> Failure(...)** - Create a failure `Result<T>` (same overloads as `Result`).
* **static Result<T> Try(Func<T>)** - Execute a function and return its value, or failure if exception is thrown.

> **Note:** `Result<T>.Combine()` was removed in v1.2.1. Use `Result.Combine(...)` from the base class instead, which accepts both `Result` and `Result<T>` instances.

#### Functional Composition
* **Result<TNext> Map(Func\<T, TNext>)** - Transform the value on success, propagate errors on failure.
* **Result<TNext> Bind(Func\<T, Result<TNext>>)** - Chain operations that return `Result<TNext>`.

#### Async Operations
* **Task<Result<TNext>> MapAsync(Func\<T, Task<TNext>>)** - Asynchronously transform the value.
* **Task<Result<TNext>> BindAsync(Func\<T, Task<Result<TNext>>>)** - Asynchronously chain operations.
* **static Task<Result<T>> TryAsync(Func<Task<T>>)** - Execute async function, catching exceptions.
* **Task<Result<T>> TapAsync(Func\<T, Task>)** - Execute async side effect.

#### Value Extraction & Fallbacks
* **T GetValueOr(T defaultValue)** - Get value if successful, otherwise return default.
* **T GetValueOr(Func<T>)** - Get value if successful, otherwise call function for default.
* **Result<T> OrElse(Result<T>)** - Return this if successful, otherwise return alternative.
* **Result<T> OrElse(Func<Result<T>>)** - Return this if successful, otherwise call function for alternative.

#### Error Management
* **Result<T> WithValue(T)** - Set the `.Value` on an existing successful result.
* **new Result<T> AddError(Error)** - Add an error (returns `Result<T>` for chaining).
* **new Result<T> AddErrors(...)** - Add multiple errors (returns `Result<T>`).

#### Control Flow & Pattern Matching
* **void Match(Action<T>, Action<IReadOnlyList<Error>>)** - Execute one of two actions based on result state.
* **TReturn Match<TReturn>(Func\<T, TReturn>, Func<IReadOnlyList<Error>, TReturn>)** - Execute function and return value.
* **Result<T> OnSuccess(Func\<T, Result<T>>)** - Execute a function with the value if successful.
* **Result<T> OnFailure(Func<IReadOnlyList<Error>, Result<T>>)** - Execute a function if failed.

#### Side Effects & Debugging
* **Result<T> Tap(Action<T>)** - Execute an action with the value without modifying result.
* **new Result<T> TapOnFailure(Action<IReadOnlyList<Error>>)** - Execute an action only if failed.

#### Validation
* **Result<T> Ensure(Func\<T, bool>, Error)** - Validate the value, adding an error if condition is false.
* **Result<T> Ensure(Func\<T, bool>, string)** - Validate the value, adding an error message if condition is false.

---

### Error

* **string Code**
  Error code (optional).

* **string Message**
  Human-readable error message.

* **override string ToString()**
  Returns `"Code: Message"` or just `"Message"` if no code.

* **Equality operators**
  `==`, `!=` for value equality.

---

## Advanced Usage

### Pattern Matching
```csharp
// Handle both success and failure cases
var result = Result<int>.Try(() => int.Parse(input));

var message = result.Match(
    onSuccess: value => $"Parsed: {value}",
    onFailure: errors => $"Failed: {errors.First().Message}"
);
```

### Exception Safety with Try
```csharp
// Synchronous
var result = Result<int>.Try(() => RiskyOperation());

// Asynchronous
var asyncResult = await Result<string>.TryAsync(async () => await FetchDataAsync());
```

### Value Extraction with Fallbacks
```csharp
// Simple fallback
int value = result.GetValueOr(0);

// Lazy fallback
int value = result.GetValueOr(() => ExpensiveDefault());

// Alternative result
var final = primaryResult.OrElse(fallbackResult);
```

### Fluent Chaining & Validation
```csharp
var result = Result<int>.Success(42)
    .Ensure(v => v > 0, "Must be positive")
    .Ensure(v => v < 100, "Must be less than 100")
    .Map(v => v * 2)
    .Tap(v => Console.WriteLine($"Value: {v}"))
    .Bind(v => AnotherOperation(v));
```

### Async Operations
```csharp
var result = await Result<User>.Success(userId)
    .MapAsync(async id => await GetUserAsync(id))
    .BindAsync(async user => await ValidateUserAsync(user))
    .TapAsync(async user => await LogAsync($"User: {user.Name}"));
```

### Error Recovery
```csharp
var result = Result<int>.Failure("Database error")
    .OnFailure(errors =>
    {
        Logger.Log(errors);
        return Result<int>.Success(GetCachedValue());
    });
```

### Combining Results
```csharp
var combined = Result.Combine(
    ValidateName(name),
    ValidateEmail(email),
    ValidateAge(age)
);

if (combined.IsFailure)
{
    Console.WriteLine($"Validation failed: {string.Join(", ", combined.Errors)}");
}
```

---

## Thread Safety

### Overview

BYSResults is designed with immutability and thread safety in mind, but there are important considerations when sharing Result instances across threads.

### Thread-Safe Operations

**Immutable Components:**
- **Error** instances are fully immutable and thread-safe
  - All properties are readonly
  - Can be safely shared across threads

**Reading Results:**
- Reading properties (`IsSuccess`, `IsFailure`, `Value`, `Errors`) is thread-safe
- Once a Result is created, its success/failure state doesn't change

### Thread-Unsafe Operations

**Mutable Error Lists:**
- `AddError()` and `AddErrors()` methods modify the internal error list
- These mutations are **NOT thread-safe**
- Concurrent calls to `AddError()` from multiple threads can cause race conditions

**Recommendations:**

1. **Avoid Mutation After Creation** (Preferred)
   ```csharp
   // Good: Create result with all errors upfront
   var errors = new List<Error> { error1, error2 };
   var result = Result<int>.Failure(errors);
   // Now safe to share across threads
   ```

2. **Don't Share Mutable Results**
   ```csharp
   // Avoid: Sharing a result while adding errors
   var result = Result.Success();
   Task.Run(() => result.AddError(error1)); // NOT SAFE
   Task.Run(() => result.AddError(error2)); // NOT SAFE
   ```

3. **Synchronize Mutations**
   ```csharp
   // If you must mutate, use synchronization
   var result = Result.Success();
   var lockObj = new object();

   lock (lockObj)
   {
       result.AddError(error1);
       result.AddError(error2);
   }
   ```

### Best Practices for Concurrent Code

**Pattern 1: Immutable Results**
```csharp
// Create results immutably and share freely
public async Task<Result<Data>> ProcessAsync()
{
    var result = await FetchDataAsync();
    // Once created, safe to share
    return result;
}
```

**Pattern 2: Collect Then Create**
```csharp
// Collect errors locally, then create result once
public Result ValidateConcurrently(IEnumerable<Item> items)
{
    var errors = new ConcurrentBag<Error>();

    Parallel.ForEach(items, item =>
    {
        if (!IsValid(item))
            errors.Add(new Error($"Invalid: {item.Name}"));
    });

    return errors.Any()
        ? Result.Failure(errors)
        : Result.Success();
}
```

**Pattern 3: Task Results**
```csharp
// Each task creates its own result
public async Task<Result> ProcessMultipleAsync(IEnumerable<Item> items)
{
    var tasks = items.Select(ProcessItemAsync);
    var results = await Task.WhenAll(tasks);

    // Combine results (thread-safe operation)
    return Result.Combine(results);
}
```

### Summary

| Operation | Thread-Safe? | Notes |
|-----------|-------------|-------|
| Reading properties | ✓ Yes | Always safe once created |
| Creating new results | ✓ Yes | Factory methods are safe |
| `Map`, `Bind`, etc. | ✓ Yes | Return new instances |
| `AddError()` | ✗ No | Mutates internal list |
| `AddErrors()` | ✗ No | Mutates internal list |
| `Combine()` | ✓ Yes | Reads only, creates new result |

**General Rule:** Treat Result instances as immutable after creation. If you need to add errors, create the result with all errors upfront or ensure proper synchronization.

---

## Revision History

For a detailed changelog with all releases and changes, see [CHANGELOG.md](CHANGELOG.md).

**Latest Release: v1.2.1** (2025-10-31)
- Fixed `AddError(Exception)` to use exception type as error code
- Improved inner exception message formatting
- Removed `Result<T>.Combine()` (use `Result.Combine()` instead)
- Modernized `Error.GetHashCode()`

---

## Contributing

1. Fork the repository.
2. Create a feature branch:

   ```bash
   git checkout -b feature/YourFeature
   ```
3. Commit your changes:

   ```bash
   git commit -m "Add awesome feature"
   ```
4. Push to the branch:

   ```bash
   git push origin feature/YourFeature
   ```
5. Open a Pull Request.
6. Follow the code style and include tests.

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

---

## License

Licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## Authors & Acknowledgments

[@Thumper631](https://github.com/Thumper631)

Thanks to all contributors.

---

## Links

* NuGet: [https://www.nuget.org/packages/BYSResults](https://www.nuget.org/packages/BYSResults)
* Repository: [https://github.com/Thumper631/BYSResults](https://github.com/Thumper631/BYSResults)
* Issues: [https://github.com/Thumper631/BYSResults/issues](https://github.com/Thumper631/BYSResults/issues)
* Documentation: [https://Thumper631.github.io/BYSResults](https://Thumper631.github.io/BYSResults)