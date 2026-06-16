using LightMES.Application.Features.Routes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LightMES.Api.Endpoints;

public static class RouteEndpoints
{
    public static void MapRouteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/routes").WithTags("工艺路线 (Routes)").WithOpenApi();
        group
            .MapPost(
                "/",
                async ([FromBody] CreateRouteCommand command, ISender sender) =>
                {
                    var routeId = await sender.Send(command);
                    return Results.Created("/api/routes/{routeId}", new { RouteId = routeId });
                }
            )
            .WithName("CreateRoute").WithSummary("创建工序");
        _ = group
            .MapPut(
                "/{id:guid}",
                async (Guid id, [FromBody] UpdateRouteCommand command, ISender sender) =>
                {
                    if (id != command.Id)
                        return Results.BadRequest("请求的 ID 与路由主键不一致");
                    try
                    {
                        await sender.Send(command);
                        return Results.NoContent();
                    }
                    catch (KeyNotFoundException)
                    {
                        return Results.NotFound();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Conflict(new { ex.Message });
                    }
                }
            )
            .WithName("UpdateRoute").WithSummary("更新工序");
        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, ISender sender) =>
                {
                    try
                    {
                        await sender.Send(new DeleteRouteCommand(id));
                        return Results.NoContent();
                    }
                    catch (KeyNotFoundException)
                    {
                        return Results.NotFound();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { ex.Message });
                    }
                }
            )
            .WithName("DeleteRoute").WithSummary("删除工序");
        group
            .MapGet(
                "/",
                async (ISender sender) =>
                {
                    var routes = await sender.Send(new GetRoutesQuery());
                    return Results.Ok(routes);
                }
            )
            .WithName("GetRoutes").WithSummary("获取所有工序");
        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, ISender sender) =>
                {
                    try
                    {
                        var routeDetail = await sender.Send(new GetRouteByIdQuery(id));
                        return Results.Ok(routeDetail);
                    }
                    catch (KeyNotFoundException)
                    {
                        return Results.NotFound();
                    }
                }
            )
            .WithName("GetRouteDetails").WithSummary("通过ID获取工序详情");
    }
}
