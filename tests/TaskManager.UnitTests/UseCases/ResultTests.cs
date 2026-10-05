using TaskManager.Application.Common.Results;

namespace TaskManager.UnitTests.UseCases;

public sealed class ResultTests
{
    [Fact]
    public void Success_ExposesValue()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_ExposesErrorAndBlocksValueAccess()
    {
        Result<int> result = Error.NotFound("X.NotFound", "não encontrado");

        Assert.False(result.IsSuccess);
        Assert.Equal("X.NotFound", result.Error.Code);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
