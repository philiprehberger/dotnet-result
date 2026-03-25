using Xunit;

namespace Philiprehberger.Result.Tests;

public class ResultTests
{
    [Fact]
    public void Ok_IsOk_ReturnsTrue()
    {
        var result = Result<int, string>.Ok(42);

        Assert.True(result.IsOk);
        Assert.False(result.IsErr);
    }

    [Fact]
    public void Err_IsErr_ReturnsTrue()
    {
        var result = Result<int, string>.Err("error");

        Assert.True(result.IsErr);
        Assert.False(result.IsOk);
    }

    [Fact]
    public void Unwrap_OkResult_ReturnsValue()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void Unwrap_ErrResult_ThrowsInvalidOperationException()
    {
        var result = Result<int, string>.Err("fail");

        Assert.Throws<InvalidOperationException>(() => result.Unwrap());
    }

    [Fact]
    public void UnwrapOr_ErrResult_ReturnsDefault()
    {
        var result = Result<int, string>.Err("fail");

        Assert.Equal(99, result.UnwrapOr(99));
    }

    [Fact]
    public void UnwrapOrElse_ErrResult_CallsFactory()
    {
        var result = Result<int, string>.Err("fail");

        Assert.Equal(4, result.UnwrapOrElse(e => e.Length));
    }

    [Fact]
    public void UnwrapErr_ErrResult_ReturnsError()
    {
        var result = Result<int, string>.Err("error");

        Assert.Equal("error", result.UnwrapErr());
    }

    [Fact]
    public void Map_OkResult_TransformsValue()
    {
        var result = Result<int, string>.Ok(10);

        var mapped = result.Map(v => v * 2);

        Assert.Equal(20, mapped.Unwrap());
    }

    [Fact]
    public void MapErr_ErrResult_TransformsError()
    {
        var result = Result<int, string>.Err("err");

        var mapped = result.MapErr(e => e.Length);

        Assert.Equal(3, mapped.UnwrapErr());
    }

    [Fact]
    public void FlatMap_OkResult_ChainsResult()
    {
        var result = Result<int, string>.Ok(5);

        var chained = result.FlatMap(v => Result<string, string>.Ok(v.ToString()));

        Assert.Equal("5", chained.Unwrap());
    }

    [Fact]
    public void Match_OkResult_CallsOnOk()
    {
        var result = Result<int, string>.Ok(42);

        var output = result.Match(v => $"ok:{v}", e => $"err:{e}");

        Assert.Equal("ok:42", output);
    }

    [Fact]
    public void Match_ErrResult_CallsOnErr()
    {
        var result = Result<int, string>.Err("fail");

        var output = result.Match(v => $"ok:{v}", e => $"err:{e}");

        Assert.Equal("err:fail", output);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(3, false)]
    public void Filter_PredicateResult_ReturnsCorrectly(int value, bool expectedOk)
    {
        var result = Result<int, string>.Ok(value)
            .Filter(v => v > 5, v => $"{v} is too small");

        Assert.Equal(expectedOk, result.IsOk);
    }

    [Fact]
    public void Expect_ErrResult_ThrowsWithCustomMessage()
    {
        var result = Result<int, string>.Err("bad");

        var ex = Assert.Throws<InvalidOperationException>(() => result.Expect("should be ok"));
        Assert.Contains("should be ok", ex.Message);
    }

    [Fact]
    public void ToString_OkResult_FormatsCorrectly()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Equal("Ok(42)", result.ToString());
    }

    [Fact]
    public void ToString_ErrResult_FormatsCorrectly()
    {
        var result = Result<int, string>.Err("fail");

        Assert.Equal("Err(fail)", result.ToString());
    }
}
