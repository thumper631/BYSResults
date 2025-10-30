using BYSResults;

namespace BYSResults.Examples;

/// <summary>
/// Examples demonstrating railway-oriented programming and business logic chaining
/// </summary>
public class ChainingExamples
{
    /// <summary>
    /// Example: Simple railway-oriented pipeline
    /// </summary>
    public Result<int> CalculateTotalPrice(OrderInput input)
    {
        return Result<OrderInput>.Success(input)
            .Ensure(i => i.Quantity > 0, "Quantity must be positive")
            .Ensure(i => i.Price >= 0, "Price cannot be negative")
            .Map(i => i.Quantity * i.Price)
            .Tap(total => Console.WriteLine($"Subtotal: ${total}"))
            .Bind(subtotal => ApplyDiscount(subtotal, input.DiscountPercent))
            .Tap(finalPrice => Console.WriteLine($"Final price: ${finalPrice}"));
    }

    /// <summary>
    /// Example: Complex business workflow with multiple stages
    /// </summary>
    public Result<CompletedOrder> ProcessCustomerOrder(CustomerOrderInput input)
    {
        return Result<CustomerOrderInput>.Success(input)
            // Stage 1: Validation
            .Ensure(i => i.CustomerId > 0, "Invalid customer ID")
            .Ensure(i => i.Items.Any(), "Order must contain items")
            .Tap(_ => Console.WriteLine("✓ Input validated"))

            // Stage 2: Customer verification
            .Bind(i => VerifyCustomer(i.CustomerId))
            .Tap(customer => Console.WriteLine($"✓ Customer verified: {customer.Name}"))

            // Stage 3: Inventory check
            .Bind(customer => CheckInventory(input.Items)
                .Map(_ => customer))
            .Tap(_ => Console.WriteLine("✓ Inventory available"))

            // Stage 4: Calculate pricing
            .Map(customer => new { customer, total = CalculateTotal(input.Items) })
            .Tap(data => Console.WriteLine($"✓ Total calculated: ${data.total}"))

            // Stage 5: Payment processing
            .Bind(data => ProcessPayment(data.customer.Id, data.total)
                .Map(payment => new { data.customer, data.total, payment }))
            .Tap(_ => Console.WriteLine("✓ Payment processed"))

            // Stage 6: Create order record
            .Map(data => new CompletedOrder
            {
                OrderId = GenerateOrderId(),
                CustomerId = data.customer.Id,
                CustomerName = data.customer.Name,
                TotalAmount = data.total,
                PaymentId = data.payment.TransactionId,
                Status = "Completed"
            })
            .Tap(order => Console.WriteLine($"✓ Order created: {order.OrderId}"));
    }

    /// <summary>
    /// Example: Branching logic with OnSuccess and OnFailure
    /// </summary>
    public Result<string> ProcessWithRecovery(TransactionInput input)
    {
        return Result<TransactionInput>.Success(input)
            .Ensure(i => i.Amount > 0, "Amount must be positive")
            .Bind(i => ProcessPrimaryPayment(i))
            .OnFailure(errors =>
            {
                Console.WriteLine("Primary payment failed, trying backup...");
                return ProcessBackupPayment(input);
            })
            .OnSuccess(result =>
            {
                Console.WriteLine("Payment successful, sending notification...");
                SendNotification(result.Value!);
                return result;
            });
    }

    /// <summary>
    /// Example: Aggregating multiple operations
    /// </summary>
    public Result<AccountSummary> CreateAccountSummary(int accountId)
    {
        var balanceResult = GetAccountBalance(accountId);
        var transactionsResult = GetRecentTransactions(accountId);
        var limitsResult = GetAccountLimits(accountId);

        var combined = Result.Combine(balanceResult, transactionsResult, limitsResult);

        return combined.IsSuccess
            ? Result<AccountSummary>.Success(new AccountSummary
            {
                AccountId = accountId,
                Balance = balanceResult.Value,
                RecentTransactions = transactionsResult.Value!,
                Limits = limitsResult.Value!
            })
            : Result<AccountSummary>.Failure(combined.Errors);
    }

