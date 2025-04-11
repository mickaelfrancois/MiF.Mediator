using MiF.Mediator.Interfaces;

namespace MiF.MediatorSample.Features.Artists.Commands.Add;

public class AddArtistCommand : ICommand<ArtistDto>
{
    public string Name { get; set; } = string.Empty;

    public uint Year { get; set; }
}
