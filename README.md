# Philiprehberger.Result

A lightweight Result type for .NET — model success and failure without exceptions.

## Install

```bash
dotnet add package Philiprehberger.Result
```

## Usage

```csharp
using Philiprehberger.Result;

// Create results
var ok = Result<int, string>.Ok(42);
var err = Result<int, string>.Err("not found");

// Unwrap
int value = ok.Unwrap();           // 42
int safe = err.UnwrapOr(0);        // 0

// Transform
var doubled = ok.Map(x => x * 2);  // Ok(84)

// Chain
var result = ok.FlatMap(x =>
    x > 0 ? Result<string, string>.Ok($"positive: {x}")
           : Result<string, string>.Err("must be positive"));

// Pattern match
string msg = ok.Match(
    onOk: v => $"Got {v}",
    onErr: e => $"Error: {e}"
);

// Try — wrap throwing code
var parsed = Result.Try(() => int.Parse("123"));  // Ok(123)
var failed = Result.Try(() => int.Parse("abc"));  // Err(FormatException)

// Collect
var all = Result.All(new[] {
    Result<int, string>.Ok(1),
    Result<int, string>.Ok(2),
});  // Ok([1, 2])

// Combine — fail-fast, returns first error
var combined = Result.Combine(new[] {
    Result<int, string>.Ok(1),
    Result<int, string>.Ok(2),
});  // Ok([1, 2])

// CombineAll — collects ALL errors
var validated = Result.CombineAll(new[] {
    Result<int, string>.Err("too short"),
    Result<int, string>.Err("too old"),
});  // Err(["too short", "too old"])

// Tap / TapErr — side-effects without breaking chains
var logged = ok
    .Tap(v => Console.WriteLine($"Got value: {v}"))
    .Map(x => x * 2);

err.TapErr(e => Console.WriteLine($"Error: {e}"));

// LINQ query syntax
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

### Static helpers

| Method | Description |
|--------|-------------|
| `Result.Try<T>(Func<T>)` | Wrap a throwing function |
| `Result.TryAsync<T>(Func<Task<T>>)` | Wrap an async throwing function |
| `Result.All<T, E>(...)` | Collect results into a single result |
| `Result.Combine<T, E>(...)` | Combine results, fail on first error |
| `Result.CombineAll<T, E>(...)` | Combine results, collect all errors |

### LINQ extensions

| Method | Description |
|--------|-------------|
| `Select` | Enables `select` in LINQ queries (maps Ok value) |
| `SelectMany` | Enables `from ... from ...` in LINQ queries (chains results) |

## License

MIT
