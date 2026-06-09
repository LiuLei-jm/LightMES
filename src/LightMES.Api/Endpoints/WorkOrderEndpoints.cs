using LightMES.Application.Features.WorkOrders;
using LightMES.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public static class WorkOrderEndpoints
{
    public static void MapWorkOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workorders").WithTags("工单业务 (Work Orders)");
        group.MapGet("/", async (
            [FromQuery] OrderStatus? status,
            [FromQuery] string? searchTerm,
            [FromQuery] int? pageSize,
            [FromQuery] int? pageNumber,
            ISender sender) =>
        {
            var query = new GetWorkOrderQuery(status, searchTerm, pageSize ?? 10, pageNumber ?? 1);
            var result = await sender.Send(query);
            return Results.Ok(result);
        }).WithName("GetWorkOrders");
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetWorkOrderDetailQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound("未找到工单");
        }).WithName("GetWorkOrderDetail");
        group.MapPost("/", async ([FromBody] CreateWorkOrderCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
            ? Results.Created($"/api/workorders/{result.Value}", result.Value)
            : Results.BadRequest(result.Error);
        }).WithName("CreateWorkOrder");
        group.MapPost("/sync-erp", async ([FromBody] SyncErpWorkOrderCommand command, ISender sender) =>
        {
            var success = await sender.Send(command);
            return success ? Results.Ok("ERP工单同步成功") : Results.BadRequest("同步失败，请检查工艺路线及防重");
        }).WithName("SyncERPWorkOrder");
        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateWorkOrderCommand command, ISender sender) =>
        {
            if (id != command.Id) return Results.BadRequest("路由 ID 与请求体 ID 不一致");
            var result = await sender.Send(command);
            return result.Success ? Results.NoContent() : Results.BadRequest(result.ErrorMessage);
        }).WithName("UpdateWorkOrder");
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new CancelWorkOrderCommand(id));
            return result.Success ? Results.Ok("工单已成功作废") : Results.BadRequest(result.ErrorMessage);
        }).WithName("CancelWorkOrder");
    }
}
