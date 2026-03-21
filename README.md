# Philiprehberger.Result

[![CI](https://github.com/philiprehberger/dotnet-result/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-result/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.Result.svg)](https://www.nuget.org/packages/Philiprehberger.Result)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-result)](LICENSE)

Lightweight Result type for .NET — model success and failure without exceptions, with pattern matching and LINQ support.

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

### Try and Collect

```csharp
var parsed = Result.Try(() => int.Parse("123"));  // Ok(123)
var failed = Result.Try(() => int.Parse("abc"));  // Err(FormatException)

var all = Result.All(new[] {
    Result<int, string>.Ok(1),
    Result<int, string>.Ok(2),
});  // Ok([1, 2])

var combined = Result.Combine(new[] {
    Result<int, string>.Ok(1),
    Result<int, string>.Ok(2),
});  // Ok([1, 2])

var validated = Result.CombineAll(new[] {
    Result<int, string>.Err("too short"),
    Result<int, string>.Err("too old"),
});  // Err(["too short", "too old"])
```

### Side Effects

```csharp
var logged = ok
    .Tap(v => Console.WriteLine($"Got value: {v}"))
    .Map(x => x * 2);

err.TapErr(e => Console.WriteLine($"Error: {e}"));
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
| `Tap(Action<T>)` | Execute side-effect if Ok, return same result |
| `TapErr(Action<E>)` | Execute side-effect if Err, return same result |
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
| `Result.CombineAll<T, E>(...)` | Combine results, collect all errors |
| `Result.Flatten<T, E>(...)` | Unwrap a nested `Result<Result<T,E>,E>` |

### LINQ extensions

| Method | Description |
|--------|-------------|
| `Select` | Enables `select` in LINQ queries (maps Ok value) |
| `SelectMany` | Enables `from ... from ...` in LINQ queries (chains results) |

### Error Recovery

```csharp
// OrElse — recover from an error by trying an alternative
Result<int, string> Fetch(string key) =>
    Cache.Get(key).OrElse(err => Database.Get(key));

// Expect — unwrap with a meaningful message on failure
int port = config.OrElse(_ => Result<int, string>.Ok(8080))
    .Expect("Failed to resolve port");
```

### Filtering

```csharp
// Filter — narrow Ok values with a predicate
var positive = Result<int, string>.Ok(42)
    .Filter(x => x > 0, x => $"{x} is not positive");  // Ok(42)

var rejected = Result<int, string>.Ok(-1)
    .Filter(x => x > 0, x => $"{x} is not positive");  // Err("-1 is not positive")

// Flatten — unwrap nested results
Result<Result<int, string>, string> nested =
    Result<Result<int, string>, string>.Ok(Result<int, string>.Ok(42));
Result<int, string> flat = Result.Flatten(nested);  // Ok(42)

// IsOkAnd / IsErrAnd — quick predicate checks
bool isLarge = Result<int, string>.Ok(100).IsOkAnd(x => x > 50);   // true
bool isNotFound = Result<int, string>.Err("404").IsErrAnd(e => e.Contains("404")); // true
```

## Development

```bash
dotnet build src/Philiprehberger.Result.csproj --configuration Release
```

## License

MIT
