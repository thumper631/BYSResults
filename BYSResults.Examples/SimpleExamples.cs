using BYSResults;

namespace BYSResults.Examples;

/// <summary>
/// Simple, working examples that demonstrate core BYSResults features
/// </summary>
public class SimpleExamples
{
    public static void RunExamples()
    {
        Console.WriteLine("=== Simple BYSResults Examples ===\n");

        // Example 1: Basic Success and Failure
        Example1_BasicUsage();

        // Example 2: Validation with Ensure
        Example2_Validation();

        // Example 3: Chaining with Map and Bind
        Example3_Chaining();

        // Example 4: Pattern Matching
        Example4_PatternMatching();

        // Example 5: Error Handling
        Example5_ErrorHandling();

        // Example 6: Combining Results
        Example6_CombiningResults();

        // Example 7: Fallback with OrElse
        Example7_Fallback();

        // Example 8: Try/Catch Wrapper
        Example8_TryCatch();
    }

    static void Example1_BasicUsage()
    {
        Console.WriteLine("1. Basic Success and Failure:");

        var success = Result<int>.Success(42);
        Console.WriteLine($"   Success: {success.IsSuccess}, Value: {success.Value}");

        var failure = Result<int>.Failure("ERROR", "Something went wrong");
        Console.WriteLine($"   Failure: {failure.IsFailure}, Error: {failure.FirstError?.Message}\n");
    }

    static void Example2_Validation()
    {
        Console.WriteLine("2. Validation with Ensure:");

        var validAge = Result<int>.Success(25)
            .Ensure(age => age >= 18, "Must be 18 or older")
            .Ensure(age => age < 100, "Age must be realistic");

        Console.WriteLine($"   Age 25 valid: {validAge.IsSuccess}");

        var invalidAge = Result<int>.Success(15)
            .Ensure(age => age >= 18, "Must be 18 or older");

        Console.WriteLine($"   Age 15 valid: {invalidAge.IsSuccess}");
        Console.WriteLine($"   Error: {invalidAge.FirstError?.Message}\n");
    }

    static void Example3_Chaining()
    {
        Console.WriteLine("3. Chaining with Map and Bind:");

        var result = Result<int>.Success(10)
            .Map(x => x * 2)                                      // Transform value
            .Tap(x => Console.WriteLine($"   After Map: {x}"))
            .Bind(x => x > 15
                ? Result<int>.Success(x)
                : Result<int>.Failure("Value too small"))
            .Map(x => x + 5);

        Console.WriteLine($"   Final result: {result.Value}\n");
    }

    static void Example4_PatternMatching()
    {
        Console.WriteLine("4. Pattern Matching:");

        var result = Result<int>.Success(100);

        result.Match(
            onSuccess: value => Console.WriteLine($"   Success! Value is {value}"),
            onFailure: errors => Console.WriteLine($"   Failure! {errors.First().Message}")
        );

        var message = result.Match(
            onSuccess: value => $"Got value: {value}",
            onFailure: errors => $"Got error: {errors.First().Message}"
        );

        Console.WriteLine($"   Message: {message}\n");
    }

    static void Example5_ErrorHandling()
    {
        Console.WriteLine("5. Error Handling:");

        var result = Result<string>.Success("test")
            .Ensure(s => s.Length > 10, "String too short")
            .TapOnFailure(errors =>
            {
                Console.WriteLine("   Validation failed:");
                foreach (var error in errors)
                    Console.WriteLine($"     - {error.Message}");
            });

        Console.WriteLine();
    }

    static void Example6_CombiningResults()
    {
        Console.WriteLine("6. Combining Results:");

        var name = ValidateName("John");
        var email = ValidateEmail("john@example.com");
        var age = ValidateAge(25);

        var combined = Result.Combine(name, email, age);

        Console.WriteLine($"   All valid: {combined.IsSuccess}");
        if (combined.IsFailure)
        {
            foreach (var error in combined.Errors)
                Console.WriteLine($"     - {error.Message}");
        }
        Console.WriteLine();
    }

    static void Example7_Fallback()
    {
        Console.WriteLine("7. Fallback with OrElse:");

        var primary = Result<string>.Failure("Primary failed");
        var fallback = Result<string>.Success("Fallback value");

        var result = primary.OrElse(fallback);
        Console.WriteLine($"   Result: {result.Value}");

        // With function
        var lazyResult = primary.OrElse(() => Result<string>.Success("Lazy fallback"));
        Console.WriteLine($"   Lazy result: {lazyResult.Value}\n");
    }

    static void Example8_TryCatch()
    {
        Console.WriteLine("8. Try/Catch Wrapper:");

        var successResult = Result<int>.Try(() => int.Parse("42"));
        Console.WriteLine($"   Parse '42': {successResult.IsSuccess}, Value: {successResult.Value}");

        var failureResult = Result<int>.Try(() => int.Parse("invalid"));
        Console.WriteLine($"   Parse 'invalid': {failureResult.IsSuccess}");
        if (failureResult.IsFailure)
            Console.WriteLine($"   Error: {failureResult.FirstError?.Message}\n");
    }

    // Helper validation methods
    static Result ValidateName(string name)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(name), "Name is required")
            .Ensure(() => name.Length >= 2, "Name must be at least 2 characters");
    }

    static Result ValidateEmail(string email)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(email), "Email is required")
            .Ensure(() => email.Contains("@"), "Email must contain @");
    }

    static Result ValidateAge(int age)
    {
        return Result.Success()
            .Ensure(() => age >= 18, "Must be 18 or older")
            .Ensure(() => age < 150, "Age must be realistic");
    }
}
