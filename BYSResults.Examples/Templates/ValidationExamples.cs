using BYSResults;

namespace BYSResults.Examples;

/// <summary>
/// Examples demonstrating validation patterns with Result types
/// </summary>
public class ValidationExamples
{
    /// <summary>
    /// Example: Simple inline validation with Ensure
    /// </summary>
    public Result<int> ValidateAge(int age)
    {
        return Result<int>.Success(age)
            .Ensure(a => a >= 0, "Age cannot be negative")
            .Ensure(a => a <= 150, "Age must be realistic")
            .Ensure(a => a >= 18, new Error("AGE_RESTRICTION", "Must be 18 or older"));
    }

    /// <summary>
    /// Example: Email validation with multiple rules
    /// </summary>
    public Result<string> ValidateEmail(string email)
    {
        return Result<string>.Success(email)
            .Ensure(e => !string.IsNullOrWhiteSpace(e), "Email is required")
            .Ensure(e => e.Contains("@"), "Email must contain @")
            .Ensure(e => e.Contains("."), "Email must contain domain")
            .Ensure(e => e.Length >= 5, "Email is too short")
            .Ensure(e => e.Length <= 255, "Email is too long")
            .Ensure(e => !e.StartsWith("@"), "Email cannot start with @")
            .Ensure(e => !e.EndsWith("@"), "Email cannot end with @");
    }

    /// <summary>
    /// Example: Combining multiple field validations
    /// </summary>
    public Result ValidateRegistrationForm(RegistrationForm form)
    {
        var nameValidation = ValidateName(form.Name);
        var emailValidation = ValidateEmail(form.Email);
        var passwordValidation = ValidatePassword(form.Password);
        var ageValidation = ValidateAge(form.Age);

        return Result.Combine(nameValidation, emailValidation, passwordValidation, ageValidation);
    }

    /// <summary>
    /// Example: Collecting all validation errors at once
    /// </summary>
    public Result<RegistrationForm> ValidateRegistrationFormWithDetails(RegistrationForm form)
    {
        var errors = new List<Error>();

        // Name validation
        if (string.IsNullOrWhiteSpace(form.Name))
            errors.Add(new Error("NAME_REQUIRED", "Name is required"));
        else if (form.Name.Length < 2)
            errors.Add(new Error("NAME_TOO_SHORT", "Name must be at least 2 characters"));

        // Email validation
        if (string.IsNullOrWhiteSpace(form.Email))
            errors.Add(new Error("EMAIL_REQUIRED", "Email is required"));
        else if (!form.Email.Contains("@"))
            errors.Add(new Error("EMAIL_INVALID", "Email must contain @"));

        // Password validation
        if (string.IsNullOrWhiteSpace(form.Password))
            errors.Add(new Error("PASSWORD_REQUIRED", "Password is required"));
        else
        {
            if (form.Password.Length < 8)
                errors.Add(new Error("PASSWORD_TOO_SHORT", "Password must be at least 8 characters"));
            if (!form.Password.Any(char.IsUpper))
                errors.Add(new Error("PASSWORD_NO_UPPER", "Password must contain uppercase letter"));
            if (!form.Password.Any(char.IsDigit))
                errors.Add(new Error("PASSWORD_NO_DIGIT", "Password must contain digit"));
        }

        // Age validation
        if (form.Age < 18)
            errors.Add(new Error("AGE_RESTRICTION", "Must be 18 or older"));
        if (form.Age > 150)
            errors.Add(new Error("AGE_INVALID", "Age must be realistic"));

        return errors.Any()
            ? Result<RegistrationForm>.Failure(errors)
            : Result<RegistrationForm>.Success(form);
    }

    /// <summary>
    /// Example: Business rule validation
    /// </summary>
    public Result<Order> ValidateOrder(Order order)
    {
        return Result<Order>.Success(order)
            .Ensure(o => o.Items.Any(), "Order must contain at least one item")
            .Ensure(o => o.Items.All(i => i.Quantity > 0), "All items must have positive quantity")
            .Ensure(o => o.Items.All(i => i.Price >= 0), "All items must have valid price")
            .Ensure(o => o.TotalAmount == o.Items.Sum(i => i.Price * i.Quantity),
                new Error("AMOUNT_MISMATCH", "Total amount doesn't match items"))
            .Ensure(o => o.TotalAmount <= 10000,
                new Error("AMOUNT_LIMIT", "Order exceeds maximum amount of $10,000"));
    }

    /// <summary>
    /// Example: Conditional validation based on object state
    /// </summary>
    public Result<User> ValidateUserUpdate(User user, bool isAdmin)
    {
        var result = Result<User>.Success(user)
            .Ensure(u => !string.IsNullOrWhiteSpace(u.Name), "Name is required")
            .Ensure(u => !string.IsNullOrWhiteSpace(u.Email), "Email is required");

        // Additional validation for non-admin users
        if (!isAdmin)
        {
            result = result
                .Ensure(u => u.Role != "Admin", "Cannot assign Admin role")
                .Ensure(u => u.Status != "Suspended", "Cannot change status to Suspended");
        }

        return result;
    }

