using Xunit;

namespace Philiprehberger.Result.Tests;

public class ResultAsyncTests
{
    [Fact]
    public async Task MapAsync_OkResult_TransformsValueAsync()
    {
        var result = Result<int, string>.Ok(5);

        var mapped = await result.MapAsync(v => Task.FromResult(v * 10));

        Assert.True(mapped.IsOk);
        Assert.Equal(50, mapped.Unwrap());
    }

    [Fact]
    public async Task MapAsync_ErrResult_PropagatesError()
    {
        var result = Result<int, string>.Err("fail");

        var mapped = await result.MapAsync(v => Task.FromResult(v * 10));

        Assert.True(mapped.IsErr);
        Assert.Equal("fail", mapped.UnwrapErr());
    }

    [Fact]
    public async Task FlatMapAsync_OkResult_ChainsAsync()
    {
        var result = Result<int, string>.Ok(5);

        var chained = await result.FlatMapAsync(v =>
            Task.FromResult(Result<string, string>.Ok($"value:{v}")));

        Assert.True(chained.IsOk);
        Assert.Equal("value:5", chained.Unwrap());
    }

    [Fact]
    public async Task FlatMapAsync_ErrResult_PropagatesError()
    {
        var result = Result<int, string>.Err("fail");

        var chained = await result.FlatMapAsync(v =>
            Task.FromResult(Result<string, string>.Ok($"value:{v}")));

        Assert.True(chained.IsErr);
        Assert.Equal("fail", chained.UnwrapErr());
    }

    [Fact]
    public async Task FlatMapAsync_OkResult_InnerErr_ReturnsErr()
    {
        var result = Result<int, string>.Ok(5);

        var chained = await result.FlatMapAsync(v =>
            Task.FromResult(Result<string, string>.Err("inner fail")));

        Assert.True(chained.IsErr);
        Assert.Equal("inner fail", chained.UnwrapErr());
    }

    [Fact]
    public async Task MatchAsync_OkResult_CallsOnSuccess()
    {
        var result = Result<int, string>.Ok(42);

        var output = await result.MatchAsync(
            v => Task.FromResult($"ok:{v}"),
            e => Task.FromResult($"err:{e}"));

        Assert.Equal("ok:42", output);
    }

    [Fact]
    public async Task MatchAsync_ErrResult_CallsOnFailure()
    {
        var result = Result<int, string>.Err("fail");

        var output = await result.MatchAsync(
            v => Task.FromResult($"ok:{v}"),
            e => Task.FromResult($"err:{e}"));

        Assert.Equal("err:fail", output);
    }
}
