using Xunit;

namespace Philiprehberger.Result.Tests;

public class ResultTapErrorTests
{
    [Fact]
    public void TapError_ErrResult_ExecutesAction()
    {
        var result = Result<int, string>.Err("fail");
        string? captured = null;

        result.TapError(e => captured = e);

        Assert.Equal("fail", captured);
    }

    [Fact]
    public void TapError_OkResult_DoesNotExecuteAction()
    {
        var result = Result<int, string>.Ok(42);
        var executed = false;

        result.TapError(_ => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void TapError_ReturnsSameResult()
    {
        var result = Result<int, string>.Err("fail");

        var returned = result.TapError(_ => { });

        Assert.True(returned.IsErr);
        Assert.Equal("fail", returned.UnwrapErr());
    }

    [Fact]
    public void Tap_And_TapError_ChainedInPipeline()
    {
        string? okLog = null;
        string? errLog = null;

        var result = Result<int, string>.Ok(42)
            .Tap(v => okLog = $"value:{v}")
            .TapError(e => errLog = $"error:{e}")
            .Map(x => x * 2);

        Assert.Equal("value:42", okLog);
        Assert.Null(errLog);
        Assert.Equal(84, result.Unwrap());
    }
}
