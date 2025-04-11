using MiF.Mediator.Interfaces;

namespace MiF.MediatorUnitTests;

public class QueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnProcessedResult()
    {
        // Arrange
        SampleQuery query = new() { QueryData = "TestQuery" };
        SampleQueryHandler handler = new();

        // Act
        string result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Processed: TestQuery", result);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleNullQueryData()
    {
        // Arrange
        SampleQuery query = new() { QueryData = null };
        SampleQueryHandler handler = new();

        // Act
        string result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Processed: ", result);
    }
}


public class SampleQuery : IRequest<string>
{
    public string? QueryData { get; set; } = string.Empty;
}

public class SampleQueryHandler : IQueryHandler<SampleQuery, string>
{
    public Task<string> HandleAsync(SampleQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Processed: {query.QueryData}");
    }
}