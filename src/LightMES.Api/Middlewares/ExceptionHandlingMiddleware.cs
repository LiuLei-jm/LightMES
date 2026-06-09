using FluentValidation;
using System.Text.Json;

namespace LightMES.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "服务器发生未处理异常：{Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        if (ex is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = validationException.Errors
                .Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage });
            return context.Response.WriteAsync(JsonSerializer.Serialize(new { Error = "输入验证失败", Details = errors }));
        }
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { Error = "系统内部错误，请联系管理员" }));
    }
}
