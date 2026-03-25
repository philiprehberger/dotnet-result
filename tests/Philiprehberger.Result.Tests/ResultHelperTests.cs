using Xunit;

namespace Philiprehberger.Result.Tests;

public class ResultHelperTests
{
    [Fact]
    public void Ok_StaticHelper_CreatesOkResult()
    {
        var result = Result.Ok<int, string>(42);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void Err_StaticHelper_CreatesErrResult()
    {
        var result = Result.Err<int, string>("fail");

        Assert.True(result.IsErr);
        Assert.Equal("fail", result.UnwrapErr());
    }

    [Fact]
    public void Try_SuccessfulFunction_ReturnsOk()
    {
        var result = Result.Try(() => 42);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void Try_ThrowingFunction_ReturnsErr()
    {
        var result = Result.Try<int>(() => throw new InvalidOperationException("boom"));

        Assert.True(result.IsErr);
        Assert.IsType<InvalidOperationException>(result.UnwrapErr());
    }

    [Fact]
    public async Task TryAsync_SuccessfulFunction_ReturnsOk()
    {
        var result = await Result.TryAsync(() => Task.FromResult(42));

        Assert.True(result.IsOk);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void All_AllOk_ReturnsOkWithList()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        var combined = Result.All(results);

        Assert.True(combined.IsOk);
        Assert.Equal(3, combined.Unwrap().Count);
    }

    [Fact]
    public void All_ContainsErr_ReturnsFirstErr()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("fail"),
            Result<int, string>.Ok(3)
        };

        var combined = Result.All(results);

        Assert.True(combined.IsErr);
        Assert.Equal("fail", combined.UnwrapErr());
    }

    [Fact]
    public void CombineAll_MultipleErrors_CollectsAllErrors()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("a"),
            Result<int, string>.Err("b")
        };

        var combined = Result.CombineAll(results);

        Assert.True(combined.IsErr);
        Assert.Equal(2, combined.UnwrapErr().Count);
    }

    [Fact]
    public void Flatten_NestedOk_UnwrapsInnerResult()
    {
        var nested = Result<Result<int, string>, string>.Ok(Result<int, string>.Ok(42));

        var flat = Result.Flatten(nested);

        Assert.True(flat.IsOk);
        Assert.Equal(42, flat.Unwrap());
    }
}
