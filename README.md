# SharpResults

<div align="center">

![SharpResults Logo](https://img.shields.io/badge/SharpResults-Type--Safe%20Error%20Handling-blue)
[![NuGet](https://img.shields.io/nuget/v/SharpResults.svg)](https://www.nuget.org/packages/SharpResults/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Downloads](https://img.shields.io/nuget/dt/SharpResults.svg)](https://www.nuget.org/packages/SharpResults/)

</div>

A lightweight C# library for functional-style error handling in .NET. No dependencies, just clean error handling with `Result<T, TError>` and `Option<T>` types.

## Why SharpResults?

Tired of try-catch blocks everywhere? Want to make errors explicit and impossible to ignore? SharpResults helps you write safer code by representing success/failure and presence/absence right in your type signatures. No more hidden exceptions or null reference errors sneaking up on you.

## What's in the box

- Type-safe error handling - your errors are part of the type system
- Chainable operations with `Map`, `AndThen`, `Match`, and friends
- Works great with LINQ queries
- Full async/await support
- Pattern matching and deconstruction
- Implicit conversions for cleaner code
- JSON serialization out of the box
- NumericOption for mathematical operations
- Unit type for void-like operations
- Zero dependencies
- Built for .NET 8+

## Installation

```powershell
Install-Package SharpResults
```

or

```bash
dotnet add package SharpResults
```

## Quick Start

### Result - when things might fail

Instead of throwing exceptions, return a `Result<T, TError>`. It's either `Ok` with a value, or `Err` with an error.

```csharp
using SharpResults;

public Result<int, string> ParseInteger(string input)
{
    if (int.TryParse(input, out int value))
        return value; // Implicit conversion to Ok
    
    return "Not a valid integer"; // Implicit conversion to Err
}

// Use it like this
var result = ParseInteger("123");

result.Match(
    ok: value => Console.WriteLine($"Success: {value}"),
    err: error => Console.WriteLine($"Error: {error}")
);

// Or with pattern matching
var message = result switch
{
    (true, var value, _) => $"Success: {value}",
    (false, _, var error) => $"Error: {error}"
};

// Or check the state directly
if (result.IsOk)
{
    var value = result.Unwrap();
    Console.WriteLine($"Got: {value}");
}
```

### Creating Results

Several ways to create results:

```csharp
// Explicit factory methods
var ok = Result.Ok<int, string>(42);
var err = Result.Err<int, string>("Something went wrong");

// Implicit conversions (cleaner!)
Result<int, string> result1 = 42; // Converts to Ok
Result<int, string> result2 = "Error"; // Converts to Err

// Try pattern - catches exceptions
var result3 = Result.Try(() => int.Parse("123"));
var result4 = await Result.TryAsync(async () => await GetDataAsync());

// From other types
var fromOption = Result.From(someOption);
var fromFunc = Result.From(() => DoSomething());
```

### Working with Results

```csharp
// Extract values safely
var value = result.UnwrapOr(0); // Returns value or default
var value2 = result.UnwrapOrElse(err => err.Length); // Compute default from error
var value3 = result.Expect("Expected a number"); // Throws with custom message

// Check state
if (result.WhenOk(out var val))
    Console.WriteLine($"Got {val}");

if (result.WhenErr(out var error))
    Console.WriteLine($"Error: {error}");

// Deconstruction
var (value, error) = result; // One will be null
```

### Chaining operations

Operations chain together smoothly. If any step fails, the error propagates automatically:

```csharp
public Result<double, string> GetDiscountedPrice(string userId, string productId)
{
    return GetUser(userId)
        .AndThen(user => GetProduct(productId).Map(product => (user, product)))
        .AndThen(data => CalculateDiscount(data.user, data.product))
        .Map(discount => ApplyDiscount(productPrice, discount));
}

// Or with LINQ syntax
var result = from user in GetUser(userId)
             from product in GetProduct(productId)
             from discount in CalculateDiscount(user, product)
             select ApplyDiscount(product.Price, discount);
```

### Option - when values might not exist

Use `Option<T>` instead of null. It's either `Some(value)` or `None`.

```csharp
public Option<User> FindUserById(string id)
{
    var user = _users.FirstOrDefault(u => u.Id == id);
    return Option.Create(user); // Some(user) if found, None otherwise
}

// Or use implicit conversion
Option<int> opt = 42; // Becomes Some(42)

// Usage
var option = FindUserById("user-123");

option.Match(
    some: user => Console.WriteLine($"Found user: {user.Name}"),
    none: () => Console.WriteLine("User not found.")
);

// Provide defaults easily
var userName = option.Map(user => user.Name).UnwrapOr("Guest");

// Pattern matching
if (option.WhenSome(out var user))
    Console.WriteLine($"Found: {user.Name}");

// Deconstruction (NET8+)
if (option is (true, var value))
    Console.WriteLine($"Got: {value}");

// Filter and chain
var adminOption = FindUserById("123")
    .Filter(u => u.IsAdmin)
    .Map(u => u.Name);
```

### NumericOption - Options with math

For numeric types, use `NumericOption<T>` to perform mathematical operations:

```csharp
NumericOption<int> a = 5;
NumericOption<int> b = 10;

var sum = a + b; // Some(15)
var product = a * b; // Some(50)

NumericOption<int> none = NumericOption<int>.None;
var invalid = a + none; // None - operations with None produce None

// Numeric checks
bool isPositive = NumericOption.IsPositive(a); // true
bool isEven = NumericOption.IsEvenInteger(b); // true

// Parse numbers safely
var parsed = NumericOption<int>.Parse("123"); // Some(123)
var failed = NumericOption<int>.Parse("abc"); // None

// Convert between types
Option<int> regularOption = myNumericOption; // Implicit conversion
```

### Unit type - for void-like operations

When you want to return `Result` from a void method:

```csharp
public Result<Unit, string> SaveData(string data)
{
    try
    {
        File.WriteAllText("data.txt", data);
        return Unit.Default; // Success with no value
    }
    catch (Exception ex)
    {
        return ex.Message; // Error
    }
}

// Or use Result.From for actions
var result = Result.From(() => File.Delete("temp.txt"));
```

### Async support

Everything works seamlessly with async/await:

```csharp
public async Task<Result<string, string>> GetUserDataAsync(string url)
{
    return await Result.TryAsync(async () =>
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(url);
    })
    .MapErr(ex => ex.Message);
}

// Async transformations for Result
var result = await GetUserAsync(id)
    .MapAsync(async user => await GetProfileAsync(user))
    .AndThenAsync(async profile => await EnrichAsync(profile))
    .MapOrElseAsync(
        mapper: async data => await FormatAsync(data),
        defaultFactory: async err => await GetDefaultAsync()
    );

// Async transformations for Option
var option = await FindUserAsync(id)
    .MapAsync(async user => await user.GetNameAsync())
    .AndThenAsync(async name => await ValidateAsync(name))
    .OrElseAsync(async () => await GetDefaultNameAsync());

// Async with bool extensions
var result = await isValid.ThenAsync(async () => await LoadDataAsync());
var option = await hasPermission.ThenSomeAsync(GetUserDataAsync());

// Work with async sequences
await foreach (var value in asyncOptions.ValuesAsync())
{
    Console.WriteLine(value);
}

var firstAsync = await asyncSequence.FirstOrNoneAsync();
var filteredAsync = await asyncSequence.FirstOrNoneAsync(x => x > 10);

// Async collection operations for Results
await foreach (var success in asyncResults.ValuesAsync())
{
    ProcessSuccess(success);
}

await foreach (var error in asyncResults.ErrorsAsync())
{
    LogError(error);
}
```

### LINQ Integration

Use results and options in LINQ queries:

```csharp
// Query syntax
var result = from user in GetUser(id)
             from orders in GetOrders(user.Id)
             from total in CalculateTotal(orders)
             select total;

// Method syntax
var names = users
    .Select(u => FindUserName(u.Id))
    .WhereSome() // Filter out None values
    .ToList();

// Working with collections
var results = ids
    .Select(id => GetUser(id))
    .Collect(); // Result<List<User>, TError>
```

### Collection helpers

Work with collections safely:

```csharp
// Safe LINQ operations
var first = users.FirstOrNone(); // Option<User>
var last = users.LastOrNone(u => u.IsActive);
var single = users.SingleOrNone(u => u.Id == id);
var element = users.ElementAtOrNone(5);

// Dictionary operations
var value = dict.GetValueOrNone(key); // Option<TValue>

// Stack and Queue operations
var peeked = stack.PeekOrNone(); // Doesn't remove
var popped = stack.PopOrNone(); // Removes and returns
var dequeued = queue.DequeueOrNone();

// PriorityQueue
var next = priorityQueue.PeekOrNone<Task, int>(); // (Task, Priority)
var task = priorityQueue.DequeueOrNone<Task, int>();

// Concurrent collections (thread-safe)
var item = concurrentBag.TakeOrNone();
var value = concurrentStack.PopOrNone();

// Sets
var found = hashSet.GetValueOrNone(searchValue);
var item = sortedSet.GetValueOrNone(value);

// SelectWhere - transform and filter in one pass
var results = items
    .SelectWhere(x => x > 0 ? Option.Some(x * 2) : Option.None<int>());

// Extract all Some values from a sequence
var values = options.Values(); // IEnumerable<T>

// Sequence - convert list of Options to Option of list
var allOrNone = options.Sequence(); // Option<IEnumerable<T>>
var listOrNone = options.SequenceList(); // Option<List<T>>
```

### Bool extensions

Use booleans to create Options:

```csharp
// Execute function conditionally
var result = isValid.Then(() => ProcessData()); // Option<T>

// Create Some/None based on condition  
var option = hasPermission.ThenSome(userData); // Some(userData) or None
```

### JSON extensions

Work with System.Text.Json safely:

```csharp
using SharpResults.Extensions;

// JsonValue
var number = jsonValue.GetOption<int>(); // Option<int>

// JsonObject
var name = jsonObj.GetPropValue<string>("name"); // Option<string>
var node = jsonObj.GetPropOption("address"); // Option<JsonNode>

// JsonElement
var prop = jsonElement.GetPropOption("field"); // Option<JsonElement>
var value = jsonElement.GetPropOption("id".AsSpan());
```

### JSON Serialization

All SharpResults types work seamlessly with System.Text.Json:

```csharp
using System.Text.Json;

// Option serialization
var option = Option.Some(42);
var json = JsonSerializer.Serialize(option); // "42"
var none = Option.None<int>();
var jsonNone = JsonSerializer.Serialize(none); // "null"

// Result serialization
var ok = Result.Ok<int, string>(42);
var jsonOk = JsonSerializer.Serialize(ok); // {"ok":42}

var err = Result.Err<int, string>("Error message");
var jsonErr = JsonSerializer.Serialize(err); // {"err":"Error message"}

// NumericOption serialization (.NET 7+)
NumericOption<int> numOpt = 100;
var jsonNum = JsonSerializer.Serialize(numOpt); // "100"

// Unit serialization
var unit = Unit.Default;
var jsonUnit = JsonSerializer.Serialize(unit); // "null"

// Deserialization works automatically
var deserializedOpt = JsonSerializer.Deserialize<Option<int>>("42");
var deserializedResult = JsonSerializer.Deserialize<Result<int, string>>("""{"ok":42}""");
```

Built-in JSON converters handle all the serialization automatically - no configuration needed!

### Result collection operations

```csharp
// Extract all Ok values
var successes = results.Values(); // IEnumerable<T>

// Extract all Err values
var failures = results.Errors(); // IEnumerable<TError>

// Inspect without consuming
var result = GetData()
    .Inspect(data => Console.WriteLine($"Got: {data}"))
    .InspectErr(err => Logger.Error(err));

// Check contents
if (result.Contains(expectedValue))
    Console.WriteLine("Found it!");

if (errorResult.ContainsErr(specificException))
    Console.WriteLine("Expected error occurred");
```

### Option/Result interop

Convert between Options and Results:

```csharp
// Option to Result
var result = option.OkOr("Value not found"); // Result<T, string>
var result2 = option.OkOrElse(() => new MyError("Missing"));

// Result to Option
var okOption = result.Ok(); // Option<T> - None if Err
var errOption = result.Err(); // Option<TError> - None if Ok

// Transpose nested types
Option<Result<int, string>> optRes = /* ... */;
Result<Option<int>, string> resOpt = optRes.Transpose();

Result<Option<int>, string> resOpt2 = /* ... */;
Option<Result<int, string>> optRes2 = resOpt2.Transpose();

// NumericOption to Result
NumericOption<int> numOpt = 42;
var result = numOpt.OkOr("No number");
```

### LINQ Integration

Use results and options in LINQ queries:

```csharp
// Query syntax
var result = from user in GetUser(id)
             from orders in GetOrders(user.Id)
             from total in CalculateTotal(orders)
             select total;

// Method syntax
var names = users
    .Select(u => FindUserName(u.Id))
    .WhereSome() // Filter out None values
    .ToList();

// Working with collections
var results = ids
    .Select(id => GetUser(id))
    .Collect(); // Result<List<User>, TError>
```

### Advanced patterns

```csharp
// OrElse - provide alternative on error
var result = TryPrimary()
    .OrElse(err => TrySecondary())
    .OrElse(err => TryTertiary());

// Flatten nested results
Result<Result<int, string>, string> nested = GetNestedResult();
Result<int, string> flattened = nested.Flatten();

// Zip multiple results
var combined = result1.Zip(result2, (a, b) => a + b);

// Exception handling with custom error types
public record MyError(string Message, int Code);

var result = Result.From(
    () => RiskyOperation(),
    ex => new MyError(ex.Message, 500)
);
```

### Error type helper

Use the built-in `Error` type for flexible error handling:

```csharp
public Result<Data, Error> LoadData()
{
    try
    {
        return LoadFromFile();
    }
    catch (Exception ex)
    {
        return new Error(ex); // Captures exception
    }
}

// Error can be created from strings or exceptions
Error error1 = "Simple error message";
Error error2 = new InvalidOperationException("Oops");

// Access the original exception if needed
if (error.Exception != null)
    Console.WriteLine($"Exception: {error.Exception.Message}");
```

## API Overview

### Result<T, TError>

**Creation:**
- `Result.Ok<T, TError>(value)` - Create successful result
- `Result.Err<T, TError>(error)` - Create error result
- `Result.Try(() => ...)` - Catch exceptions
- `Result.TryAsync(async () => ...)` - Catch async exceptions
- `Result.From(func)` - Convert function to result
- Implicit conversions from `T` or `TError`

**State checking:**
- `IsOk` / `IsErr` - Boolean properties
- `WhenOk(out value)` - Get value if Ok
- `WhenErr(out error)` - Get error if Err

**Extracting values:**
- `Unwrap()` - Get value or throw
- `UnwrapErr()` - Get error or throw
- `UnwrapOr(default)` - Get value or default
- `UnwrapOrElse(func)` - Get value or compute default
- `UnwrapOrDefault()` - Get value or type default
- `Expect(message)` - Get value or throw with message
- `ExpectErr(message)` - Get error or throw with message

**Transformations:**
- `Map(func)` - Transform Ok value
- `MapErr(func)` - Transform Err value
- `AndThen(func)` - Chain to another Result
- `OrElse(func)` - Provide alternative on error
- `Flatten()` - Unwrap nested Results

**Pattern matching:**
- `Match(okFunc, errFunc)` - Handle both cases
- Deconstruction: `var (value, error) = result`
- Pattern: `result switch { (true, var val, _) => ..., ... }`

**Conversions:**
- `AsSpan()` - Convert to ReadOnlySpan
- `AsEnumerable()` - Convert to IEnumerable

### Option<T>

**Creation:**
- `Option.Some(value)` - Create Some
- `Option.None<T>()` - Create None
- `Option.Create(nullable)` - From nullable value
- `Option.Try(tryGetFunc)` - From Try pattern
- `Option.Parse<T>(string)` - Parse with IParsable (.NET 7+)
- `Option.ParseEnum<T>(string)` - Parse enums
- Implicit conversion from `T`

**State checking:**
- `IsSome` / `IsNone` - Boolean properties
- `WhenSome(out value)` - Get value if Some

**Extracting values:**
- `Unwrap()` - Get value or throw
- `UnwrapOr(default)` - Get value or default
- `UnwrapOrElse(func)` - Get value or compute default

**Transformations:**
- `Map(func)` - Transform Some value
- `Then(func)` - Chain to another Option
- `OrElse(func)` - Provide alternative on None
- `Filter(predicate)` - Keep only if predicate true
- `Flatten()` - Unwrap nested Options

**Pattern matching:**
- `Match(someFunc, noneFunc)` - Handle both cases
- Deconstruction: `var (isSome, value) = option` (.NET 8+)

**Conversions:**
- `AsSpan()` - Convert to ReadOnlySpan
- `AsEnumerable()` - Convert to IEnumerable

### NumericOption<T>

All `Option<T>` methods plus:

**Math operations:**
- `+`, `-`, `*`, `/`, `%` - Arithmetic operators
- `++`, `--` - Increment/decrement
- Unary `+`, `-` - Positive/negative

**Numeric functions:**
- `Abs(value)` - Absolute value
- `Max(x, y)` / `Min(x, y)` - Min/max values
- `Clamp(value, min, max)` - Clamp to range

**Number checks:**
- `IsEvenInteger()` / `IsOddInteger()`
- `IsPositive()` / `IsNegative()`
- `IsInfinity()` / `IsNaN()` / `IsZero()`
- And more INumber<T> methods

**Parsing:**
- `Parse(string)` - Parse with TryParse
- Supports all numeric types implementing INumber<T>

### Extension Methods

**Option Extensions:**
- Transform: `Map()`, `MapOr()`, `MapOrElse()`
- Chain: `AndThen()`, `And()`, `OrElse()`, `Or()`, `Xor()`
- Utilities: `Filter()`, `Flatten()`, `Zip()`, `ZipWith()`
- Inspect: `Expect()`, `UnwrapOr()`, `UnwrapOrElse()`
- Conversions: `AsOption()`, `Some()`, `None()`, `IsNoneOr()`
- Async: `MapAsync()`, `AndThenAsync()`, `OrElseAsync()`, `MapOrElseAsync()`

**Result Extensions:**
- Transform: `Map()`, `MapErr()`, `MapOr()`, `MapOrElse()`
- Chain: `AndThen()`, `And()`, `OrElse()`, `Or()`
- Utilities: `Flatten()`, `Inspect()`, `InspectErr()`, `Contains()`, `ContainsErr()`
- LINQ: `Select()`, `SelectMany()`, `Where()`
- Extract: `UnwrapOr()`
- Async: `MapAsync()`, `AndThenAsync()`, `OrElseAsync()`, `MapOrElseAsync()`

**Collection Extensions (Option):**
- Safe access: `FirstOrNone()`, `LastOrNone()`, `SingleOrNone()`, `ElementAtOrNone()`
- Transform: `SelectWhere()`, `Values()`
- Dictionary: `GetValueOrNone()`
- Stack/Queue: `PeekOrNone()`, `PopOrNone()`, `DequeueOrNone()`
- Immutable collections: Full support for ImmutableStack, ImmutableQueue
- Concurrent collections: Thread-safe operations for ConcurrentBag, ConcurrentQueue, etc.
- Set operations: `GetValueOrNone()` for HashSet, SortedSet, ImmutableHashSet, etc.
- Sequence: `Sequence()`, `SequenceList()`, `ToList()`
- Async: `FirstOrNoneAsync()`, `ValuesAsync()` for IAsyncEnumerable

**Collection Extensions (Result):**
- Extract: `Values()`, `Errors()`
- Filter collections of results
- Async: `ValuesAsync()`, `ErrorsAsync()` for IAsyncEnumerable

**Interop Extensions:**
- Option ↔ Result: `OkOr()`, `OkOrElse()`, `Ok()`, `Err()`, `Transpose()`
- JSON support: `GetOption()`, `GetPropValue()`, `GetPropOption()` for System.Text.Json types
- Numeric: Extensions for `NumericOption<T>` with full INumber<T> support

**Bool Extensions:**
- `Then<T>()` - Execute function if true, return Option
- `ThenSome<T>()` - Create Some if true, None otherwise
- Async: `ThenAsync()`, `ThenSomeAsync()`

**String/Number Extensions:**
- `Parse<T>()` - Safe parsing to Option for any INumber<T>

All async extensions support both `Task<T>` and `ValueTask<T>` variants for maximum flexibility and performance.

## Features

| Feature | SharpResults | OneOf | FluentResults |
|---------|:------------:|:-----:|:-------------:|
| **Core Types** |
| Result<T, TError> | ✅ | ✅ | ✅ |
| Option<T> | ✅ | ❌ | ❌ |
| Custom Error Types | ✅ | ✅ | ✅ |
| Unit Type | ✅ | ❌ | ❌ |
| NumericOption<T> | ✅ | ❌ | ❌ |
| **Language Features** |
| Pattern Matching | ✅ | ✅ | ❌ |
| Deconstruction | ✅ | ✅ | ❌ |
| Implicit Conversions | ✅ | ✅ | ❌ |
| LINQ Query Syntax | ✅ | ❌ | ❌ |
| **Functional Operations** |
| Map/Then/OrElse | ✅ | ❌ | ✅ |
| Flatten | ✅ | ❌ | ❌ |
| Filter | ✅ | ❌ | ❌ |
| Zip | ✅ | ❌ | ❌ |
| **Async Support** |
| Full Async/Await | ✅ | ❌ | ✅ |
| ValueTask Support | ✅ | ❌ | ❌ |
| IAsyncEnumerable | ✅ | ❌ | ❌ |
| **Collections** |
| Collection Extensions | ✅ | ❌ | ❌ |
| Safe LINQ (FirstOrNone, etc) | ✅ | ❌ | ❌ |
| Immutable Collections | ✅ | ❌ | ❌ |
| Concurrent Collections | ✅ | ❌ | ❌ |
| **Serialization** |
| JSON Serialization | ✅ | ❌ | ✅ |
| Built-in Converters | ✅ | ❌ | ✅ |
| **Error Handling** |
| Exception Capturing | ✅ | ❌ | ✅ |
| Multiple Errors | ✅ | ❌ | ✅ |
| Error Transformation | ✅ | ❌ | ✅ |
| **Interop** |
| Option ↔ Result | ✅ | ❌ | ❌ |
| JSON Extensions | ✅ | ❌ | ❌ |
| Bool Extensions | ✅ | ❌ | ❌ |
| **Other** |
| Zero Dependencies | ✅ | ✅ | ✅ |
| .NET 8+ | ✅ | ✅ (.NET Standard 2.0+) | ✅ (.NET Standard 2.0+) |
| Active Development | ✅ | ✅ | ✅ |

## Contributing

Pull requests are welcome! If you want to add a feature or fix a bug:

1. Fork the repo
2. Create your branch: `git checkout -b feature/cool-new-thing`
3. Make your changes and add tests
4. Make sure everything passes
5. Push and open a PR

Try to match the existing code style, and update docs if you're changing behavior.

## License

MIT License - do whatever you want with it. See the LICENSE file for the legal stuff.