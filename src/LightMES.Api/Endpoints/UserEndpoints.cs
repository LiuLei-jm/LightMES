using LightMES.Application.Features.Roles;
using LightMES.Application.Features.Users;
using LightMES.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LightMES.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("用户与权限控制（Users）").WithOpenApi();
        group.MapPost("/register", async ([FromBody] RegisterUserCommand command, ISender sender) =>
        {
            try
            {
                var userId = await sender.Send(command);
                return Results.Created($"/api/users/{userId}", new { UserId = userId });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }).WithName("Register").WithSummary("注册用户");
        group.MapPost("/login", async ([FromBody] LoginUserCommand request, ISender sender) =>
        {
            var result = await sender.Send(request);
            return result.Success ? Results.Ok(result) : Results.Json(result, statusCode: 401);
        }).WithName("Login").WithSummary("用户登录");
        group.MapGet("/", async ([FromQuery] bool? activeOnly, ISender sender) =>
        {
            var users = await sender.Send(new GetUsersQuery(activeOnly));
            return Results.Ok(users);
        }).WithName("GetUsers").WithSummary("获取所有用户");
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var user = await sender.Send(new GetUserByIdQuery(id));
            return Results.Ok(user);
        }).WithName("GetUserById").WithSummary("通过Id获取用户");
        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateUserCommand command, ISender sender) =>
        {
            if (id != command.Id) return Results.BadRequest("路由 ID 不匹配");
            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound("未找到该用户");
        }).WithName("UpdateUser").WithSummary("更新用户");
        group.MapPut("/me/changepassword", async ([FromBody] ChangeUserPasswordCommand command, ISender sender) =>
        {
            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound("未找到该用户");
        }).WithName("ChangePassword").WithSummary("用户更改自己的密码");
        group.MapPut("/{id:guid}/resetpassword", async (Guid id, [FromBody] string newPassword, ISender sender) =>
        {
            await sender.Send(new ResetUserPasswordCommand(id, newPassword));
            return Results.NoContent();
        }).WithName("ResetPassword").WithSummary("管理员重置用户密码");
        group.MapPatch("/{id:guid}/status", async (Guid id, [FromBody] bool isActive, ISender sender) =>
        {
            var success = await sender.Send(new ToggleUserStatusCommand(id, isActive));
            return success ? Results.Ok($"用户状态已变更为：{(isActive ? "活动" : "禁止")}") : Results.NotFound("未找到该用户");
        }).WithName("ToggleUserStatus").WithSummary("切换用户状态");
        group.MapDelete("/{id:guid}", async (Guid id, [FromBody] DeleteUserCommand command, ISender sender) =>
        {
            if (id != command.Id) return Results.BadRequest("路由 ID 不匹配");
            var result = await sender.Send(command);
            return result ? Results.NoContent() : Results.NotFound();
        }).WithName("DeleteUser").WithSummary("删除用户");
        group.MapPost("/roles", async ([FromBody] CreateRoleCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/users/roles/{id}", new { Id = id });
        }).WithName("CreateRole").WithSummary("创建角色");
        group.MapGet("/roles", async (ISender sender) =>
        {
            var roles = await sender.Send(new GetRolesQuery());
            return Results.Ok(roles);
        }).WithName("GetRoles").WithSummary("获取所有角色");
        group.MapPut("/roles/{id:guid}", async (Guid id, [FromBody] UpdateRoleCommand command, ISender sender) =>
        {
            if (id != command.Id) return Results.BadRequest("路由 ID 不匹配");
            var result = await sender.Send(command);
            return result ? Results.NoContent() : Results.NotFound("未找到该角色");
        }).WithName("UpdateRole").WithSummary("更新角色");
        group.MapDelete("/roles/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRoleCommand(id));
        }).WithName("DeleteRole").WithSummary("删除角色");
        group.MapPost("/{userId:guid}/roles/{roleId:guid}", async (Guid userId, Guid roleId, ISender sender) =>
        {
            await sender.Send(new BindUserRoleCommand(userId, roleId));
            return Results.NoContent();
        }).WithName("BindUserRole").WithSummary("绑定用户角色");
        group.MapDelete("/{userId:guid}/roles/{roleId:guid}", async (Guid userId, Guid roleId, ISender sender) =>
        {
            await sender.Send(new UnbindUserRoleCommand(userId, roleId));
            return Results.NoContent();
        }).WithName("UnbindUserRole").WithSummary("解绑用户角色");
        group.MapPost("/roles/{roleId:guid}/permissions", async (Guid roleId, [FromBody] string permission, ISender sender) =>
        {
            await sender.Send(new BindRolePermissionCommand(roleId, permission));
            return Results.NoContent();
        }).WithName("BindRolePermission").WithSummary("绑定角色权限");
        group.MapDelete("/roles/{roleId:guid}/permissions", async (Guid roleId, string permission, ISender sender) =>
        {
            await sender.Send(new UnbindRolePermissionCommand(roleId, permission));
            return Results.NoContent();
        }).WithName("UnbindRolePermission").WithSummary("解绑角色权限");
        group.MapGet("/permissions", () =>
        {
            var permissions = Permissions.GetAll();
            return Results.Ok(permissions);
        }).WithName("GetAllPermissions").WithSummary("获取系统所有定义的权限列表");
    }
}
