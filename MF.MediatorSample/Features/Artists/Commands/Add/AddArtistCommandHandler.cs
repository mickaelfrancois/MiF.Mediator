using MiF.Mediator.Interfaces;

namespace MiF.MediatorSample.Features.Artists.Commands.Add;

public class AddArtistCommandHandler : ICommandHandler<AddArtistCommand, ArtistDto>
{
    public Task<ArtistDto> HandleAsync(AddArtistCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ArtistDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Year = request.Year,
        });
    }
}
