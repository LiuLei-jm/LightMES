using LightMES.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LightMES.Api.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An Unhandled exception occureed: {Message}", exception.Message);
        var (statusCode, title, detail) = exception switch
        {
            UnauthorizedAccessException => (
            StatusCodes.Status401Unauthorized,
            "Unauthorized",
            exception.Message ?? "用户未登录或 Token 已过期."
            ),
            ForbiddenAccessException => (
            StatusCodes.Status403Forbidden,
            "Forbidden",
            exception.Message ?? "您没有执行该操作的权限."
            ),
            KeyNotFoundException => (
            StatusCodes.Status404NotFound,
            "Not Found",
            exception.Message ?? "请求的资源未找到"
            ),
            ArgumentException or InvalidOperationException =>
            (
            StatusCodes.Status400BadRequest,
            "Bad Request",
            exception.Message
            ),
            FluentValidation.ValidationException => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                "输入的数据未通过系统验证，请修改后重试."
            ),
            DbUpdateException dbUpdateEx when dbUpdateEx.InnerException is PostgresException pgEx => pgEx.SqlState switch
            {
                "23505" =>(
                    StatusCodes.Status409Conflict,
                    "Data Conflict",
                    "您提交的数据在系统中已存在（编码、工号或条码重复），请检查后重试."
                ),
                "23503" =>(
                    StatusCodes.Status400BadRequest,
                    "Data Association Error",
                    "操作失败：由于关联的数据（如用户、设备或工艺）已被引用，无法执行此操作."
                ),
                _ =>(
                    StatusCodes.Status400BadRequest,
                    "Database Constraint Violation",
                    $"数据库约束冲突：{pgEx.MessageText}"
                )
            },
            _ => (
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "系统内部错误，请联系管理员."
            )
        };
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (exception is FluentValidation.ValidationException validationException)
        {
            var errors = validationException.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            problemDetails.Extensions.Add("errors", errors);
        }
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
