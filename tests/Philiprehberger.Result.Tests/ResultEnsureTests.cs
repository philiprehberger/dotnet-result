using Xunit;

namespace Philiprehberger.Result.Tests;

public class ResultEnsureTests
{
    [Fact]
    public void Ensure_OkAndPredicateTrue_ReturnsOk()
    {
        var result = Result<int, string>.Ok(10);

        var ensured = result.Ensure(x => x > 0, "Must be positive");

        Assert.True(ensured.IsOk);
        Assert.Equal(10, ensured.Unwrap());
    }

    [Fact]
    public void Ensure_OkAndPredicateFalse_ReturnsErr()
    {
        var result = Result<int, string>.Ok(-5);

        var ensured = result.Ensure(x => x > 0, "Must be positive");

        Assert.True(ensured.IsErr);
        Assert.Equal("Must be positive", ensured.UnwrapErr());
    }

    [Fact]
    public void Ensure_ErrResult_PropagatesOriginalError()
    {
        var result = Result<int, string>.Err("original error");

        var ensured = result.Ensure(x => x > 0, "Must be positive");

        Assert.True(ensured.IsErr);
        Assert.Equal("original error", ensured.UnwrapErr());
    }

    [Fact]
    public void Ensure_Chained_AllPredicatesMustPass()
    {
        var result = Result<int, string>.Ok(15)
            .Ensure(x => x > 0, "Must be positive")
            .Ensure(x => x < 100, "Must be under 100")
            .Ensure(x => x % 5 == 0, "Must be divisible by 5");

        Assert.True(result.IsOk);
        Assert.Equal(15, result.Unwrap());
    }

    [Fact]
    public void Ensure_Chained_FailsOnFirstFalse()
    {
        var result = Result<int, string>.Ok(150)
            .Ensure(x => x > 0, "Must be positive")
            .Ensure(x => x < 100, "Must be under 100")
            .Ensure(x => x % 5 == 0, "Must be divisible by 5");

        Assert.True(result.IsErr);
        Assert.Equal("Must be under 100", result.UnwrapErr());
    }
}
