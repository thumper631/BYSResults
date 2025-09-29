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
6. [Contributing](#contributing)  
7. [License](#license)  
8. [Authors & Acknowledgments](#authors--acknowledgments)  
9. [Links](#links)  

---

## Features

- **No exceptions for control flow** – all outcomes are explicit  
- **Fluent chaining** via `Bind`, `Map`, etc.  
- **Easy combination** with `Result.Combine(...)`  
- **Error aggregation** and inspection (`.Errors`, `.FirstError`)  
- **Generic and non-generic** variants (`Result` vs. `Result<T>`)  

---

## Installation

Install via **.NET CLI**:

```bash
dotnet add package BYSResults
````

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

* **bool IsSuccess**
  True if operation succeeded.

* **bool IsFailure**
  True if operation failed.

* **IReadOnlyList<Error> Errors**
  List of all errors (empty if success).

* **Error? FirstError**
  Shortcut to the first error, or null if none.

* **static Result Success()**
  Create a successful `Result`.

* **static Result Failure(...)**
  Create a failure `Result` (overloads: `Error`, `IEnumerable<Error>`, `string`, `(code, message)`).

* **static Result Combine(...)**
  Combine multiple `Result` instances into one, aggregating errors.

* **Result AddError(Error)**
  Add an error to an existing `Result`.

* **Result AddError(Exception)**
  Add an exception as a error to an existing `Result`.

* **Result AddErrors(...)**
  Add multiple errors to an existing `Result`.

---

### Result<T>

* **T? Value**
  The value if successful (default if failure).

* **static Result<T> Success(T value)**
  Create a successful `Result<T>` with the given value.

* **static Result<T> Failure(...)**
  Create a failure `Result<T>` (same overloads as `Result`).

* **static new Result<T> Combine(...)**
  Combine multiple `Result<T>` instances; returns `Result<T>`.

* **Result<TNext> Map(Func\<T, TNext>)**
  Transform the value on success, propagate errors on failure.

* **Result<TNext> Bind(Func\<T, Result<TNext>>)**
  Chain operations that return `Result<TNext>`.

* **Result<T> WithValue(T)**
  Set the `.Value` on an existing successful result.

* **new Result<T> AddError(Error)**
  Add an error (returns `Result<T>` for chaining).

* **new Result<T> AddErrors(...)**
  Add multiple errors (returns `Result<T>`).

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

```csharp
// Combine several results
var c = Result.Combine(r1, r2, r3);
if (c.IsFailure)
{
    Console.WriteLine($"Combined failed: {string.Join(", ", c.Errors)}");
}

// Fluent chaining
var final = Result.Success()
    .Bind(_ => SomeOperation())       // returns Result<T>
    .Map(value => Process(value))     // returns Result<U>
    .AddError(new Error("X100", "Extra issue"));
```

---

## Revision History

| Version | Date         | Description                    |
|---------|--------------|--------------------------------|
| 1.0.0   | 2025-05-08   | Initial check-in               |
| 1.1.0   | 2025-06-01   | Added AddError(Exception exception)        |
| 1.1.1   | 2025-06-01   | Correct issues with readme.md       |
| 1.1.2   | 2025-06-01   | Correct issues with readme.md       |
| 1.1.3   | 2025-06-01   | Added Revision History to readme.md       |
| 1.1.3   | 2025-06-02   | Updated GetInnerException to handle null InnerException       |

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
```
