using BYSResults;

namespace BYSResults.Examples;

/// <summary>
/// Examples demonstrating Result usage with database operations
/// </summary>
public class DatabaseExamples
{
    private readonly IRepository _repository = new MockRepository();

    /// <summary>
    /// Example: Simple database query with error handling
    /// </summary>
    public Result<Customer> GetCustomer(int id)
    {
        return Result<int>.Try(() =>
        {
            if (id <= 0)
                throw new ArgumentException("Invalid customer ID");
            return id;
        })
        .Bind(validId => _repository.FindCustomer(validId))
        .Tap(customer => Console.WriteLine($"Retrieved customer: {customer.Name}"));
    }

    /// <summary>
    /// Example: Creating a record with validation and side effects
    /// </summary>
    public async Task<Result<Customer>> CreateCustomerAsync(CustomerDto dto)
    {
        return await Result<CustomerDto>.Success(dto)
            .Ensure(d => !string.IsNullOrEmpty(d.Name), "Name is required")
            .Ensure(d => !string.IsNullOrEmpty(d.Email), "Email is required")
            .Ensure(d => d.Email.Contains("@"), "Valid email is required")
            .Ensure(d => d.Age >= 18, new Error("AGE_RESTRICTION", "Must be 18 or older"))
            .MapAsync(async d => new Customer
            {
                Id = 0,
                Name = d.Name,
                Email = d.Email,
                Age = d.Age,
                CreatedAt = DateTime.UtcNow
            })
            .BindAsync(async customer => await _repository.SaveCustomerAsync(customer))
            .TapAsync(async customer => await SendWelcomeEmailAsync(customer));
    }

    /// <summary>
    /// Example: Update with optimistic concurrency check
    /// </summary>
    public async Task<Result<Customer>> UpdateCustomerAsync(int id, CustomerDto dto)
    {
        return await _repository.FindCustomerAsync(id)
            .Ensure(c => c != null, new Error("NOT_FOUND", $"Customer {id} not found"))
            .Map(customer =>
            {
                customer.Name = dto.Name;
                customer.Email = dto.Email;
                customer.Age = dto.Age;
                customer.UpdatedAt = DateTime.UtcNow;
                return customer;
            })
            .BindAsync(async customer => await _repository.UpdateCustomerAsync(customer))
            .TapOnFailure(errors => Console.WriteLine($"Update failed: {string.Join(", ", errors)}"));
    }

    /// <summary>
    /// Example: Transaction-style operation with rollback on failure
    /// </summary>
    public async Task<Result<Order>> CreateOrderAsync(OrderDto orderDto)
    {
        return await _repository.BeginTransactionAsync()
            .BindAsync(async tx => await ValidateInventoryAsync(orderDto.Items))
            .BindAsync(async _ => await _repository.CreateOrderAsync(orderDto))
            .BindAsync(async order => await _repository.ReserveInventoryAsync(order))
            .TapAsync(async order => await _repository.CommitTransactionAsync())
            .TapOnFailure(async errors => await _repository.RollbackTransactionAsync());
    }

    /// <summary>
    /// Example: Batch operation with error aggregation
    /// </summary>
    public async Task<Result<List<Customer>>> ImportCustomersAsync(List<CustomerDto> customers)
    {
        var results = new List<Result<Customer>>();

        foreach (var dto in customers)
        {
            var result = await CreateCustomerAsync(dto);
            results.Add(result);
        }

        // Check if any failed
        var failures = results.Where(r => r.IsFailure).ToList();
        if (failures.Any())
        {
            var allErrors = failures.SelectMany(f => f.Errors).ToList();
            return Result<List<Customer>>.Failure(allErrors);
        }

        var successfulCustomers = results.Select(r => r.Value!).ToList();
        return Result<List<Customer>>.Success(successfulCustomers);
    }

    /// <summary>
    /// Example: Query with fallback to cache
    /// </summary>
    public async Task<Result<Customer>> GetCustomerWithCacheAsync(int id)
    {
        return await _repository.FindCustomerAsync(id)
            .OrElse(async () => await LoadFromCacheAsync(id))
            .OrElse(() => Result<Customer>.Failure("NOT_FOUND", "Customer not found in database or cache"));
    }

    /// <summary>
    /// Example: Soft delete with audit trail
    /// </summary>
    public async Task<Result> DeleteCustomerAsync(int id, string deletedBy)
    {
        return await _repository.FindCustomerAsync(id)
            .Ensure(c => c != null, new Error("NOT_FOUND", "Customer not found"))
            .Map(customer =>
            {
                customer.IsDeleted = true;
                customer.DeletedAt = DateTime.UtcNow;
                customer.DeletedBy = deletedBy;
                return customer;
            })
            .BindAsync(async customer => await _repository.UpdateCustomerAsync(customer))
            .Map(_ => Result.Success())
            .GetValueOr(Result.Failure("DELETE_FAILED", "Failed to delete customer"));
    }

    private async Task<Result> ValidateInventoryAsync(List<OrderItemDto> items)
    {
        await Task.Delay(10); // Simulate async work
        return items.All(i => i.Quantity > 0)
            ? Result.Success()
            : Result.Failure("INVENTORY", "Insufficient inventory");
    }

    private async Task SendWelcomeEmailAsync(Customer customer)
    {
        await Task.Delay(10); // Simulate sending email
        Console.WriteLine($"Welcome email sent to {customer.Email}");
    }

