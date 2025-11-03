# Contributing to BYSResults

Thank you for your interest in contributing to BYSResults! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Code Style Guidelines](#code-style-guidelines)
- [Testing Requirements](#testing-requirements)
- [Pull Request Process](#pull-request-process)
- [Reporting Issues](#reporting-issues)

## Code of Conduct

This project adheres to professional standards of respectful collaboration. Please be kind, constructive, and considerate in all interactions.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/bysresults.git
   cd bysresults
   ```
3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/ORIGINAL-OWNER/bysresults.git
   ```

## Development Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A code editor (Visual Studio 2022, VS Code, or JetBrains Rider recommended)
- Git

### Building the Project

```bash
# Build the solution
dotnet build src/BYSResults.sln

# Build in Release mode
dotnet build src/BYSResults.sln -c Release
```

### Running Tests

```bash
# Run all tests
dotnet test BYSResults.Tests/BYSResults.Tests.csproj

# Run tests with detailed output
dotnet test BYSResults.Tests/BYSResults.Tests.csproj --verbosity normal

# Run with code coverage
dotnet test BYSResults.Tests/BYSResults.Tests.csproj /p:CollectCoverage=true
```

### Running Examples

```bash
# Run the examples project
dotnet run --project BYSResults.Examples/BYSResults.Examples.csproj
```

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

- **Bug fixes**: Fix issues in existing functionality
- **New features**: Add new functionality (please discuss in an issue first)
- **Documentation**: Improve README, XML comments, or examples
- **Tests**: Add missing test coverage
- **Performance improvements**: Optimize existing code
- **Code quality**: Refactoring, cleanup, or modernization

### Before You Start

1. **Check existing issues** to see if your idea or bug is already being discussed
2. **Open an issue** to discuss significant changes before investing time
3. **Keep changes focused**: One feature/fix per pull request
4. **Update documentation**: Ensure README and XML comments reflect your changes

## Code Style Guidelines

### General Principles

- Follow standard C# coding conventions
- Use nullable reference types (`#nullable enable`)
- Prefer immutability where possible
- Write self-documenting code with clear names
- Keep methods focused and concise
- Follow SOLID principles

### Naming Conventions

- **Classes/Interfaces**: PascalCase (e.g., `Result`, `IError`)
- **Methods/Properties**: PascalCase (e.g., `GetValueOr`, `IsSuccess`)
- **Parameters/Variables**: camelCase (e.g., `errorMessage`, `value`)
- **Private fields**: camelCase with underscore prefix (e.g., `_errors`)
- **Constants**: PascalCase (e.g., `DefaultErrorCode`)

### Code Structure

```csharp
// ✓ Good: Clear, focused method with XML documentation
/// <summary>
/// Maps the result value to a new type using the provided function.
/// </summary>
/// <typeparam name="TNext">The type to map to.</typeparam>
/// <param name="func">The mapping function.</param>
/// <returns>A new result with the mapped value.</returns>
public Result<TNext> Map<TNext>(Func<T, TNext> func)
{
    if (IsFailure)
        return Result<TNext>.Failure(Errors);
    return Result<TNext>.Success(func(Value!));
}
```

### XML Documentation

- All public types, methods, and properties must have XML documentation
- Use `<summary>`, `<param>`, `<typeparam>`, and `<returns>` tags
- Describe what the code does, not how it does it
- Include examples for complex APIs using `<example>` tags

### Functional Programming Patterns

This library follows functional programming principles:

- **Immutability**: Prefer readonly fields and return new instances
- **Pure functions**: Avoid side effects in core logic
- **Fluent API**: Enable method chaining where appropriate
- **Railway-Oriented Programming**: Success/failure paths are explicit

## Testing Requirements

### Test Coverage

- **All new features** must include unit tests
- **Bug fixes** should include regression tests
- Aim for **high code coverage** (current: >90%)
- Test both **success and failure** paths

### Test Structure

Follow the Arrange-Act-Assert (AAA) pattern:

```csharp
[Fact]
public void Map_WithSuccessResult_TransformsValue()
{
    // Arrange
    var result = Result<int>.Success(5);

    // Act
    var mapped = result.Map(x => x * 2);

    // Assert
    Assert.True(mapped.IsSuccess);
    Assert.Equal(10, mapped.Value);
}
```

### Test Naming

- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Examples:
  - `Map_WithSuccessResult_TransformsValue`
  - `Bind_WithFailureResult_ReturnsFailure`
  - `Ensure_WhenPredicateFails_AddsError`

### Test Files

Place tests in the appropriate file:

- `ErrorTests.cs`: Error class tests
- `ResultTests.cs`: Non-generic Result tests
- `ResultGenericTests.cs`: Result<T> tests
- `ResultExtensionTests.cs`: Extension method tests

## Pull Request Process

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/issue-description
```

### 2. Make Your Changes

- Write clear, focused commits
- Follow the code style guidelines
- Add/update tests as needed
- Update documentation

### 3. Commit Your Changes

```bash
git add .
git commit -m "Add feature X that does Y"
```

**Commit message guidelines:**
- Use present tense ("Add feature" not "Added feature")
- Be descriptive but concise
- Reference issue numbers when applicable (e.g., "Fix #123: Handle null values")

### 4. Keep Your Branch Updated

```bash
git fetch upstream
git rebase upstream/main
```

### 5. Run All Tests

```bash
# Ensure all tests pass
dotnet test BYSResults.Tests/BYSResults.Tests.csproj

# Verify build succeeds
dotnet build src/BYSResults.sln -c Release
```

### 6. Push to Your Fork

```bash
git push origin feature/your-feature-name
```

### 7. Open a Pull Request

- Go to your fork on GitHub
- Click "New Pull Request"
- Select `main` as the base branch
- Provide a clear description of your changes
- Link related issues

### Pull Request Checklist

- [ ] Code follows style guidelines
- [ ] All tests pass
- [ ] New tests added for new functionality
- [ ] XML documentation added/updated
- [ ] README.md updated (if applicable)
- [ ] CLAUDE.md updated (if architecture changed)
- [ ] No breaking changes (or clearly documented)

## Reporting Issues

### Bug Reports

When reporting bugs, please include:

1. **Description**: Clear summary of the issue
2. **Steps to reproduce**: Minimal code example
3. **Expected behavior**: What should happen
4. **Actual behavior**: What actually happens
5. **Environment**: .NET version, OS, etc.

**Example:**

```markdown
**Description**: `Map` throws NullReferenceException when function returns null

**Steps to reproduce**:
```csharp
var result = Result<string>.Success("test");
var mapped = result.Map<string>(x => null); // Throws
```

**Expected**: Should create success result with null value

**Actual**: Throws NullReferenceException

**Environment**: .NET 8.0, Windows 11
```

### Feature Requests

When requesting features, please include:

1. **Use case**: What problem does this solve?
2. **Proposed solution**: How should it work?
3. **Alternatives**: Other approaches you've considered
4. **Examples**: Code examples of proposed API

## Release Process

(For maintainers only)

1. Update version in `BYSResults.csproj`
2. Update `README.md` revision history
3. Update `CLAUDE.md` recent changes section
4. Create git tag: `git tag -a v1.x.x -m "Release v1.x.x"`
5. Push tag: `git push origin v1.x.x`
6. Build and pack: `dotnet pack src/BYSResults/BYSResults.csproj -c Release`
7. Push to NuGet: `dotnet nuget push *.nupkg -s https://api.nuget.org/v3/index.json`

## Questions?

If you have questions about contributing:

1. Check the [README.md](README.md) for API documentation
2. Look at existing code and tests for examples
3. Open an issue for discussion

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to BYSResults! 🎉
