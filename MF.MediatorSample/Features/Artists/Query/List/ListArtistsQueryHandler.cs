using MiF.Mediator;

namespace MiF.MediatorSample.Features.Artists.Query.List;

public class ListArtistsQueryHandler : QueryHandler<ListArtistsQuery, ListArtistsQueryResponse>
{
    protected override Task<ListArtistsQueryResponse> HandleQueryAsync(ListArtistsQuery query, CancellationToken cancellationToken)
    {
        ListArtistsQueryResponse response = new();

        response.Artists.AddRange(
            "Metallica",
            "Iron Maiden",
            "Black Sabbath",
            "Judas Priest",
            "AC/DC",
            "Led Zeppelin"
        );

        return Task.FromResult(response);
    }
}
