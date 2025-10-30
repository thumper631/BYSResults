using BYSResults;
using System.Text.Json;

namespace BYSResults.Examples;

/// <summary>
/// Examples demonstrating async operations and external API integration
/// </summary>
public class AsyncExamples
{
    private readonly HttpClient _httpClient = new();

    /// <summary>
    /// Example: Basic async operation with TryAsync
    /// </summary>
    public async Task<Result<string>> FetchDataAsync(string url)
    {
        return await Result<string>.TryAsync(async () =>
            await _httpClient.GetStringAsync(url));
    }

    /// <summary>
    /// Example: Chaining async operations with MapAsync and BindAsync
    /// </summary>
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

    /// <summary>
    /// Example: Parallel async operations
    /// </summary>
    public async Task<Result<DashboardData>> GetDashboardDataAsync(int userId)
    {
        // Execute multiple async operations in parallel
        var userTask = GetUserAsync(userId);
        var ordersTask = GetUserOrdersAsync(userId);
        var statsTask = GetUserStatsAsync(userId);

        await Task.WhenAll(userTask, ordersTask, statsTask);

        var userResult = await userTask;
        var ordersResult = await ordersTask;
        var statsResult = await statsTask;

        // Combine results
        var combined = Result.Combine(userResult, ordersResult, statsResult);

        return combined.IsSuccess
            ? Result<DashboardData>.Success(new DashboardData
            {
                User = userResult.Value!,
                Orders = ordersResult.Value!,
                Stats = statsResult.Value!
            })
            : Result<DashboardData>.Failure(combined.Errors);
    }

    /// <summary>
    /// Example: Sequential async operations with early exit
    /// </summary>
    public async Task<Result<ProcessedOrder>> ProcessOrderPipelineAsync(OrderRequest request)
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

    /// <summary>
    /// Example: Retry logic with async
    /// </summary>
    public async Task<Result<T>> RetryAsync<T>(Func<Task<Result<T>>> operation, int maxRetries = 3)
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

