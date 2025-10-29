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
4. [API Reference](#api-reference)  
   - [Result](#result)  
   - [Result&lt;T&gt;](#resultt)  
   - [Error](#error)  
5. [Advanced Usage](#advanced-usage)  
6. [Revision History](#revision-history) 
7. [Contributing](#contributing)  
8. [License](#license)  
9. [Authors & Acknowledgments](#authors--acknowledgments)  
10. [Links](#links)  

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
* **static new Result<T> Combine(...)** - Combine multiple `Result<T>` instances; returns `Result<T>`.

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

## Revision History

| Version | Date       | Description                                                              |
|---------|------------|--------------------------------------------------------------------------|
| 1.0.0   | 2025-05-08 | Initial check-in                                                         |
| 1.1.0   | 2025-06-01 | Added AddError(Exception exception)                                      |
| 1.1.1   | 2025-06-01 | Corrected issues with readme.md                                          |
| 1.1.2   | 2025-06-01 | Corrected issues with readme.md                                          |
| 1.1.3   | 2025-06-01 | Added Revision History to readme.md                                      |
| 1.1.4   | 2025-06-02 | Updated GetInnerException to handle null InnerException                  |
| 1.1.5   | 2025-09-30 | Fixed NuGet package health issues (deterministic builds, symbols); added unit tests |
| 1.2.0   | 2025-10-29 | Major feature release: Added Match pattern matching, Try/TryAsync exception safety, GetValueOr/OrElse value extraction, Tap/TapAsync side effects, OnSuccess/OnFailure callbacks, Ensure validation, MapAsync/BindAsync async operations, and comprehensive test suite |

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
```

## Key Changes Made:

1. **Fixed duplicate version 1.1.3** - The original had two entries for 1.1.3; I kept the first one and corrected the second to be 1.1.4
2. **Added version 1.1.5** with today's date (2025-09-30) and a description of the changes
3. **Fixed formatting issues** in the revision history table for consistency
4. **Corrected "Correct" to "Corrected"** in version descriptions for proper grammar
5. **Fixed Table of Contents numbering** - Contributing was listed as item 6 twice; corrected the sequence

The revision history now clearly shows the progression of your package and includes the important infrastructure improvements made in version 1.1.5!