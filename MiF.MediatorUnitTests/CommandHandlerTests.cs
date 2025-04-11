using MiF.Mediator.Interfaces;

namespace MiF.MediatorUnitTests;

public class CommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturn42_WhenCommandIsValid()
    {
        // Arrange
        SampleCommand command = new() { CommandData = "TestCommand" };
        SampleCommandHandler handler = new();

        // Act
        int result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert        
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentNullException_WhenCommandDataIsNull()
    {
        // Arrange
        SampleCommand command = new() { CommandData = null };
        SampleCommandHandler handler = new();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(command, CancellationToken.None));
    }
}


public class SampleCommand : IRequest<int>
{
    public string? CommandData { get; set; } = string.Empty;
}

public class SampleCommandHandler : ICommandHandler<SampleCommand, int>
{
    public Task<int> HandleAsync(SampleCommand command, CancellationToken cancellationToken)
    {
        if (command.CommandData == null)
            throw new ArgumentNullException(nameof(command.CommandData));

        int response = 42;

        return Task.FromResult(response);
    }
}