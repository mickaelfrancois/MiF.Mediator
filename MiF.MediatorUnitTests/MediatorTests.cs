using MiF.Mediator.Interfaces;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace MiF.MediatorUnitTests;

public class MediatorTests
{
    [Fact]
    public async Task HandleAsync_ShouldInvokeRequestProcessorAndReturnResponse()
    {
        // Arrange
        var mockServiceFactory = new Mock<IServiceFactory>();
        var mockRequestProcessor = new Mock<IRequestProcessor<SampleRequest, string>>();
        var request = new SampleRequest { RequestData = "" };

        mockRequestProcessor
            .Setup(processor => processor.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Processed: TestRequest");

        mockServiceFactory
            .Setup(factory => factory.GetInstance(typeof(IRequestProcessor<SampleRequest, string>)))
            .Returns(mockRequestProcessor.Object);

        var mediator = new MiF.Mediator.Mediator(mockServiceFactory.Object);

        // Act
        var result = await mediator.SendMessageAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Processed: TestRequest", result);
        mockRequestProcessor.Verify(processor => processor.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenProcessorNotFound()
    {
        // Arrange
        var mockServiceFactory = new Mock<IServiceFactory>();
        var request = new SampleRequest { RequestData = "TestRequest" };

        mockServiceFactory
            .Setup(factory => factory.GetInstance(typeof(IRequestProcessor<SampleRequest, string>)))
            .Throws(new ArgumentException("Processor not found"));

        var mediator = new MiF.Mediator.Mediator(mockServiceFactory.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => mediator.SendMessageAsync(request, CancellationToken.None));
    }
}

public class SampleRequest : IRequest<string>
{
    [Required]
    public string? RequestData { get; set; } = string.Empty;
}