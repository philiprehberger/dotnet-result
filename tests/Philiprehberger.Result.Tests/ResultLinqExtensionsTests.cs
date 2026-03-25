using Xunit;

namespace Philiprehberger.Result.Tests;

public class ResultLinqExtensionsTests
{
    [Fact]
    public void Select_OkResult_ProjectsValue()
    {
        var result = Result<int, string>.Ok(10);

        var projected = result.Select(v => v * 2);

        Assert.Equal(20, projected.Unwrap());
    }

    [Fact]
    public void Select_ErrResult_PropagatesError()
    {
        var result = Result<int, string>.Err("fail");

        var projected = result.Select(v => v * 2);

        Assert.True(projected.IsErr);
    }

    [Fact]
    public void SelectMany_QuerySyntax_ChainsResults()
    {
        var a = Result<int, string>.Ok(10);
        var b = Result<int, string>.Ok(20);

        var combined =
            from x in a
            from y in b
            select x + y;

        Assert.Equal(30, combined.Unwrap());
    }

    [Fact]
    public void SelectMany_ErrInChain_PropagatesError()
    {
        var a = Result<int, string>.Ok(10);
        var b = Result<int, string>.Err("fail");

        var combined =
            from x in a
            from y in b
            select x + y;

        Assert.True(combined.IsErr);
        Assert.Equal("fail", combined.UnwrapErr());
    }
}
