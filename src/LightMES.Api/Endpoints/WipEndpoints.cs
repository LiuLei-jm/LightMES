using LightMES.Application.Features.Wip;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LightMES.Api.Endpoints;

public static class WipEndpoints
{
    public static void MapWipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wips").WithTags("在制品 (Wips)");
        group.MapPost("/track-in", async ([FromBody] TrackInCommand command, ISender sender) =>
        {
            var trackWipId = await sender.Send(command);
            return Results.Ok(new TrackInResult(true, trackWipId, "进站成功"));
        }).WithName("TrackIn");
        group.MapPost("/track-out", async ([FromBody] TrackOutCommand command, ISender sender) =>
        {
            var unit = await sender.Send(command);
            return Results.Ok(new TrackOutResult(true, unit, "出站成功"));
        }).WithName("TrackOut");
    }
}
