using LightMES.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
            StatusCodes.Status100Continue,
            "Unauthorized",
            exception.Message ?? "用户未登录或 Token 已过期."
            ),
            ForbiddenAccessException => (
            StatusCodes.Status403Forbidden,
            "Forbidden",
            exception.Message ?? "您没有执行该操作的权限."
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
                exception.Message
            ),
            _ => (
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "系统内部错误，请联系管理员."
            )
        };
        if (exception is UnauthorizedAccessException)
        {
            statusCode = StatusCodes.Status401Unauthorized;
        }
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
