using BYSResults;

namespace BYSResults.Examples;

/// <summary>
/// Examples demonstrating how to use Result types in Web API controllers
/// </summary>
public class WebApiExamples
{
    // Mock user service for demonstration
    private readonly IUserService _userService = new MockUserService();

    /// <summary>
    /// Example: Converting Result to HTTP-style responses
    /// </summary>
    public ApiResponse<User> GetUser(int id)
    {
        var result = _userService.GetUserById(id);

        return result.Match(
            onSuccess: user => new ApiResponse<User> { StatusCode = 200, Data = user },
            onFailure: errors => new ApiResponse<User>
            {
                StatusCode = 404,
                Errors = errors.Select(e => e.Message).ToList()
            }
        );
    }

    /// <summary>
    /// Example: Creating a user with validation
    /// </summary>
    public ApiResponse<User> CreateUser(CreateUserRequest request)
    {
        var result = Result<CreateUserRequest>.Success(request)
            .Ensure(r => !string.IsNullOrEmpty(r.Email), "Email is required")
            .Ensure(r => r.Email.Contains("@"), "Email must be valid")
            .Ensure(r => !string.IsNullOrEmpty(r.Name), "Name is required")
            .Bind(r => _userService.CreateUser(r.Name, r.Email));

        return result.Match(
            onSuccess: user => new ApiResponse<User> { StatusCode = 201, Data = user },
            onFailure: errors => new ApiResponse<User>
            {
                StatusCode = 400,
                Errors = errors.Select(e => e.Message).ToList()
            }
        );
    }

    /// <summary>
    /// Example: Updating a user with partial updates
    /// </summary>
    public ApiResponse<User> UpdateUser(int id, UpdateUserRequest request)
    {
        var result = _userService.GetUserById(id)
            .Ensure(u => u.Id == id, "User ID mismatch")
            .Map(user =>
            {
                if (!string.IsNullOrEmpty(request.Name))
                    user.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Email))
                    user.Email = request.Email;
                return user;
            })
            .Bind(user => _userService.UpdateUser(user));

        return result.Match(
            onSuccess: user => new ApiResponse<User> { StatusCode = 200, Data = user },
            onFailure: errors => new ApiResponse<User>
            {
                StatusCode = errors.Any(e => e.Code == "NOT_FOUND") ? 404 : 400,
                Errors = errors.Select(e => e.Message).ToList()
            }
        );
    }

    /// <summary>
    /// Example: Deleting with authorization check
    /// </summary>
    public ApiResponse<bool> DeleteUser(int id, int currentUserId)
    {
        var result = _userService.GetUserById(id)
            .Ensure(u => u.Id == currentUserId || currentUserId == 1,
                new Error("FORBIDDEN", "You can only delete your own account"))
            .Bind(u => _userService.DeleteUser(u.Id));

        return result.Match(
            onSuccess: _ => new ApiResponse<bool> { StatusCode = 204, Data = true },
            onFailure: errors => new ApiResponse<bool>
            {
                StatusCode = errors.Any(e => e.Code == "FORBIDDEN") ? 403 : 404,
                Errors = errors.Select(e => e.Message).ToList()
            }
        );
    }

    public static void RunExamples()
    {
        Console.WriteLine("=== Web API Examples ===\n");

        var examples = new WebApiExamples();

        // Example 1: Get existing user
        Console.WriteLine("1. Get User (Success):");
        var getResult = examples.GetUser(1);
        Console.WriteLine($"   Status: {getResult.StatusCode}");
        Console.WriteLine($"   User: {getResult.Data?.Name}\n");

        // Example 2: Get non-existent user
        Console.WriteLine("2. Get User (Not Found):");
        var notFoundResult = examples.GetUser(999);
        Console.WriteLine($"   Status: {notFoundResult.StatusCode}");
        Console.WriteLine($"   Errors: {string.Join(", ", notFoundResult.Errors)}\n");

        // Example 3: Create user with validation
        Console.WriteLine("3. Create User (Success):");
        var createResult = examples.CreateUser(new CreateUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com"
        });
        Console.WriteLine($"   Status: {createResult.StatusCode}");
        Console.WriteLine($"   User: {createResult.Data?.Name}\n");

        // Example 4: Create user with invalid data
        Console.WriteLine("4. Create User (Validation Failed):");
        var invalidResult = examples.CreateUser(new CreateUserRequest
        {
            Name = "",
            Email = "invalid-email"
        });
        Console.WriteLine($"   Status: {invalidResult.StatusCode}");
        Console.WriteLine($"   Errors: {string.Join(", ", invalidResult.Errors)}\n");
    }
}

// Supporting classes
public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

public interface IUserService
{
    Result<User> GetUserById(int id);
    Result<User> CreateUser(string name, string email);
    Result<User> UpdateUser(User user);
    Result DeleteUser(int id);
}

public class MockUserService : IUserService
{
    private readonly List<User> _users = new()
    {
        new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
        new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
    };

    public Result<User> GetUserById(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return user != null
            ? Result<User>.Success(user)
            : Result<User>.Failure("NOT_FOUND", "User not found");
    }

    public Result<User> CreateUser(string name, string email)
    {
        var user = new User { Id = _users.Count + 1, Name = name, Email = email };
        _users.Add(user);
        return Result<User>.Success(user);
    }

    public Result<User> UpdateUser(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing == null)
            return Result<User>.Failure("NOT_FOUND", "User not found");

        existing.Name = user.Name;
        existing.Email = user.Email;
        return Result<User>.Success(existing);
    }

    public Result DeleteUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return Result.Failure("NOT_FOUND", "User not found");

        _users.Remove(user);
        return Result.Success();
    }
}
