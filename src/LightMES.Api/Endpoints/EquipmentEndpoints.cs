using LightMES.Application.Features.Equipments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LightMES.Api.Endpoints;

public static class EquipmentEndpoints
{
    public static void MapEquipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/equipments").WithTags("设备管理 (Equipments)").WithOpenApi();
        group
            .MapPost(
                "/",
                async ([FromBody] CreateEquipmentCommand command, ISender sender) =>
                {
                    var id = await sender.Send(command);
                    return Results.CreatedAtRoute("GetEquipmentById", new { id }, id);
                }
            )
            .WithName("CreateEquipment")
            .WithSummary("创建设备");
        group
            .MapPut(
                "/{id:guid}",
                async (Guid id, [FromBody] UpdateEquipmentCommand command, ISender sender) =>
                {
                    if (id != command.Id)
                        return Results.BadRequest("路径 ID 与请求体 ID 不一致");
                    await sender.Send(command);
                    return Results.NoContent();
                }
            )
            .WithName("UpdateEquipment")
            .WithSummary("更新设备");
        group
            .MapPut(
                "/{id:guid}/status",
                async (Guid id, [FromBody] ChangeEquipmentStatusCommand command, ISender sender) =>
                {
                    if (id != command.Id)
                        return Results.BadRequest("路径 ID 与请求体 ID 不一致");
                    await sender.Send(command);
                    return Results.NoContent();
                }
            )
            .WithName("ChangeEquipmentStatus")
            .WithSummary("修改设备状态");
        group
            .MapPut(
                "/{id:guid}/deactivate",
                async (Guid id, ISender sender) =>
                {
                    await sender.Send(new DeactivateEquipmentCommand(id));
                    return Results.NoContent();
                }
            )
            .WithName("DeactivateEquipment")
            .WithSummary("设备停用");
        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, ISender sender) =>
                {
                    var result = await sender.Send(new GetEquipmentByIdQuery(id));
                    return Results.Ok(result);
                }
            )
            .WithName("GetEquipmentById")
            .WithSummary("按设备ID查询");
        group
            .MapGet(
                "/",
                async ([AsParameters] GetEquipmentsWithPaginationQuery query, ISender sender) =>
                {
                    var results = await sender.Send(query);
                    return Results.Ok(results);
                }
            )
            .WithName("GetEquipments")
            .WithSummary("获取设备列表");
    }
}
