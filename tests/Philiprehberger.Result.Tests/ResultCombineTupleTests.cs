using Xunit;

namespace Philiprehberger.Result.Tests;

public class ResultCombineTupleTests
{
    [Fact]
    public void Combine3_AllOk_ReturnsTuple()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<string, string>.Ok("two");
        var r3 = Result<double, string>.Ok(3.0);

        var combined = Result.Combine(r1, r2, r3);

        Assert.True(combined.IsOk);
        var (v1, v2, v3) = combined.Unwrap();
        Assert.Equal(1, v1);
        Assert.Equal("two", v2);
        Assert.Equal(3.0, v3);
    }

    [Fact]
    public void Combine3_SecondIsErr_ReturnsFirstErr()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<string, string>.Err("fail2");
        var r3 = Result<double, string>.Ok(3.0);

        var combined = Result.Combine(r1, r2, r3);

        Assert.True(combined.IsErr);
        Assert.Equal("fail2", combined.UnwrapErr());
    }

    [Fact]
    public void Combine4_AllOk_ReturnsTuple()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<int, string>.Ok(2);
        var r3 = Result<int, string>.Ok(3);
        var r4 = Result<int, string>.Ok(4);

        var combined = Result.Combine(r1, r2, r3, r4);

        Assert.True(combined.IsOk);
        var (v1, v2, v3, v4) = combined.Unwrap();
        Assert.Equal(1, v1);
        Assert.Equal(2, v2);
        Assert.Equal(3, v3);
        Assert.Equal(4, v4);
    }

    [Fact]
    public void Combine4_ThirdIsErr_ReturnsFirstErr()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<int, string>.Ok(2);
        var r3 = Result<int, string>.Err("fail3");
        var r4 = Result<int, string>.Ok(4);

        var combined = Result.Combine(r1, r2, r3, r4);

        Assert.True(combined.IsErr);
        Assert.Equal("fail3", combined.UnwrapErr());
    }

    [Fact]
    public void Combine5_AllOk_ReturnsTuple()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<int, string>.Ok(2);
        var r3 = Result<int, string>.Ok(3);
        var r4 = Result<int, string>.Ok(4);
        var r5 = Result<int, string>.Ok(5);

        var combined = Result.Combine(r1, r2, r3, r4, r5);

        Assert.True(combined.IsOk);
        var (v1, v2, v3, v4, v5) = combined.Unwrap();
        Assert.Equal(1, v1);
        Assert.Equal(2, v2);
        Assert.Equal(3, v3);
        Assert.Equal(4, v4);
        Assert.Equal(5, v5);
    }

    [Fact]
    public void Combine5_FirstIsErr_ReturnsFirstErr()
    {
        var r1 = Result<int, string>.Err("fail1");
        var r2 = Result<int, string>.Ok(2);
        var r3 = Result<int, string>.Ok(3);
        var r4 = Result<int, string>.Ok(4);
        var r5 = Result<int, string>.Ok(5);

        var combined = Result.Combine(r1, r2, r3, r4, r5);

        Assert.True(combined.IsErr);
        Assert.Equal("fail1", combined.UnwrapErr());
    }

    [Fact]
    public void Combine3_MultipleErrs_ReturnsFirstErr()
    {
        var r1 = Result<int, string>.Err("fail1");
        var r2 = Result<string, string>.Err("fail2");
        var r3 = Result<double, string>.Err("fail3");

        var combined = Result.Combine(r1, r2, r3);

        Assert.True(combined.IsErr);
        Assert.Equal("fail1", combined.UnwrapErr());
    }
}
