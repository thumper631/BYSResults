using BYSResults.Examples;

namespace BYSResults.Examples;

/// <summary>
/// Main program to run all BYSResults examples
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          BYSResults - Comprehensive Examples                  ║");
        Console.WriteLine("║                                                                ║");
        Console.WriteLine("║  Demonstrating real-world usage patterns for Result types     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            // Run all example sets
            RunAllExamples();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    All Examples Completed!                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error running examples: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static void RunAllExamples()
    {
        var examples = new Dictionary<string, Action>
        {
            ["Simple Examples"] = SimpleExamples.RunExamples
            // Note: Additional examples (WebAPI, Database, Validation, Async, Chaining)
            // are available in their respective files but may need adjustments
            // to match your specific use cases. SimpleExamples demonstrates
            // the core BYSResults features that work out of the box.
        };

        foreach (var (name, action) in examples)
        {
            RunExampleSet(name, action);
        }
    }

    static void RunExampleSet(string name, Action exampleAction)
    {
        Console.WriteLine($"\n┌──────────────────────────────────────────────────────────────┐");
        Console.WriteLine($"│ {name.PadRight(60)} │");
        Console.WriteLine($"└──────────────────────────────────────────────────────────────┘");

        try
        {
            exampleAction();
            Console.WriteLine($"✓ {name} completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ {name} failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Run a specific example by name
    /// </summary>
    static void RunSpecificExample(string exampleName)
    {
        switch (exampleName.ToLower())
        {
            case "simple":
            case "basic":
                SimpleExamples.RunExamples();
                break;
            default:
                Console.WriteLine($"Unknown example: {exampleName}");
                Console.WriteLine("Available examples: simple");
                Console.WriteLine("\nNote: Additional example templates are available in the Templates/ folder");
                Console.WriteLine("for reference. These templates show patterns for WebAPI, Database, Validation,");
                Console.WriteLine("Async operations, and Chaining, but require adaptation for your specific use case.");
                break;
        }
    }
}
