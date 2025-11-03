# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.3] - 2025-11-02

### Added
- **"Choosing Between Result and Result<T>"** section in README with comprehensive guidance on when to use each type
  - Quick decision rules with code examples
  - Common scenarios comparison table
  - Feature availability explanation
  - Practical repository examples (delete vs get operations)
  - Design tip comparing to traditional void vs typed return values

### Changed
- Enhanced API Reference section introduction with cross-reference to usage guidance
- Improved XML documentation for `Result` class to clarify it's for operations without return values
- Improved XML documentation for `Result<T>` class to clarify it's for operations that return values on success

## [1.2.2] - 2025-11-02

### Added
- Comprehensive **Thread Safety** documentation section in README with best practices for concurrent code
- **CONTRIBUTING.md** with detailed contribution guidelines, code style, testing requirements, and PR process
- **CHANGELOG.md** following Keep a Changelog format for structured version history
- **.editorconfig** for consistent code formatting across editors (C#, XML, JSON, YAML, Markdown)
- **GitHub issue templates** for bug reports and feature requests
- **GitHub PR template** with comprehensive checklist

### Changed
- Updated copyright statements to "Copyright (c) 2025 James Thompson" for consistency with LICENSE
- Removed manual "Last Modified On" dates from file headers (Git provides accurate tracking)
- Updated Revision History in README to link to CHANGELOG.md

### Fixed
- Fixed README API documentation by removing outdated `Result<T>.Combine()` reference (removed in v1.2.1)

## [1.2.1] - 2025-10-31

### Fixed
- Fixed `AddError(Exception)` to use exception type name (e.g., `ArgumentNullException`) as error code instead of exception message
- Improved inner exception message formatting with `-->` separator for better clarity
- Updated XML documentation for `AddError`/`AddErrors` methods to accurately reflect behavior

### Changed
- Modernized `Error.GetHashCode()` to use `HashCode.Combine()` instead of custom hash calculation

### Removed
- **BREAKING**: Removed `Result<T>.Combine()` method - use `Result.Combine()` from base class instead, which accepts both `Result` and `Result<T>` instances

## [1.2.0] - 2025-10-29

### Added
- **Pattern Matching**: `Match` methods for elegant success/failure handling (void and generic variants)
- **Exception Safety**: `Try` and `TryAsync` factory methods for safe exception handling
- **Value Extraction**: `GetValueOr` and `OrElse` methods for fallback handling
- **Side Effects**: `Tap`, `TapAsync`, `TapOnSuccess`, `TapOnFailure` methods for executing actions without breaking chains
- **Control Flow**: `OnSuccess` and `OnFailure` methods for conditional execution
- **Validation**: `Ensure` methods for inline condition checking with predicates
- **Async Operations**: `MapAsync`, `BindAsync`, `TapAsync` for asynchronous composition
- Comprehensive test suite in `ResultExtensionTests.cs` covering all new features

## [1.1.5] - 2025-09-30

### Fixed
- Fixed NuGet package health issues by enabling deterministic builds
- Added continuous integration build support
- Added symbol packages (.snupkg) for better debugging experience

### Added
- Unit tests for existing functionality
- Code coverage reporting

## [1.1.4] - 2025-06-02

### Fixed
- Updated `GetInnerException` helper method to safely handle null `InnerException` property

## [1.1.3] - 2025-06-01

### Added
- Added Revision History section to README.md for better version tracking

## [1.1.2] - 2025-06-01

### Fixed
- Corrected formatting and content issues in README.md

## [1.1.1] - 2025-06-01

### Fixed
- Corrected issues with README.md documentation

## [1.1.0] - 2025-06-01

### Added
- `AddError(Exception exception)` method to convert exceptions to errors
- Exception-to-error conversion support in `Result` and `Result<T>`

## [1.0.0] - 2025-05-08

### Added
- Initial release of BYSResults library
- Core `Error` class with immutable error representation
- `Result` class for operations without return values
- `Result<T>` generic class for operations with typed return values
- Factory methods: `Success()`, `Failure(...)`
- Error management: `AddError`, `AddErrors`
- Error aggregation: `Combine(...)`
- Functional composition: `Map`, `Bind`
- Fluent API support
- Value mutation: `WithValue`
- Implicit conversion from `T` to `Result<T>`
- MIT License
- Comprehensive README documentation
- NuGet package configuration

---

[1.2.3]: https://github.com/Thumper631/BYSResults/compare/v1.2.2...v1.2.3
[1.2.2]: https://github.com/Thumper631/BYSResults/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/Thumper631/BYSResults/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/Thumper631/BYSResults/compare/v1.1.5...v1.2.0
[1.1.5]: https://github.com/Thumper631/BYSResults/compare/v1.1.4...v1.1.5
[1.1.4]: https://github.com/Thumper631/BYSResults/compare/v1.1.3...v1.1.4
[1.1.3]: https://github.com/Thumper631/BYSResults/compare/v1.1.2...v1.1.3
[1.1.2]: https://github.com/Thumper631/BYSResults/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/Thumper631/BYSResults/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/Thumper631/BYSResults/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/Thumper631/BYSResults/releases/tag/v1.0.0
