using LightMES.Application.Features.RouteSteps;
using MediatR;

namespace LightMES.Api.Endpoints;

public static class RouteStepEndpoints
{
    public static void MapRouteStepEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/route-steps")
            .WithTags("工序步骤 (RouteSteps)")
            .WithOpenApi();
        group
            .MapPost(
                "/",
                async (CreateRouteStepCommand command, ISender sender) =>
                {
                    var id = await sender.Send(command);
                    return Results.Created($"/api/route-steps/{id}", id);
                }
            )
            .WithName("CreateRouteStep")
            .WithSummary("创建工艺");
        group
            .MapPut(
                "/{id:guid}",
                async (Guid id, UpdateRouteStepCommand command, ISender sender) =>
                {
                    if (id != command.Id)
                        return Results.BadRequest("路由ID不匹配");
                    await sender.Send(command);
                    return Results.NoContent();
                }
            )
            .WithName("UpdateRouteStep")
            .WithSummary("更新工艺");
        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, ISender sender) =>
                {
                    await sender.Send(new DeleteRouteStepCommand(id));
                    return Results.NoContent();
                }
            )
            .WithName("DeleteRouteStep")
            .WithSummary("删除工艺");
        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, ISender sender) =>
                {
                    try
                    {
                        var result = await sender.Send(new GetRouteStepByIdQuery(id));
                        return Results.Ok(result);
                    }
                    catch (KeyNotFoundException)
                    {
                        return Results.NotFound(0);
                    }
                }
            )
            .WithName("GetRouteStepById")
            .WithSummary("通过ID获取工艺");
        group
            .MapGet(
                "/by-route/{routeId:guid}",
                async (Guid routeId, ISender sender) =>
                {
                    var result = await sender.Send(new GetRouteStepsByRouteIdQuery(routeId));
                    return Results.Ok(result);
                }
            )
            .WithName("GetRouteStepByRouteId")
            .WithSummary("通过RouteId获取工艺");
    }
}
