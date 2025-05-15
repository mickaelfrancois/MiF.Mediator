using MiF.Mediator.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MiF.MediatorSample.Features.Artists.Commands.Add;

public class AddArtistCommand : ICommand<ArtistDto>
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public uint Year { get; set; }
}