    /// <summary>
    /// Example: Conditional chaining based on intermediate results
    /// </summary>
    public Result<ShippingInfo> CalculateShipping(ShippingRequest request)
    {
        return Result<ShippingRequest>.Success(request)
            .Ensure(r => r.Weight > 0, "Weight must be positive")
            .Map(r => new { request = r, cost = CalculateBaseCost(r.Weight) })
            .Map(data => data.cost > 100
                ? new { data.request, cost = data.cost * 0.9m } // Apply discount for heavy items
                : data)
            .Map(data => data.request.IsExpress
                ? new { data.request, cost = data.cost * 1.5m } // Add express fee
                : data)
            .Map(data => new ShippingInfo
            {
                Cost = data.cost,
                EstimatedDays = data.request.IsExpress ? 1 : 5,
                Carrier = data.request.IsExpress ? "Express Courier" : "Standard Post"
            });
    }

    /// <summary>
    /// Example: Error context enrichment through the pipeline
    /// </summary>
    public Result<UserProfile> LoadUserProfile(int userId)
    {
        return Result<int>.Success(userId)
            .Bind(id => GetUserBasicInfo(id))
            .AddError(new Error("CONTEXT", $"Loading profile for user {userId}"))
            .Bind(user => GetUserPreferences(user.Id)
                .Map(prefs => new { user, prefs }))
            .AddError(new Error("CONTEXT", "Loading user preferences"))
            .Bind(data => GetUserPermissions(data.user.Id)
                .Map(perms => new UserProfile
                {
                    UserId = data.user.Id,
                    Name = data.user.Name,
                    Preferences = data.prefs,
                    Permissions = perms
                }))
            .AddError(new Error("CONTEXT", "Loading user permissions"))
            .TapOnFailure(errors =>
            {
                Console.WriteLine("Failed to load user profile:");
                foreach (var error in errors)
                    Console.WriteLine($"  - {error}");
            });
    }

    /// <summary>
    /// Example: Value transformation chain
    /// </summary>
    public Result<FormattedReport> GenerateReport(ReportRequest request)
    {
        return Result<ReportRequest>.Success(request)
            .Ensure(r => r.StartDate < r.EndDate, "Invalid date range")
            .Map(r => FetchRawData(r))
            .Map(data => FilterData(data, request.Filters))
            .Map(data => AggregateData(data))
            .Map(data => SortData(data, request.SortBy))
            .Map(data => FormatData(data, request.Format))
            .Map(formatted => new FormattedReport
            {
                Data = formatted,
                GeneratedAt = DateTime.UtcNow,
                Format = request.Format
            })
            .Tap(report => Console.WriteLine($"Report generated: {report.Data.Count} rows"));
    }

    /// <summary>
    /// Example: Nested operations with early exit
    /// </summary>
    public Result<ApprovalResult> ProcessApprovalWorkflow(ApprovalRequest request)
    {
        return Result<ApprovalRequest>.Success(request)
            .Ensure(r => r.Amount > 0, "Amount must be positive")
            .Bind(r =>
            {
                // Level 1: Manager approval (auto-approve under $1000)
                if (r.Amount < 1000)
                    return Result<ApprovalRequest>.Success(r);

                return GetManagerApproval(r)
                    .Ensure(approved => approved, "Manager approval required");
            })
            .Bind(r =>
            {
                // Level 2: Director approval (required over $5000)
                if (r.Amount < 5000)
                    return Result<ApprovalRequest>.Success(r);

                return GetDirectorApproval(r)
                    .Ensure(approved => approved, "Director approval required");
            })
            .Bind(r =>
            {
                // Level 3: Board approval (required over $50000)
                if (r.Amount < 50000)
                    return Result<ApprovalRequest>.Success(r);

                return GetBoardApproval(r)
                    .Ensure(approved => approved, "Board approval required");
            })
            .Map(r => new ApprovalResult
            {
                RequestId = r.RequestId,
                Amount = r.Amount,
                Approved = true,
                ApprovedAt = DateTime.UtcNow
            });
    }

