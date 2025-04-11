using MiF.Mediator;

namespace MiF.MediatorUnitTests;

public class UnitTests
{
    [Fact]
    public void Result_ShouldReturnNewInstance()
    {
        // Act
        var result = Unit.Result;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Unit>(result);
    }
}