        return lastResult!.AddError(new Error("MAX_RETRIES", $"Operation failed after {maxRetries} attempts"));
    }

    /// <summary>
    /// Example: Async with timeout
    /// </summary>
    public async Task<Result<T>> WithTimeoutAsync<T>(Task<Result<T>> operation, TimeSpan timeout)
    {
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(operation, timeoutTask);

        if (completedTask == timeoutTask)
            return Result<T>.Failure("TIMEOUT", $"Operation timed out after {timeout.TotalSeconds} seconds");

        return await operation;
    }

    /// <summary>
    /// Example: External API call with error handling
    /// </summary>
    public async Task<Result<ExchangeRate>> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        return await Result<(string from, string to)>.TryAsync(async () =>
        {
            var url = $"https://api.exchangerate.example.com/rate/{fromCurrency}/{toCurrency}";
            var response = await _httpClient.GetStringAsync(url);
            return (fromCurrency, toCurrency);
        })
        .MapAsync(async tuple => await ParseExchangeRateAsync(tuple.from, tuple.to))
        .Ensure(rate => rate.Rate > 0, "Invalid exchange rate")
        .TapOnFailure(errors => Console.WriteLine($"Failed to get exchange rate: {errors.First().Message}"));
    }

    /// <summary>
    /// Example: Batch async operations with partial failure handling
    /// </summary>
    public async Task<Result<BatchResult<T>>> ProcessBatchAsync<T>(List<T> items, Func<T, Task<Result<T>>> processor)
    {
        var results = new List<Result<T>>();

        foreach (var item in items)
        {
            var result = await processor(item);
            results.Add(result);
        }

        var successful = results.Where(r => r.IsSuccess).Select(r => r.Value!).ToList();
        var failed = results.Where(r => r.IsFailure).ToList();

        return Result<BatchResult<T>>.Success(new BatchResult<T>
        {
            Successful = successful,
            Failed = failed.SelectMany(f => f.Errors).ToList(),
            TotalProcessed = items.Count,
            SuccessCount = successful.Count,
            FailureCount = failed.Count
        });
    }

    /// <summary>
    /// Example: Async stream processing
    /// </summary>
    public async Task<Result<List<T>>> ProcessStreamAsync<T>(IAsyncEnumerable<T> stream, Func<T, Task<Result<T>>> processor)
    {
        var results = new List<T>();
        var errors = new List<Error>();

        await foreach (var item in stream)
        {
            var result = await processor(item);
            if (result.IsSuccess)
                results.Add(result.Value!);
            else
                errors.AddRange(result.Errors);
        }

        return errors.Any()
            ? Result<List<T>>.Failure(errors)
            : Result<List<T>>.Success(results);
    }

    /// <summary>
    /// Example: Fallback chain with async
    /// </summary>
    public async Task<Result<Configuration>> LoadConfigurationAsync()
    {
        return await LoadFromRemoteAsync()
            .OrElse(async () => await LoadFromDatabaseAsync())
            .OrElse(async () => await LoadFromFileAsync())
            .OrElse(() => Result<Configuration>.Success(GetDefaultConfiguration()));
    }

    // Helper methods
    private async Task<string> FetchWeatherJsonAsync(string city)
    {
        await Task.Delay(10);
        return $"{{\"city\":\"{city}\",\"temperature\":22.5}}";
    }

    private async Task<WeatherData> DeserializeWeatherAsync(string json)
    {
        await Task.Delay(10);
        return JsonSerializer.Deserialize<WeatherData>(json) ?? new WeatherData();
    }

    private async Task<Result<User>> GetUserAsync(int userId)
    {
        await Task.Delay(10);
        return Result<User>.Success(new User { Id = userId, Name = "John Doe" });
    }

    private async Task<Result<List<Order>>> GetUserOrdersAsync(int userId)
    {
        await Task.Delay(10);
        return Result<List<Order>>.Success(new List<Order>());
    }

    private async Task<Result<UserStats>> GetUserStatsAsync(int userId)
    {
        await Task.Delay(10);
        return Result<UserStats>.Success(new UserStats { TotalOrders = 5 });
    }

    private async Task<Result<OrderRequest>> ValidateOrderAsync(OrderRequest request)
    {
        await Task.Delay(10);
        return Result<OrderRequest>.Success(request);
    }

    private async Task<Result<OrderRequest>> CheckInventoryAsync(OrderRequest request)
    {
        await Task.Delay(10);
        return Result<OrderRequest>.Success(request);
    }

    private async Task<Result<OrderRequest>> CalculatePricingAsync(OrderRequest request)
    {
        await Task.Delay(10);
        return Result<OrderRequest>.Success(request);
    }

    private async Task<Result<OrderRequest>> ProcessPaymentAsync(OrderRequest request)
    {
        await Task.Delay(10);
        return Result<OrderRequest>.Success(request);
    }

    private async Task<Result<ProcessedOrder>> CreateShipmentAsync(OrderRequest request)
    {
        await Task.Delay(10);
        return Result<ProcessedOrder>.Success(new ProcessedOrder { OrderId = 1 });
    }

    private async Task SendConfirmationEmailAsync(ProcessedOrder order)
    {
        await Task.Delay(10);
        Console.WriteLine($"Confirmation email sent for order {order.OrderId}");
    }

    private async Task LogOrderAsync(ProcessedOrder order)
    {
        await Task.Delay(10);
    }

    private async Task<ExchangeRate> ParseExchangeRateAsync(string from, string to)
    {
        await Task.Delay(10);
        return new ExchangeRate { From = from, To = to, Rate = 1.25m };
    }

    private async Task<Result<Configuration>> LoadFromRemoteAsync()
    {
        await Task.Delay(10);
        return Result<Configuration>.Failure("REMOTE_UNAVAILABLE", "Remote server unavailable");
    }

    private async Task<Result<Configuration>> LoadFromDatabaseAsync()
    {
        await Task.Delay(10);
        return Result<Configuration>.Failure("DB_ERROR", "Database connection failed");
    }

    private async Task<Result<Configuration>> LoadFromFileAsync()
    {
        await Task.Delay(10);
        return Result<Configuration>.Failure("FILE_NOT_FOUND", "Config file not found");
    }

    private Configuration GetDefaultConfiguration()
    {
        return new Configuration { Setting1 = "default" };
    }

    public static void RunExamples()
    {
        Console.WriteLine("=== Async Examples ===\n");

        var examples = new AsyncExamples();

        // Example 1: Basic async operation
        Console.WriteLine("1. Fetch Weather Data:");
        var weatherTask = examples.GetWeatherAsync("Vancouver");
        weatherTask.Wait();
        var weatherResult = weatherTask.Result;
        if (weatherResult.IsSuccess)
            Console.WriteLine($"   Temperature: {weatherResult.Value?.Temperature}°C");
        Console.WriteLine();

        // Example 2: Parallel operations
        Console.WriteLine("2. Dashboard Data (Parallel):");
        var dashboardTask = examples.GetDashboardDataAsync(1);
        dashboardTask.Wait();
        var dashboardResult = dashboardTask.Result;
        Console.WriteLine($"   Success: {dashboardResult.IsSuccess}");
        if (dashboardResult.IsSuccess)
            Console.WriteLine($"   User: {dashboardResult.Value?.User.Name}");
        Console.WriteLine();

        // Example 3: Retry logic
        Console.WriteLine("3. Retry Logic:");
        var retryTask = examples.RetryAsync(async () =>
        {
            // Simulate a flaky operation
            return await Task.FromResult(Result<string>.Success("Success after retry"));
        });
        retryTask.Wait();
        Console.WriteLine($"   Result: {retryTask.Result.IsSuccess}");
        Console.WriteLine();

        // Example 4: Fallback chain
        Console.WriteLine("4. Configuration Fallback:");
        var configTask = examples.LoadConfigurationAsync();
        configTask.Wait();
        var configResult = configTask.Result;
        Console.WriteLine($"   Success: {configResult.IsSuccess}");
        if (configResult.IsSuccess)
            Console.WriteLine($"   Setting1: {configResult.Value?.Setting1}");
        Console.WriteLine();
    }
}

// Supporting classes
public class WeatherData
{
    public string City { get; set; } = string.Empty;
    public double Temperature { get; set; }
}

public class UserStats
{
    public int TotalOrders { get; set; }
}

public class DashboardData
{
    public User User { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public UserStats Stats { get; set; } = new();
}

public class OrderRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ProcessedOrder
{
    public int OrderId { get; set; }
}

public class ExchangeRate
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}

public class BatchResult<T>
{
    public List<T> Successful { get; set; } = new();
    public List<Error> Failed { get; set; } = new();
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class Configuration
{
    public string Setting1 { get; set; } = string.Empty;
}
