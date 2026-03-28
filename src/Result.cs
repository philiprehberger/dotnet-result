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

    /// <summary>Creates a success result containing <paramref name="value"/>.</summary>
    /// <param name="value">The success value.</param>
    /// <returns>An Ok result.</returns>
    public static Result<T, E> Ok(T value) => new(value);

    /// <summary>Creates a failure result containing <paramref name="error"/>.</summary>
    /// <param name="error">The error value.</param>
    /// <returns>An Err result.</returns>
    public static Result<T, E> Err(E error) => new(error, false);

    /// <summary>Gets a value indicating whether this result is Ok.</summary>
    public bool IsOk => _isOk;

    /// <summary>Gets a value indicating whether this result is Err.</summary>
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

    /// <summary>Executes an action if Err, then returns the same result. Alias for <see cref="TapErr"/>.</summary>
    public Result<T, E> TapError(Action<E> action) => TapErr(action);

    /// <summary>If Ok, tests the value against <paramref name="predicate"/>. Returns Err with the specified error when the predicate fails.</summary>
    /// <param name="predicate">A function to test the success value.</param>
    /// <param name="error">The error to return when the predicate is false.</param>
    /// <returns>The original result if Err or if the predicate passes; otherwise Err with <paramref name="error"/>.</returns>
    public Result<T, E> Ensure(Func<T, bool> predicate, E error)
    {
        if (!_isOk) return this;
        return predicate(_value!) ? this : Err(error);
    }

    /// <summary>Asynchronously transforms the success value.</summary>
    /// <typeparam name="TNew">The new success type.</typeparam>
    /// <param name="mapper">An async function to transform the success value.</param>
    /// <returns>A task containing the mapped result.</returns>
    public async Task<Result<TNew, E>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
    {
        if (!_isOk) return Result<TNew, E>.Err(_error!);
        var newValue = await mapper(_value!);
        return Result<TNew, E>.Ok(newValue);
    }

    /// <summary>Asynchronously chains a function that returns a Result.</summary>
    /// <typeparam name="TNew">The new success type.</typeparam>
    /// <param name="mapper">An async function that returns a new Result.</param>
    /// <returns>A task containing the chained result.</returns>
    public async Task<Result<TNew, E>> FlatMapAsync<TNew>(Func<T, Task<Result<TNew, E>>> mapper)
    {
        if (!_isOk) return Result<TNew, E>.Err(_error!);
        return await mapper(_value!);
    }

    /// <summary>Asynchronously pattern match on Ok or Err.</summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="onSuccess">An async function called when Ok.</param>
    /// <param name="onFailure">An async function called when Err.</param>
    /// <returns>A task containing the matched output.</returns>
    public async Task<TOut> MatchAsync<TOut>(Func<T, Task<TOut>> onSuccess, Func<E, Task<TOut>> onFailure)
    {
        return _isOk ? await onSuccess(_value!) : await onFailure(_error!);
    }

    /// <summary>If Err, calls <paramref name="fn"/> with the error to attempt recovery. If Ok, returns this.</summary>
    public Result<T, E> OrElse(Func<E, Result<T, E>> fn)
    {
        return IsOk ? this : fn(_error!);
    }

    /// <summary>If Ok, tests the value against <paramref name="predicate"/>. Returns Err (via <paramref name="errorFactory"/>) when the predicate fails.</summary>
    public Result<T, E> Filter(Func<T, bool> predicate, Func<T, E> errorFactory)
    {
        if (!IsOk) return this;
        return predicate(_value!) ? this : Err(errorFactory(_value!));
    }

    /// <summary>Returns <see langword="true"/> if Ok and the value satisfies <paramref name="predicate"/>.</summary>
    public bool IsOkAnd(Func<T, bool> predicate) => IsOk && predicate(_value!);

    /// <summary>Returns <see langword="true"/> if Err and the error satisfies <paramref name="predicate"/>.</summary>
    public bool IsErrAnd(Func<E, bool> predicate) => !IsOk && predicate(_error!);

    /// <summary>Returns the success value or throws <see cref="InvalidOperationException"/> with a custom message.</summary>
    public T Expect(string message)
    {
        if (IsOk) return _value!;
        throw new InvalidOperationException($"{message}: {_error}");
    }

    /// <summary>Returns a string representation of the result.</summary>
    /// <returns>A string in the form "Ok(value)" or "Err(error)".</returns>
    public override string ToString() =>
        _isOk ? $"Ok({_value})" : $"Err({_error})";
}

