# SharpResults: Robust and Expressive Error Handling for .NET

<div align="center">

![SharpResults Logo](https://img.shields.io/badge/SharpResults-Type--Safe%20Error%20Handling-blue)
[![NuGet](https://img.shields.io/nuget/v/SharpResults.svg)](https://www.nuget.org/packages/SharpResults/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Downloads](https://img.shields.io/nuget/dt/SharpResults.svg)](https://www.nuget.org/packages/SharpResults/)
[![Build Status](https://github.com/your-username/SharpResults/actions/workflows/publish.yml/badge.svg)](https://github.com/your-username/SharpResults/actions/workflows/publish.yml)

</div>

**SharpResults** is a lightweight, zero-dependency C# library that brings the power of functional-style error handling to your .NET projects. It provides `Result<T, TError>` and `Option<T>` types, enabling you to write more robust, predictable, and readable code by making success/failure and presence/absence states explicit. Move away from exception-driven control flow and embrace a more expressive way to handle errors.

---

## 🚀 Key Features

- 🛡️ **Type-Safe Errors**: Eliminate runtime surprises by representing errors in the type system.
- ⛓️ **Fluent & Chainable API**: Seamlessly compose complex operations with a rich set of extension methods like `Map`, `Then`, `Match`, and `OrElse`.
- 🧩 **Full LINQ Integration**: Use `Result` and `Option` types directly within C# query syntax for elegant data manipulation.
- ⚡ **First-Class Async Support**: Built-in `async/await` support for all major operations (`MapAsync`, `ThenAsync`, etc.).
- 🎯 **Modern .NET**: Takes advantage of the latest C# and .NET features for a clean and performant API.
- 📦 **Easy Integration**: Simple to drop into any existing project.
-  **JSON Serialization**: Built-in converters for `System.Text.Json`.
- 🪶 **Lightweight**: No external dependencies.

## 📦 Installation

Install SharpResults easily via NuGet.

**Package Manager Console:**
```powershell
Install-Package SharpResults
```

**.NET CLI:**
```bash
dotnet add package SharpResults
```

---

## Core Concepts

### `Result<T, TError>`: Handling Success or Failure

The `Result` type represents an operation that can succeed with a value of type `T` or fail with an error of type `TError`.

- **`Ok(T)`**: Represents a successful result.
- **`Err(TError)`**: Represents a failure.

This approach forces you to handle potential errors at compile time, preventing unexpected runtime exceptions.

### `Option<T>`: Handling Presence or Absence

The `Option` type represents a value that may or may not be present.

- **`Some(T)`**: Represents the presence of a value.
- **`None`**: Represents the absence of a value.

This is a robust alternative to using `null`, eliminating `NullReferenceException` errors and making your code's intent clearer.

---

## 📖 Usage Examples

### 1. Basic `Result` Usage

Consider a function that might fail, like parsing a number from a string. Instead of throwing an exception, it can return a `Result`.

```csharp
using SharpResults;

public Result<int, string> ParseInteger(string input)
{
    if (int.TryParse(input, out int value))
    {
        return Result.Ok<int, string>(value); // Or simply Result.Ok(value) with type inference or return value directly using implicit conversion
    }
    return Result.Err<int, string>($"'{input}' is not a valid integer.");
}

// --- Usage ---
var result = ParseInteger("123");

result.Match(
    ok: value => Console.WriteLine($"Success: {value}"),
    err: error => Console.WriteLine($"Error: {error}")
);
// Output: Success: 123

var failedResult = ParseInteger("abc");

if (failedResult.IsErr)
{
    Console.WriteLine(failedResult.UnwrapErr());
    // Output: 'abc' is not a valid integer.
}
```

### 2. Chaining Operations with `Result`

`Result` provides a fluent API for chaining operations. The chain continues as long as results are `Ok`; otherwise, it short-circuits and propagates the `Err`.

```csharp
public Result<double, string> GetDiscountedPrice(string userId, string productId)
{
    return GetUser(userId)
        .Then(user => GetProduct(productId).Map(product => (user, product)))
        .Then(data => CalculateDiscount(data.user, data.product))
        .Map(discount => ApplyDiscount(GetProduct(productId).Unwrap(), discount));
}

// Dummy methods for demonstration
private Result<User, string> GetUser(string id) => Result.Ok(new User { Id = id });
private Result<Product, string> GetProduct(string id) => Result.Ok(new Product { Price = 100.0 });
private Result<double, string> CalculateDiscount(User user, Product product) => Result.Ok(0.1); // 10% discount
private double ApplyDiscount(Product product, double discount) => product.Price * (1 - discount);
```

### 3. Basic `Option` Usage

Use `Option` for functions that may not return a value, like finding an item in a collection.

```csharp
using SharpResults;

public Option<User> FindUserById(string id)
{
    // In a real app, you would query a database.
    var user = _users.FirstOrDefault(u => u.Id == id);
    return Option.Create(user); // Creates Some(user) if not null, otherwise None
}

// --- Usage ---
var option = FindUserById("user-123");

option.Match(
    some: user => Console.WriteLine($"Found user: {user.Name}"),
    none: () => Console.WriteLine("User not found.")
);

// Or use it with a default value
var userName = option.Map(user => user.Name).UnwrapOr("Guest");
```

### 4. Asynchronous Operations

SharpResults provides seamless async support.

```csharp
public async Task<Result<string, string>> GetUserDataAsync(string url)
{
    return await Result.Try(async () =>
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        return response;
    })
    .MapErr(ex => ex.Message); // Map the exception to a simple string error
}

// --- Usage ---
var result = await GetUserDataAsync("https://api.example.com/data");
result.Match(
    ok: data => Console.WriteLine("Data fetched!"),
    err: error => Console.WriteLine($"API Error: {error}")
);
```

---

## API Reference

### `Result<T, TError>`

- **Constructors:** `Result.Ok(T)`, `Result.Err(TError)`
- **Properties:** `IsOk`, `IsErr`
- **Methods:** `Match`, `Unwrap`, `UnwrapErr`, `UnwrapOr`, `UnwrapOrElse`, `Map`, `MapErr`, `Then`, `OrElse`, `Flatten`

### `Option<T>`

- **Constructors:** `Option.Some(T)`, `Option.None<T>()`, `Option.Create(T?)`
- **Properties:** `IsSome`, `IsNone`
- **Methods:** `Match`, `Unwrap`, `UnwrapOr`, `UnwrapOrElse`, `Map`, `Then`, `OrElse`, `Flatten`, `Filter`

### Extension Methods

A rich set of extension methods are provided for both `Result` and `Option` to enable a fluent and expressive API. These include methods for asynchronous operations, LINQ support, and collections.

---

## Comparison with Other Libraries

The existing comparison tables are excellent and have been preserved here.

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

---

## Contributing

Contributions are welcome! Here's how you can contribute:

1.  **Fork the repository**
2.  **Create a feature branch**: `git checkout -b feature/amazing-feature`
3.  **Commit your changes**: `git commit -m 'Add some amazing feature'`
4.  **Push to the branch**: `git push origin feature/amazing-feature`
5.  **Open a Pull Request**

### Development Guidelines

-   Follow the existing code style and conventions.
-   Add unit tests for new features.
-   Update documentation for any changes.
-   Ensure all tests pass before submitting a PR.

## License

This project is licensed under the MIT License - see the `LICENSE` file for details.