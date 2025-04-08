using MiF.Mediator.Interfaces;

namespace MiF.MediatorSample.Features;

public class QueryPreProcessor<TMessage, TResponse>(ILogger<TMessage> _logger) : IRequestPreProcessor<TMessage, TResponse> where TMessage : IRequest<TResponse>
{
    public async Task<TResponse> RunAsync(TMessage message, HandleRequestDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pre " + message.ToString());

        return await next.Invoke(message, cancellationToken);
    }
}