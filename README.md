# Philiprehberger.Result

[![CI](https://github.com/philiprehberger/dotnet-result/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-result/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.Result.svg)](https://www.nuget.org/packages/Philiprehberger.Result)
[![GitHub release](https://img.shields.io/github/v/release/philiprehberger/dotnet-result)](https://github.com/philiprehberger/dotnet-result/releases)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-result)](https://github.com/philiprehberger/dotnet-result/commits/main)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-result)](LICENSE)
[![Bug Reports](https://img.shields.io/github/issues/philiprehberger/dotnet-result/bug)](https://github.com/philiprehberger/dotnet-result/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![Feature Requests](https://img.shields.io/github/issues/philiprehberger/dotnet-result/enhancement)](https://github.com/philiprehberger/dotnet-result/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)
[![Sponsor](https://img.shields.io/badge/sponsor-GitHub%20Sponsors-ec6cb9)](https://github.com/sponsors/philiprehberger)

Lightweight Result type for .NET — model success and failure without exceptions using pattern matching.

## Installation

```bash
dotnet add package Philiprehberger.Result
```

## Usage

### Create Results

```csharp
using Philiprehberger.Result;

var ok = Result<int, string>.Ok(42);
var err = Result<int, string>.Err("not found");

int value = ok.Unwrap();           // 42
int safe = err.UnwrapOr(0);        // 0
```

### Transform and Match

```csharp
var doubled = ok.Map(x => x * 2);  // Ok(84)

var result = ok.FlatMap(x =>
    x > 0 ? Result<string, string>.Ok($"positive: {x}")
           : Result<string, string>.Err("must be positive"));

string msg = ok.Match(
    onOk: v => $"Got {v}",
    onErr: e => $"Error: {e}"
);
```

### Async Operations

```csharp
var result = Result<int, string>.Ok(5);

// MapAsync — transform the value asynchronously
var mapped = await result.MapAsync(async v => await FetchFromApi(v));

// FlatMapAsync — chain async result-returning functions
var chained = await result.FlatMapAsync(async v =>
    await ValidateAsync(v)
        ? Result<int, string>.Ok(v)
        : Result<int, string>.Err("validation failed"));

// MatchAsync — pattern match with async handlers
var output = await result.MatchAsync(
    onSuccess: async v => await FormatAsync(v),
    onFailure: async e => await LogErrorAsync(e));
```

### Ensure

```csharp
// Convert success to failure if predicate returns false
var validated = Result<int, string>.Ok(42)
    .Ensure(x => x > 0, "Must be positive")
    .Ensure(x => x < 100, "Must be under 100");  // Ok(42)

var rejected = Result<int, string>.Ok(-1)
    .Ensure(x => x > 0, "Must be positive");  // Err("Must be positive")
```

### Side Effects

```csharp
var logged = ok
    .Tap(v => Console.WriteLine($"Got value: {v}"))
    .TapError(e => Console.WriteLine($"Error: {e}"))
    .Map(x => x * 2);
```

### Try and Collect

```csharp
var parsed = Result.Try(() => int.Parse("123"));  // Ok(123)
var failed = Result.Try(() => int.Parse("abc"));  // Err(FormatException)

var all = Result.All(new[] {
    Result<int, string>.Ok(1),
    Result<int, string>.Ok(2),
});  // Ok([1, 2])
```

### Combine Multiple Results

```csharp
// Typed tuple overloads for 3, 4, or 5 results
var name = Result<string, string>.Ok("Alice");
var age = Result<int, string>.Ok(30);
var email = Result<string, string>.Ok("alice@example.com");

var combined = Result.Combine(name, age, email);
// Ok(("Alice", 30, "alice@example.com"))

var (n, a, e) = combined.Unwrap();

// Collect all errors instead of failing fast
var validated = Result.CombineAll(new[] {
    Result<int, string>.Err("too short"),
    Result<int, string>.Err("too old"),
});  // Err(["too short", "too old"])
```

### Error Recovery

```csharp
Result<int, string> Fetch(string key) =>
    Cache.Get(key).OrElse(err => Database.Get(key));

int port = config.OrElse(_ => Result<int, string>.Ok(8080))
    .Expect("Failed to resolve port");
```

### Filtering

```csharp
var positive = Result<int, string>.Ok(42)
    .Filter(x => x > 0, x => $"{x} is not positive");  // Ok(42)

var rejected = Result<int, string>.Ok(-1)
    .Filter(x => x > 0, x => $"{x} is not positive");  // Err("-1 is not positive")

bool isLarge = Result<int, string>.Ok(100).IsOkAnd(x => x > 50);   // true
```

### LINQ Query Syntax

```csharp
var sum = from x in Result<int, string>.Ok(10)
          from y in Result<int, string>.Ok(20)
          select x + y;  // Ok(30)
```

## API

### `Result<T, E>`

| Method | Description |
|--------|-------------|
| `Ok(T value)` | Create a success result |
| `Err(E error)` | Create a failure result |
| `IsOk` / `IsErr` | Check result state |
| `Unwrap()` | Get value or throw |
| `UnwrapOr(T default)` | Get value or default |
| `UnwrapOrElse(Func<E, T>)` | Get value or compute from error |
| `UnwrapErr()` | Get error or throw |
| `Map<U>(Func<T, U>)` | Transform success value |
| `MapErr<F>(Func<E, F>)` | Transform error value |
| `FlatMap<U>(Func<T, Result<U, E>>)` | Chain result-returning functions |
| `Match<U>(onOk, onErr)` | Pattern match on result |
| `MapAsync<TNew>(Func<T, Task<TNew>>)` | Asynchronously transform success value |
| `FlatMapAsync<TNew>(Func<T, Task<Result<TNew, E>>>)` | Asynchronously chain result-returning functions |
| `MatchAsync<TOut>(onSuccess, onFailure)` | Asynchronously pattern match on result |
| `Tap(Action<T>)` | Execute side-effect if Ok, return same result |
| `TapErr(Action<E>)` | Execute side-effect if Err, return same result |
| `TapError(Action<E>)` | Alias for TapErr |
| `Ensure(Func<T, bool>, E)` | Convert success to failure if predicate is false |
| `OrElse(Func<E, Result<T, E>>)` | Attempt recovery from an error |
| `Filter(predicate, errorFactory)` | Narrow Ok values by predicate |
| `IsOkAnd(Func<T, bool>)` | True if Ok and predicate holds |
| `IsErrAnd(Func<E, bool>)` | True if Err and predicate holds |
| `Expect(string message)` | Get value or throw with custom message |

### Static helpers

| Method | Description |
|--------|-------------|
| `Result.Try<T>(Func<T>)` | Wrap a throwing function |
| `Result.TryAsync<T>(Func<Task<T>>)` | Wrap an async throwing function |
| `Result.All<T, E>(...)` | Collect results into a single result |
| `Result.Combine<T, E>(...)` | Combine results, fail on first error |
| `Result.Combine<T1, T2, T3, E>(...)` | Combine 3 typed results into a tuple |
| `Result.Combine<T1, T2, T3, T4, E>(...)` | Combine 4 typed results into a tuple |
| `Result.Combine<T1, T2, T3, T4, T5, E>(...)` | Combine 5 typed results into a tuple |
| `Result.CombineAll<T, E>(...)` | Combine results, collect all errors |
| `Result.Flatten<T, E>(...)` | Unwrap a nested `Result<Result<T,E>,E>` |

### LINQ extensions

| Method | Description |
|--------|-------------|
| `Select` | Enables `select` in LINQ queries (maps Ok value) |
| `SelectMany` | Enables `from ... from ...` in LINQ queries (chains results) |

## Development

```bash
dotnet build src/Philiprehberger.Result.csproj --configuration Release
```

## Support

If you find this package useful, consider giving it a star on GitHub — it helps motivate continued maintenance and development.

[![LinkedIn](https://img.shields.io/badge/Philip%20Rehberger-LinkedIn-0A66C2?logo=linkedin)](https://www.linkedin.com/in/philiprehberger)
[![More packages](https://img.shields.io/badge/more-open%20source%20packages-blue)](https://philiprehberger.com/open-source-packages)

## License

[MIT](LICENSE)