/// <summary>Helper methods for creating Result values.</summary>
public static class Result
{
    /// <summary>Creates a success result containing <paramref name="value"/>.</summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="value">The success value.</param>
    /// <returns>An Ok result.</returns>
    public static Result<T, E> Ok<T, E>(T value) => Result<T, E>.Ok(value);

    /// <summary>Creates a failure result containing <paramref name="error"/>.</summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="error">The error value.</param>
    /// <returns>An Err result.</returns>
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

    /// <summary>Flattens a nested <c>Result&lt;Result&lt;T, E&gt;, E&gt;</c> into a single <c>Result&lt;T, E&gt;</c>.</summary>
    public static Result<T, E> Flatten<T, E>(Result<Result<T, E>, E> nested)
    {
        return nested.IsOk ? nested.Unwrap() : Result<T, E>.Err(nested.UnwrapErr());
    }

    /// <summary>Combines three results into a tuple. Returns the first error if any result is a failure.</summary>
    public static Result<(T1, T2, T3), E> Combine<T1, T2, T3, E>(
        Result<T1, E> r1, Result<T2, E> r2, Result<T3, E> r3)
    {
        if (r1.IsErr) return Result<(T1, T2, T3), E>.Err(r1.UnwrapErr());
        if (r2.IsErr) return Result<(T1, T2, T3), E>.Err(r2.UnwrapErr());
        if (r3.IsErr) return Result<(T1, T2, T3), E>.Err(r3.UnwrapErr());
        return Result<(T1, T2, T3), E>.Ok((r1.Unwrap(), r2.Unwrap(), r3.Unwrap()));
    }

    /// <summary>Combines four results into a tuple. Returns the first error if any result is a failure.</summary>
    public static Result<(T1, T2, T3, T4), E> Combine<T1, T2, T3, T4, E>(
        Result<T1, E> r1, Result<T2, E> r2, Result<T3, E> r3, Result<T4, E> r4)
    {
        if (r1.IsErr) return Result<(T1, T2, T3, T4), E>.Err(r1.UnwrapErr());
        if (r2.IsErr) return Result<(T1, T2, T3, T4), E>.Err(r2.UnwrapErr());
        if (r3.IsErr) return Result<(T1, T2, T3, T4), E>.Err(r3.UnwrapErr());
        if (r4.IsErr) return Result<(T1, T2, T3, T4), E>.Err(r4.UnwrapErr());
        return Result<(T1, T2, T3, T4), E>.Ok((r1.Unwrap(), r2.Unwrap(), r3.Unwrap(), r4.Unwrap()));
    }

    /// <summary>Combines five results into a tuple. Returns the first error if any result is a failure.</summary>
    public static Result<(T1, T2, T3, T4, T5), E> Combine<T1, T2, T3, T4, T5, E>(
        Result<T1, E> r1, Result<T2, E> r2, Result<T3, E> r3, Result<T4, E> r4, Result<T5, E> r5)
    {
        if (r1.IsErr) return Result<(T1, T2, T3, T4, T5), E>.Err(r1.UnwrapErr());
        if (r2.IsErr) return Result<(T1, T2, T3, T4, T5), E>.Err(r2.UnwrapErr());
        if (r3.IsErr) return Result<(T1, T2, T3, T4, T5), E>.Err(r3.UnwrapErr());
        if (r4.IsErr) return Result<(T1, T2, T3, T4, T5), E>.Err(r4.UnwrapErr());
        if (r5.IsErr) return Result<(T1, T2, T3, T4, T5), E>.Err(r5.UnwrapErr());
        return Result<(T1, T2, T3, T4, T5), E>.Ok((r1.Unwrap(), r2.Unwrap(), r3.Unwrap(), r4.Unwrap(), r5.Unwrap()));
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
    /// <summary>Projects the success value using <paramref name="selector"/>.</summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <typeparam name="U">The projected success type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <param name="selector">The projection function.</param>
    /// <returns>A result with the projected value if Ok; otherwise the original error.</returns>
    public static Result<U, E> Select<T, E, U>(this Result<T, E> result, Func<T, U> selector) =>
        result.Map(selector);

    /// <summary>Projects the success value into a new result and applies a result selector.</summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <typeparam name="U">The intermediate success type.</typeparam>
    /// <typeparam name="V">The final success type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <param name="selector">A function that returns an intermediate result.</param>
    /// <param name="resultSelector">A function that combines the source and intermediate values.</param>
    /// <returns>A result with the combined value if both are Ok; otherwise the first error.</returns>
    public static Result<V, E> SelectMany<T, E, U, V>(
        this Result<T, E> result,
        Func<T, Result<U, E>> selector,
        Func<T, U, V> resultSelector) =>
        result.FlatMap(t => selector(t).Map(u => resultSelector(t, u)));
}