    // Helper methods
    private Result<int> ApplyDiscount(int price, int discountPercent)
    {
        if (discountPercent < 0 || discountPercent > 100)
            return Result<int>.Failure("INVALID_DISCOUNT", "Discount must be between 0 and 100");

        return Result<int>.Success(price - (price * discountPercent / 100));
    }

    private Result<Customer> VerifyCustomer(int customerId)
    {
        return Result<Customer>.Success(new Customer { Id = customerId, Name = "John Doe" });
    }

    private Result CheckInventory(List<OrderItemInput> items)
    {
        return Result.Success();
    }

    private decimal CalculateTotal(List<OrderItemInput> items)
    {
        return items.Sum(i => i.Price * i.Quantity);
    }

    private Result<PaymentResult> ProcessPayment(int customerId, decimal amount)
    {
        return Result<PaymentResult>.Success(new PaymentResult { TransactionId = "TXN123" });
    }

    private int GenerateOrderId()
    {
        return new Random().Next(1000, 9999);
    }

    private Result<string> ProcessPrimaryPayment(TransactionInput input)
    {
        return Result<string>.Failure("PRIMARY_FAILED", "Primary payment processor unavailable");
    }

    private Result<string> ProcessBackupPayment(TransactionInput input)
    {
        return Result<string>.Success("BACKUP_TXN_123");
    }

    private void SendNotification(string transactionId)
    {
        Console.WriteLine($"Notification sent for transaction: {transactionId}");
    }

    private Result<decimal> GetAccountBalance(int accountId)
    {
        return Result<decimal>.Success(1234.56m);
    }

    private Result<List<Transaction>> GetRecentTransactions(int accountId)
    {
        return Result<List<Transaction>>.Success(new List<Transaction>());
    }

    private Result<AccountLimits> GetAccountLimits(int accountId)
    {
        return Result<AccountLimits>.Success(new AccountLimits { DailyLimit = 5000 });
    }

    private decimal CalculateBaseCost(decimal weight)
    {
        return weight * 2.5m;
    }

    private Result<UserBasicInfo> GetUserBasicInfo(int userId)
    {
        return Result<UserBasicInfo>.Success(new UserBasicInfo { Id = userId, Name = "User" });
    }

    private Result<UserPreferences> GetUserPreferences(int userId)
    {
        return Result<UserPreferences>.Success(new UserPreferences());
    }

    private Result<UserPermissions> GetUserPermissions(int userId)
    {
        return Result<UserPermissions>.Success(new UserPermissions());
    }

    private List<DataRow> FetchRawData(ReportRequest request)
    {
        return new List<DataRow>();
    }

    private List<DataRow> FilterData(List<DataRow> data, string filters)
    {
        return data;
    }

    private List<DataRow> AggregateData(List<DataRow> data)
    {
        return data;
    }

    private List<DataRow> SortData(List<DataRow> data, string sortBy)
    {
        return data;
    }

    private List<DataRow> FormatData(List<DataRow> data, string format)
    {
        return data;
    }

    private Result<bool> GetManagerApproval(ApprovalRequest request)
    {
        return Result<bool>.Success(true);
    }

    private Result<bool> GetDirectorApproval(ApprovalRequest request)
    {
        return Result<bool>.Success(true);
    }

    private Result<bool> GetBoardApproval(ApprovalRequest request)
    {
        return Result<bool>.Success(true);
    }

