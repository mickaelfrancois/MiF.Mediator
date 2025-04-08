using MiF.Mediator;
using MiF.Mediator.Interfaces;

namespace MiF.MediatorSample.Features.Artists.Commands.Update;

public class UpdateArtistCommandHandler : ICommandHandler<UpdateArtistCommand>
{
    public Task<Unit> HandleAsync(UpdateArtistCommand message, CancellationToken cancellationToken)
    {
        return Task.FromResult(Unit.Result);
    }
}
