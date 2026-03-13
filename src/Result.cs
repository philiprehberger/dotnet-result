namespace Philiprehberger.Result;

/// <summary>
/// Represents a value that is either a success (Ok) or a failure (Err).
/// </summary>
public readonly struct Result<T, E>
{
    private readonly T? _value;
    private readonly E? _error;
    private readonly bool _isOk;

    private Result(T value)
    {
        _value = value;
        _error = default;
        _isOk = true;
    }

    private Result(E error, bool _)
    {
        _value = default;
        _error = error;
        _isOk = false;
    }

    public static Result<T, E> Ok(T value) => new(value);
    public static Result<T, E> Err(E error) => new(error, false);

    public bool IsOk => _isOk;
    public bool IsErr => !_isOk;

    /// <summary>Returns the success value or throws if Err.</summary>
    public T Unwrap() =>
        _isOk ? _value! : throw new InvalidOperationException($"Called Unwrap on an Err value: {_error}");

    /// <summary>Returns the success value or the provided default.</summary>
    public T UnwrapOr(T defaultValue) => _isOk ? _value! : defaultValue;

    /// <summary>Returns the success value or calls the factory function.</summary>
    public T UnwrapOrElse(Func<E, T> factory) => _isOk ? _value! : factory(_error!);

    /// <summary>Returns the error value or throws if Ok.</summary>
    public E UnwrapErr() =>
        !_isOk ? _error! : throw new InvalidOperationException("Called UnwrapErr on an Ok value");

    /// <summary>Transforms the success value.</summary>
    public Result<U, E> Map<U>(Func<T, U> fn) =>
        _isOk ? Result<U, E>.Ok(fn(_value!)) : Result<U, E>.Err(_error!);

    /// <summary>Transforms the error value.</summary>
    public Result<T, F> MapErr<F>(Func<E, F> fn) =>
        _isOk ? Result<T, F>.Ok(_value!) : Result<T, F>.Err(fn(_error!));

    /// <summary>Chains a function that returns a Result.</summary>
    public Result<U, E> FlatMap<U>(Func<T, Result<U, E>> fn) =>
        _isOk ? fn(_value!) : Result<U, E>.Err(_error!);

    /// <summary>Pattern match on Ok or Err.</summary>
    public U Match<U>(Func<T, U> onOk, Func<E, U> onErr) =>
        _isOk ? onOk(_value!) : onErr(_error!);

    /// <summary>Executes an action if Ok, then returns the same result. Useful for side-effects.</summary>
    public Result<T, E> Tap(Action<T> action)
    {
        if (_isOk) action(_value!);
        return this;
    }

    /// <summary>Executes an action if Err, then returns the same result. Useful for side-effects.</summary>
    public Result<T, E> TapErr(Action<E> action)
    {
        if (!_isOk) action(_error!);
        return this;
    }

    public override string ToString() =>
        _isOk ? $"Ok({_value})" : $"Err({_error})";
}

/// <summary>Helper methods for creating Result values.</summary>
public static class Result
{
    public static Result<T, E> Ok<T, E>(T value) => Result<T, E>.Ok(value);
    public static Result<T, E> Err<T, E>(E error) => Result<T, E>.Err(error);

    /// <summary>Wraps a function that may throw into a Result.</summary>
    public static Result<T, Exception> Try<T>(Func<T> fn)
    {
        try { return Result<T, Exception>.Ok(fn()); }
        catch (Exception ex) { return Result<T, Exception>.Err(ex); }
    }

    /// <summary>Wraps an async function that may throw into a Result.</summary>
    public static async Task<Result<T, Exception>> TryAsync<T>(Func<Task<T>> fn)
    {
        try { return Result<T, Exception>.Ok(await fn()); }
        catch (Exception ex) { return Result<T, Exception>.Err(ex); }
    }

    /// <summary>Collects a list of Results into a Result of a list.</summary>
    public static Result<List<T>, E> All<T, E>(IEnumerable<Result<T, E>> results)
    {
        var values = new List<T>();
        foreach (var r in results)
        {
            if (r.IsErr) return Result<List<T>, E>.Err(r.UnwrapErr());
            values.Add(r.Unwrap());
        }
        return Result<List<T>, E>.Ok(values);
    }

    /// <summary>Combines multiple results. Returns Ok with all values if all succeed, or the first error.</summary>
    public static Result<IReadOnlyList<T>, E> Combine<T, E>(IEnumerable<Result<T, E>> results)
    {
        var values = new List<T>();
        foreach (var r in results)
        {
            if (r.IsErr) return Result<IReadOnlyList<T>, E>.Err(r.UnwrapErr());
            values.Add(r.Unwrap());
        }
        return Result<IReadOnlyList<T>, E>.Ok(values);
    }

    /// <summary>Combines multiple results, collecting ALL errors instead of failing fast.</summary>
    public static Result<IReadOnlyList<T>, IReadOnlyList<E>> CombineAll<T, E>(IEnumerable<Result<T, E>> results)
    {
        var values = new List<T>();
        var errors = new List<E>();
        foreach (var r in results)
        {
            if (r.IsErr)
                errors.Add(r.UnwrapErr());
            else
                values.Add(r.Unwrap());
        }
        return errors.Count > 0
            ? Result<IReadOnlyList<T>, IReadOnlyList<E>>.Err(errors)
            : Result<IReadOnlyList<T>, IReadOnlyList<E>>.Ok(values);
    }
}

/// <summary>LINQ extension methods for Result.</summary>
public static class ResultLinqExtensions
{
    public static Result<U, E> Select<T, E, U>(this Result<T, E> result, Func<T, U> selector) =>
        result.Map(selector);

    public static Result<V, E> SelectMany<T, E, U, V>(
        this Result<T, E> result,
        Func<T, Result<U, E>> selector,
        Func<T, U, V> resultSelector) =>
        result.FlatMap(t => selector(t).Map(u => resultSelector(t, u)));
}