    public static void RunExamples()
    {
        Console.WriteLine("=== Chaining & Business Logic Examples ===\n");

        var examples = new ChainingExamples();

        // Example 1: Simple calculation pipeline
        Console.WriteLine("1. Price Calculation Pipeline:");
        var priceResult = examples.CalculateTotalPrice(new OrderInput
        {
            Quantity = 5,
            Price = 20,
            DiscountPercent = 10
        });
        Console.WriteLine($"   Final result: {(priceResult.IsSuccess ? $"${priceResult.Value}" : "Failed")}");
        Console.WriteLine();

        // Example 2: Complex order processing
        Console.WriteLine("2. Customer Order Processing:");
        var orderResult = examples.ProcessCustomerOrder(new CustomerOrderInput
        {
            CustomerId = 1,
            Items = new List<OrderItemInput>
            {
                new() { ProductId = 1, Quantity = 2, Price = 50 }
            }
        });
        Console.WriteLine($"   Order status: {(orderResult.IsSuccess ? orderResult.Value?.Status : "Failed")}");
        Console.WriteLine();

        // Example 3: Recovery workflow
        Console.WriteLine("3. Payment with Recovery:");
        var paymentResult = examples.ProcessWithRecovery(new TransactionInput { Amount = 100 });
        Console.WriteLine($"   Result: {(paymentResult.IsSuccess ? "Success" : "Failed")}");
        Console.WriteLine();

        // Example 4: Shipping calculation
        Console.WriteLine("4. Shipping Calculation:");
        var shippingResult = examples.CalculateShipping(new ShippingRequest
        {
            Weight = 5.5m,
            IsExpress = false
        });
        if (shippingResult.IsSuccess)
        {
            Console.WriteLine($"   Cost: ${shippingResult.Value?.Cost:F2}");
            Console.WriteLine($"   Days: {shippingResult.Value?.EstimatedDays}");
        }
        Console.WriteLine();

        // Example 5: Approval workflow
        Console.WriteLine("5. Approval Workflow:");
        var approvalResult = examples.ProcessApprovalWorkflow(new ApprovalRequest
        {
            RequestId = 1,
            Amount = 750
        });
        Console.WriteLine($"   Approved: {approvalResult.IsSuccess}");
        Console.WriteLine();
    }
}

// Supporting classes
public class OrderInput
{
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int DiscountPercent { get; set; }
}

public class CustomerOrderInput
{
    public int CustomerId { get; set; }
    public List<OrderItemInput> Items { get; set; } = new();
}

public class OrderItemInput
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class CompletedOrder
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PaymentResult
{
    public string TransactionId { get; set; } = string.Empty;
}

public class TransactionInput
{
    public decimal Amount { get; set; }
}

public class AccountSummary
{
    public int AccountId { get; set; }
    public decimal Balance { get; set; }
    public List<Transaction> RecentTransactions { get; set; } = new();
    public AccountLimits Limits { get; set; } = new();
}

public class Transaction
{
    public int Id { get; set; }
}

public class AccountLimits
{
    public decimal DailyLimit { get; set; }
}

public class ShippingRequest
{
    public decimal Weight { get; set; }
    public bool IsExpress { get; set; }
}

public class ShippingInfo
{
    public decimal Cost { get; set; }
    public int EstimatedDays { get; set; }
    public string Carrier { get; set; } = string.Empty;
}

public class UserProfile
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public UserPreferences Preferences { get; set; } = new();
    public UserPermissions Permissions { get; set; } = new();
}

public class UserBasicInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserPreferences
{
}

public class UserPermissions
{
}

public class ReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Filters { get; set; } = string.Empty;
    public string SortBy { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}

public class FormattedReport
{
    public List<DataRow> Data { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string Format { get; set; } = string.Empty;
}

public class DataRow
{
}

public class ApprovalRequest
{
    public int RequestId { get; set; }
    public decimal Amount { get; set; }
}

public class ApprovalResult
{
    public int RequestId { get; set; }
    public decimal Amount { get; set; }
    public bool Approved { get; set; }
    public DateTime ApprovedAt { get; set; }
}
