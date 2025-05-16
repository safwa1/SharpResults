# SharpResults

A lightweight, zero-dependency C# library that implements the Result and Option types for more explicit and type-safe error handling. SharpResults helps you avoid exceptions for control flow and makes success/failure and presence/absence states explicit in your code.

<div align="center">

![SharpResults Logo](https://img.shields.io/badge/SharpResults-Type--Safe%20Error%20Handling-blue)
[![NuGet](https://img.shields.io/nuget/v/SharpResults.svg)](https://www.nuget.org/packages/SharpResults/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

</div>

## 🚀 Features

- 🛡️ **Type-safe error handling** - No more unexpected exceptions
- 🔄 **Chainable operations** - Fluent API for transforming and combining results
- 🧩 **LINQ integration** - Works with C# query syntax
- ⚡ **Async support** - First-class support for async/await operations
- 🔍 **Comprehensive API** - Rich set of methods for working with results
- 🪶 **Lightweight** - Zero dependencies
- 🎯 **Multi-targeting** - Supports .NET 8.0 and .NET 9.0
- 📊 **Performance optimized** - Minimal overhead compared to traditional exception handling

## 📋 Table of Contents

- [Installation](#-installation)
- [Basic Usage](#basic-usage)
- [Advanced Usage](#advanced-usage)
- [Asynchronous Operations](#asynchronous-operations)
- [Real-World Examples](#real-world-examples)
- [Performance Considerations](#performance-considerations)
- [Comparison with Other Libraries](#comparison-with-other-libraries)
- [Contributing](#contributing)
- [License](#license)

## 📦 Installation

### Package Manager

```
Install-Package SharpResults
```

### .NET CLI

```
dotnet add package SharpResults
```

## Basic Usage

### Creating Results

```csharp
using SharpResults;
using SharpResults.Types;

// Create a successful result
Result<int> success = Result.Ok(42);

// Create a failed result with an exception
Result<int> failure = Result.Err<int>(new Exception("Something went wrong"));

// Create a failed result with just a message
Result<string> failureWithMessage = Result.Err<string>("Invalid input");

// Implicit conversion from value to successful result
Result<int> implicitSuccess = 42;
```

### Checking Result State

```csharp
Result<int> result = GetSomeResult();

// Using properties
if (result.IsOk)
{
    // Use result.Value safely here
    Console.WriteLine($"Got value: {result.Value}");
}
else
{
    // Handle the error
    Console.WriteLine($"Error: {result.Exception.Message}");
}

// Using pattern matching (C# 8.0+)
switch (result)
{
    case { IsOk: true } ok:
        Console.WriteLine($"Success: {ok.Value}");
        break;
    case { IsErr: true } err:
        Console.WriteLine($"Error: {err.Exception.Message}");
        break;
}

// Using deconstruction syntax
var (isOk, value, exception) = result;
if (isOk)
{
    Console.WriteLine($"Value: {value}");
}
```

### Transforming Results

```csharp
// Map transforms the value inside a successful result
Result<int> result = Result.Ok(42);
Result<string> mapped = result.Map(x => x.ToString());

// AndThen (or FlatMap) chains result-returning operations
Result<int> parsed = Result.Ok("42")
    .AndThen(str => {
        if (int.TryParse(str, out int value))
            return Result.Ok(value);
        return Result.Err<int>("Parsing failed");
    });

// LINQ syntax support
Result<int> computation = 
    from x in Result.Ok(10)
    from y in Result.Ok(5)
    select x + y; // Result.Ok(15)
```

### Handling Errors

```csharp
// Recover from errors
Result<int> recovered = Result.Err<int>("Original error")
    .OrElse(ex => Result.Ok(42));

// Provide default values
int value = Result.Err<int>("Error")
    .UnwrapOr(42); // 42

// Use a fallback function
int computed = Result.Err<int>(new Exception("Failed"))
    .UnwrapOrElse(ex => 42); // 42

// Map errors to different error types
Result<int, CustomError> mappedError = Result.Err<int>("Database error")
    .MapErr(msg => new CustomError(ErrorType.Database, msg));
```

### Pattern Matching with Match

```csharp
// Transform both success and error cases
string message = result.Match(
    ok: value => $"Success: {value}",
    err: ex => $"Error: {ex.Message}"
);

// Execute actions based on result state
result.Match(
    ok: value => SaveToDatabase(value),
    err: ex => LogError(ex)
);
```

## Advanced Usage

### Custom Error Types

Use `Result<T, TError>` when you want to use custom error types instead of exceptions:

```csharp
// Define a custom error type
public enum ApiError
{
    NotFound,
    Unauthorized,
    ServerError
}

// Create results with custom error types
Result<User, ApiError> GetUser(int id)
{
    if (id <= 0)
        return ApiError.NotFound; // Implicit conversion
    
    if (!IsAuthenticated())
        return Result.Err<User, ApiError>(ApiError.Unauthorized);
    
    try
    {
        var user = _repository.GetUser(id);
        return user != null 
            ? Result.Ok<User, ApiError>(user)
            : ApiError.NotFound;
    }
    catch
    {
        return ApiError.ServerError;
    }
}

// Usage
var result = GetUser(42);

// Pattern matching with custom errors
string message = result.Match(
    ok: user => $"Found user: {user.Name}",
    err: error => error switch
    {
        ApiError.NotFound => "User not found",
        ApiError.Unauthorized => "Please login first",
        ApiError.ServerError => "Server error occurred",
        _ => "Unknown error"
    }
);
```

### Working with Void Returns

Use `Unit` as a return type for operations that don't return a value:

```csharp
// For methods that don't return a value
Result<Unit> SaveData(string data)
{
    try
    {
        // Save data to database
        File.WriteAllText("data.txt", data);
        return Result.Ok(Unit.Value);
    }
    catch (Exception ex)
    {
        return Result.Err<Unit>(ex);
    }
}

// Usage
Result<Unit> saveResult = SaveData("important data");
if (saveResult.IsOk)
{
    Console.WriteLine("Data saved successfully");
}
```

### Try-Catch Alternative

Use `Result.From` to automatically catch exceptions:

```csharp
// Automatically catches exceptions
Result<int> divideResult = Result.From(() => 10 / 0);
// divideResult will be an Err with DivideByZeroException

// For void-returning methods
Result<Unit> writeResult = Result.From(() => File.WriteAllText("file.txt", "content"));

// With custom error mapping
Result<int, string> customErrorResult = Result.From<int, string>(
    () => ParseComplexData(),
    ex => $"Parsing error: {ex.Message}"
);
```

### Chaining Multiple Operations

```csharp
Result<string> GetProcessedData(string input)
{
    return Result.Ok(input)
        .AndThen(ValidateInput)
        .Map(data => data.ToUpper())
        .AndThen(ProcessData)
        .Inspect(data => Console.WriteLine($"Processing complete: {data}"))
        .InspectErr(ex => Console.WriteLine($"Error occurred: {ex.Message}"));
}

Result<string> ValidateInput(string input)
{
    return string.IsNullOrEmpty(input)
        ? Result.Err<string>("Input cannot be empty")
        : Result.Ok(input);
}

Result<string> ProcessData(string data)
{
    // Process the data
    return Result.Ok($"Processed: {data}");
}
```

### Combining Multiple Results

```csharp
// Combine multiple results into one
Result<(int, string, bool)> combined = Result.Combine(
    Result.Ok(42),
    Result.Ok("hello"),
    Result.Ok(true)
);

// If any result is an error, the combined result will be an error
Result<(int, string)> partialError = Result.Combine(
    Result.Ok(42),
    Result.Err<string>("Something went wrong")
); // Will be Err with "Something went wrong"

// Combine a collection of results
IEnumerable<Result<int>> results = new[] { Result.Ok(1), Result.Ok(2), Result.Ok(3) };
Result<IEnumerable<int>> collectionResult = Result.Combine(results);
// Result.Ok([1, 2, 3])
```

## Asynchronous Operations

SharpResults provides first-class support for asynchronous operations:

```csharp
// Async methods returning Result
public async Task<Result<User>> GetUserAsync(int id)
{
    try
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null
            ? Result.Ok(user)
            : Result.Err<User>("User not found");
    }
    catch (Exception ex)
    {
        return Result.Err<User>(ex);
    }
}

// Chaining async operations
public async Task<Result<OrderConfirmation>> PlaceOrderAsync(int userId, int productId)
{
    return await GetUserAsync(userId)
        .MapAsync(async user => {
            // Validate user can place orders
            if (!user.CanPlaceOrders)
                return Result.Err<User>("User cannot place orders");
            return Result.Ok(user);
        })
        .AndThenAsync(async user => await GetProductAsync(productId))
        .AndThenAsync(async product => {
            if (!product.InStock)
                return Result.Err<OrderConfirmation>("Product out of stock");
                
            var confirmation = await _orderService.CreateOrderAsync(userId, productId);
            return Result.Ok(confirmation);
        });
}

// Using LINQ with async results
public async Task<Result<OrderSummary>> GetOrderSummaryAsync(int orderId)
{
    var orderResult = await GetOrderAsync(orderId);
    
    return await (from order in orderResult
                 from customer in await GetCustomerAsync(order.CustomerId)
                 from items in await GetOrderItemsAsync(orderId)
                 select new OrderSummary
                 {
                     OrderId = order.Id,
                     CustomerName = customer.Name,
                     Items = items,
                     Total = items.Sum(i => i.Price)
                 });
}
```

## Real-World Examples

### Web API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserAsync(id);
        
        return result.Match<IActionResult>(
            ok: user => Ok(user),
            err: ex => ex.Message == "User not found" 
                ? NotFound(ex.Message) 
                : StatusCode(500, "An error occurred while processing your request")
        );
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        
        return result.Match<IActionResult>(
            ok: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
            err: ex => BadRequest(ex.Message)
        );
    }
}
```

### Domain Logic with Validation

```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    
    // Constructor with DI
    
    public async Task<Result<Order>> PlaceOrderAsync(OrderRequest request)
    {
        // Validate request
        var validationResult = ValidateOrderRequest(request);
        if (validationResult.IsErr)
            return Result.Err<Order>(validationResult.Exception);
            
        // Check if customer exists
        var customerResult = await _customerRepository.GetByIdAsync(request.CustomerId);
        if (customerResult.IsErr)
            return Result.Err<Order>($"Customer not found: {request.CustomerId}");
            
        // Check if all products exist and are in stock
        var productsResult = await CheckProductsAvailabilityAsync(request.Items);
        if (productsResult.IsErr)
            return productsResult.MapErr<Order>();
            
        // Create order
        var order = new Order
        {
            CustomerId = request.CustomerId,
            Items = request.Items.Select(i => new OrderItem { ProductId = i.ProductId, Quantity = i.Quantity }).ToList(),
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };
        
        // Save order
        try
        {
            await _orderRepository.CreateAsync(order);
            return Result.Ok(order);
        }
        catch (Exception ex)
        {
            return Result.Err<Order>(ex);
        }
    }
    
    private Result<Unit> ValidateOrderRequest(OrderRequest request)
    {
        if (request == null)
            return Result.Err<Unit>("Order request cannot be null");
            
        if (request.CustomerId <= 0)
            return Result.Err<Unit>("Invalid customer ID");
            
        if (request.Items == null || !request.Items.Any())
            return Result.Err<Unit>("Order must contain at least one item");
            
        if (request.Items.Any(i => i.Quantity <= 0))
            return Result.Err<Unit>("All items must have a quantity greater than zero");
            
        return Result.Ok(Unit.Value);
    }
    
    private async Task<Result<Unit>> CheckProductsAvailabilityAsync(IEnumerable<OrderItemRequest> items)
    {
        foreach (var item in items)
        {
            var productResult = await _productRepository.GetByIdAsync(item.ProductId);
            if (productResult.IsErr)
                return Result.Err<Unit>($"Product not found: {item.ProductId}");
                
            var product = productResult.Value;
            if (product.StockQuantity < item.Quantity)
                return Result.Err<Unit>($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
        }
        
        return Result.Ok(Unit.Value);
    }
}
```

## Performance Considerations

SharpResults is designed to be lightweight and efficient. Here are some performance considerations:

- **Avoid exceptions for control flow**: Using SharpResults instead of throwing exceptions can significantly improve performance in error-prone code paths.
- **Lazy error handling**: Error messages and exceptions are only created when needed.
- **Minimal allocations**: The library minimizes heap allocations where possible.
- **Struct-based implementation**: For performance-critical code, consider using the struct-based variants of Result types.

## Comparison with Other Libraries

### ⚖️ TL;DR — Summary

| Feature / Library | SharpResults | FluentResults | OneOf | CSharpFunctionalExtensions |
|------------------|----------------------|---------------|-------|-----------------------------|
| ✅ Strong Result/Error typing | ✔️ Yes | ✔️ Yes | ❌ No | ✔️ Yes |
| ✅ Rich extension methods | ✔️ Yes | ✔️ Partial | ❌ Minimal | ✔️ Yes |
| ✅ Exception capturing | ✔️ Yes | ✔️ Yes | ❌ No | ✔️ Yes |
| ✅ Pattern matching support | ✔️ With Match | ✔️ With ResultType | ✔️ Native | ❌ Manual |
| ✅ Null safety / value handling | ✔️ Yes | ✔️ Yes | ⚠️ Riskier | ✔️ Yes |
| ✅ Simplicity & minimalism | ✔️ Lean and clean | ❌ Heavy | ✔️ Minimal | ❌ Verbose |
| ✅ Control over error types | ✔️ Generic errors (TError) | ✔️ Message objects | ❌ Not applicable | ✔️ Generic |
| 🛠️ IDE-friendliness (C# tooling) | ✔️ Yes | ✔️ Yes | ⚠️ Limited | ✔️ Yes |

### Detailed Feature Comparison

| Feature | SharpResults | OneOf | CSharpFunctionalExtensions | FluentResults |
|---------|-------------|-------|----------------------------|---------------|
| Custom Error Types | ✅ | ✅ | ✅ | ✅ |
| LINQ Support | ✅ | ❌ | ✅ | ❌ |
| Async Support | ✅ | ❌ | ✅ | ✅ |
| Pattern Matching | ✅ | ✅ | ❌ | ❌ |
| Implicit Conversions | ✅ | ✅ | ✅ | ❌ |
| Deconstruction | ✅ | ✅ | ✅ | ❌ |
| Multiple Error Collection | ✅ | ❌ | ❌ | ✅ |
| Zero Dependencies | ✅ | ✅ | ✅ | ✅ |
| .NET Standard 2.0+ | ✅ | ✅ | ✅ | ✅ |

## Contributing

Contributions are welcome! Here's how you can contribute:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Commit your changes**: `git commit -m 'Add some amazing feature'`
4. **Push to the branch**: `git push origin feature/amazing-feature`
5. **Open a Pull Request**

### Development Guidelines

- Follow the existing code style and conventions
- Add unit tests for new features
- Update documentation for any changes
- Ensure all tests pass before submitting a PR

## License

This project is licensed under the MIT License - see the LICENSE file for details.