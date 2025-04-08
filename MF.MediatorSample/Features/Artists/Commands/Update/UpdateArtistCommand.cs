using MiF.Mediator;
using MiF.Mediator.Interfaces;

namespace MiF.MediatorSample.Features.Artists.Commands.Update;

public class UpdateArtistCommand : ICommand<Unit>
{
    public string Name { get; set; } = string.Empty;

    public uint Year { get; set; }
}
