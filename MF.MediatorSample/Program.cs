using Microsoft.AspNetCore.Mvc;
using MiF.Mediator.DependencyInjection;
using MiF.Mediator.Interfaces;
using MiF.MediatorSample.Features.Artists;
using MiF.MediatorSample.Features.Artists.Commands.Add;
using MiF.MediatorSample.Features.Artists.Commands.Update;
using MiF.MediatorSample.Features.Artists.Query.List;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddSimpleMediator();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();


app.MapGet("/artists", async ([FromServices] ILogger<Program> logger, [FromServices] IMediator mediator) =>
    {
        ListArtistsQueryResponse response = await mediator.SendMessageAsync(new ListArtistsQuery());
        return TypedResults.Ok(response.Artists);
    })
.WithName("GetArtists");

app.MapPost("/artists", async ([FromServices] ILogger<Program> logger, [FromServices] IMediator mediator) =>
{
    ArtistDto artistCreated = await mediator.SendMessageAsync(new AddArtistCommand() { Name = "Bon jovi", Year = 1983 });
    return TypedResults.Ok(artistCreated);
})
.WithName("AddArtist");

app.MapPut("/artists", async ([FromServices] ILogger<Program> logger, [FromServices] IMediator mediator) =>
{
    await mediator.SendMessageAsync(new UpdateArtistCommand() { Name = "Bon jovi", Year = 1985 });
    return TypedResults.NoContent();
})
.WithName("UpdateArtist");

app.Run();