    private async Task<Result<Customer>> LoadFromCacheAsync(int id)
    {
        await Task.Delay(10);
        return Result<Customer>.Failure("NOT_IN_CACHE", "Customer not in cache");
    }

    public static void RunExamples()
    {
        Console.WriteLine("=== Database Examples ===\n");

        var examples = new DatabaseExamples();

        // Example 1: Get customer
        Console.WriteLine("1. Get Customer:");
        var customer = examples.GetCustomer(1);
        customer.Match(
            onSuccess: c => Console.WriteLine($"   Found: {c.Name}"),
            onFailure: errors => Console.WriteLine($"   Error: {errors.First().Message}")
        );
        Console.WriteLine();

        // Example 2: Create customer with validation
        Console.WriteLine("2. Create Customer (Async):");
        var createTask = examples.CreateCustomerAsync(new CustomerDto
        {
            Name = "Jane Smith",
            Email = "jane@example.com",
            Age = 25
        });
        createTask.Wait();
        var createResult = createTask.Result;
        Console.WriteLine($"   Success: {createResult.IsSuccess}");
        if (createResult.IsSuccess)
            Console.WriteLine($"   Created: {createResult.Value?.Name}");
        Console.WriteLine();

        // Example 3: Create customer with validation failure
        Console.WriteLine("3. Create Customer (Validation Failed):");
        var invalidTask = examples.CreateCustomerAsync(new CustomerDto
        {
            Name = "",
            Email = "invalid",
            Age = 15
        });
        invalidTask.Wait();
        var invalidResult = invalidTask.Result;
        Console.WriteLine($"   Success: {invalidResult.IsSuccess}");
        if (invalidResult.IsFailure)
        {
            Console.WriteLine("   Errors:");
            foreach (var error in invalidResult.Errors)
                Console.WriteLine($"     - {error.Message}");
        }
        Console.WriteLine();

        // Example 4: Batch import
        Console.WriteLine("4. Batch Import:");
        var batchTask = examples.ImportCustomersAsync(new List<CustomerDto>
        {
            new() { Name = "User 1", Email = "user1@example.com", Age = 30 },
            new() { Name = "User 2", Email = "user2@example.com", Age = 25 }
        });
        batchTask.Wait();
        var batchResult = batchTask.Result;
        Console.WriteLine($"   Imported: {batchResult.Value?.Count ?? 0} customers");
        Console.WriteLine();
    }
}

// Supporting classes
public class CustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class OrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public interface IRepository
{
    Result<Customer> FindCustomer(int id);
    Task<Result<Customer>> FindCustomerAsync(int id);
    Task<Result<Customer>> SaveCustomerAsync(Customer customer);
    Task<Result<Customer>> UpdateCustomerAsync(Customer customer);
    Task<Result> BeginTransactionAsync();
    Task<Result> CommitTransactionAsync();
    Task<Result> RollbackTransactionAsync();
    Task<Result<Order>> CreateOrderAsync(OrderDto orderDto);
    Task<Result<Order>> ReserveInventoryAsync(Order order);
}

public class MockRepository : IRepository
{
    private readonly List<Customer> _customers = new()
    {
        new Customer { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Age = 30, CreatedAt = DateTime.UtcNow },
        new Customer { Id = 2, Name = "Bob Smith", Email = "bob@example.com", Age = 35, CreatedAt = DateTime.UtcNow }
    };

    private int _nextId = 3;

    public Result<Customer> FindCustomer(int id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        return customer != null
            ? Result<Customer>.Success(customer)
            : Result<Customer>.Failure("NOT_FOUND", $"Customer {id} not found");
    }

    public async Task<Result<Customer>> FindCustomerAsync(int id)
    {
        await Task.Delay(10);
        return FindCustomer(id);
    }

    public async Task<Result<Customer>> SaveCustomerAsync(Customer customer)
    {
        await Task.Delay(10);
        customer.Id = _nextId++;
        _customers.Add(customer);
        return Result<Customer>.Success(customer);
    }

    public async Task<Result<Customer>> UpdateCustomerAsync(Customer customer)
    {
        await Task.Delay(10);
        var existing = _customers.FirstOrDefault(c => c.Id == customer.Id);
        if (existing == null)
            return Result<Customer>.Failure("NOT_FOUND", "Customer not found");

        existing.Name = customer.Name;
        existing.Email = customer.Email;
        existing.Age = customer.Age;
        existing.UpdatedAt = customer.UpdatedAt;
        return Result<Customer>.Success(existing);
    }

    public async Task<Result> BeginTransactionAsync()
    {
        await Task.Delay(10);
        return Result.Success();
    }

    public async Task<Result> CommitTransactionAsync()
    {
        await Task.Delay(10);
        return Result.Success();
    }

    public async Task<Result> RollbackTransactionAsync()
    {
        await Task.Delay(10);
        return Result.Success();
    }

    public async Task<Result<Order>> CreateOrderAsync(OrderDto orderDto)
    {
        await Task.Delay(10);
        var order = new Order
        {
            Id = 1,
            Items = orderDto.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };
        return Result<Order>.Success(order);
    }

    public async Task<Result<Order>> ReserveInventoryAsync(Order order)
    {
        await Task.Delay(10);
        return Result<Order>.Success(order);
    }
}