    /// <summary>
    /// Example: Cross-field validation
    /// </summary>
    public Result ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        return Result.Success()
            .Ensure(() => startDate < endDate, "Start date must be before end date")
            .Ensure(() => endDate <= DateTime.UtcNow.AddYears(1), "End date cannot be more than 1 year in future")
            .Ensure(() => (endDate - startDate).TotalDays <= 90, "Date range cannot exceed 90 days");
    }

    /// <summary>
    /// Example: Nested object validation
    /// </summary>
    public Result<CustomerOrder> ValidateCustomerOrder(CustomerOrder order)
    {
        // Validate customer
        var customerValidation = Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(order.CustomerName), "Customer name is required")
            .Ensure(() => !string.IsNullOrWhiteSpace(order.CustomerEmail), "Customer email is required");

        // Validate shipping address
        var addressValidation = Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(order.ShippingAddress.Street), "Street is required")
            .Ensure(() => !string.IsNullOrWhiteSpace(order.ShippingAddress.City), "City is required")
            .Ensure(() => !string.IsNullOrWhiteSpace(order.ShippingAddress.PostalCode), "Postal code is required");

        // Validate order items
        var itemsValidation = Result<CustomerOrder>.Success(order)
            .Ensure(o => o.Items.Any(), "Order must contain items")
            .Ensure(o => o.Items.All(i => i.Quantity > 0), "All quantities must be positive");

        // Combine all validations
        var combined = Result.Combine(customerValidation, addressValidation);

        return combined.IsSuccess
            ? itemsValidation
            : Result<CustomerOrder>.Failure(combined.Errors);
    }

    /// <summary>
    /// Example: Validation with transformation
    /// </summary>
    public Result<NormalizedUser> ValidateAndNormalizeUser(string name, string email)
    {
        return Result<(string name, string email)>.Success((name, email))
            .Ensure(t => !string.IsNullOrWhiteSpace(t.name), "Name is required")
            .Ensure(t => !string.IsNullOrWhiteSpace(t.email), "Email is required")
            .Map(t => new NormalizedUser
            {
                Name = t.name.Trim(),
                Email = t.email.Trim().ToLowerInvariant()
            })
            .Ensure(u => u.Email.Contains("@"), "Invalid email format");
    }

    private Result ValidateName(string name)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(name), "Name is required")
            .Ensure(() => name.Length >= 2, "Name must be at least 2 characters");
    }

    private Result ValidatePassword(string password)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(password), "Password is required")
            .Ensure(() => password.Length >= 8, "Password must be at least 8 characters")
            .Ensure(() => password.Any(char.IsUpper), "Password must contain uppercase letter")
            .Ensure(() => password.Any(char.IsDigit), "Password must contain digit");
    }

    public static void RunExamples()
    {
        Console.WriteLine("=== Validation Examples ===\n");

        var examples = new ValidationExamples();

        // Example 1: Simple age validation
        Console.WriteLine("1. Age Validation:");
        var validAge = examples.ValidateAge(25);
        var invalidAge = examples.ValidateAge(15);
        Console.WriteLine($"   Age 25: {validAge.IsSuccess}");
        Console.WriteLine($"   Age 15: {invalidAge.IsSuccess} - {invalidAge.FirstError?.Message}");
        Console.WriteLine();

        // Example 2: Email validation
        Console.WriteLine("2. Email Validation:");
        var validEmail = examples.ValidateEmail("user@example.com");
        var invalidEmail = examples.ValidateEmail("invalid");
        Console.WriteLine($"   'user@example.com': {validEmail.IsSuccess}");
        Console.WriteLine($"   'invalid': {invalidEmail.IsSuccess}");
        if (invalidEmail.IsFailure)
        {
            Console.WriteLine("   Errors:");
            foreach (var error in invalidEmail.Errors)
                Console.WriteLine($"     - {error.Message}");
        }
        Console.WriteLine();

        // Example 3: Form validation with multiple fields
        Console.WriteLine("3. Registration Form Validation:");
        var form = new RegistrationForm
        {
            Name = "Jo",
            Email = "invalid",
            Password = "weak",
            Age = 15
        };
        var formResult = examples.ValidateRegistrationFormWithDetails(form);
        Console.WriteLine($"   Success: {formResult.IsSuccess}");
        if (formResult.IsFailure)
        {
            Console.WriteLine("   Errors:");
            foreach (var error in formResult.Errors)
                Console.WriteLine($"     [{error.Code}] {error.Message}");
        }
        Console.WriteLine();

        // Example 4: Order validation
        Console.WriteLine("4. Order Validation:");
        var order = new Order
        {
            Items = new List<OrderItem>
            {
                new() { ProductId = 1, Quantity = 2, Price = 50.00m },
                new() { ProductId = 2, Quantity = 1, Price = 30.00m }
            },
            TotalAmount = 130.00m
        };
        var orderResult = examples.ValidateOrder(order);
        Console.WriteLine($"   Success: {orderResult.IsSuccess}");
        Console.WriteLine();

        // Example 5: Normalize and validate
        Console.WriteLine("5. Validate and Normalize:");
        var normalized = examples.ValidateAndNormalizeUser("  John Doe  ", "  JOHN@EXAMPLE.COM  ");
        if (normalized.IsSuccess)
        {
            Console.WriteLine($"   Name: '{normalized.Value!.Name}'");
            Console.WriteLine($"   Email: '{normalized.Value!.Email}'");
        }
        Console.WriteLine();
    }
}

// Supporting classes
public class RegistrationForm
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class CustomerOrder
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public Address ShippingAddress { get; set; } = new();
    public List<OrderItem> Items { get; set; } = new();
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public class NormalizedUser
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
