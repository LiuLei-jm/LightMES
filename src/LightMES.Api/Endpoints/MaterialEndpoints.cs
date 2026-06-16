using LightMES.Application.Features.Materials;
using MediatR;

namespace LightMES.Api.Endpoints;

public static class MaterialEndpoints
{
    public static void MapMaterialEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/materials").WithTags("Materials (物料主数据)").WithOpenApi();
        group
            .MapPost(
                "/",
                async (CreateMaterialCommand command, ISender sender) =>
                {
                    var results = await sender.Send(command);
                    return Results.Ok(results);
                }
            )
            .WithName("GetMateraitls")
            .WithSummary("获取物料表");
        group
            .MapPut(
                "/{id:guid}",
                async (Guid id, UpdateMaterialCommand command, ISender sender) =>
                {
                    var result = await sender.Send(command);
                    return Results.NoContent();
                }
            )
            .WithName("UpdateMaterial")
            .WithSummary("更新物料");
        group
            .MapPut(
                "{id:guid}/activate",
                async (Guid id, ISender sender) =>
                {
                    await sender.Send(new ActivateMaterialCommand(id));
                    return Results.NoContent();
                }
            )
            .WithName("ActivateMaterial")
            .WithSummary("激活材料");
        group
            .MapPut(
                "/{id:guid}/deactivate",
                async (Guid id, ISender sender) =>
                {
                    await sender.Send(new DeactivateMaterialCommand(id));
                    return Results.NoContent();
                }
            )
            .WithName("DeactivateMaterial")
            .WithSummary("停用材料");
        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, ISender sender) =>
                {
                    var material = await sender.Send(new GetMaterialByIdQuery(id));
                    return material is not null ? Results.Ok(material) : Results.NotFound();
                }
            )
            .WithName("GetMaterialById")
            .WithSummary("通过ID获取材料");
    }
}
