using LightMES.Api;
using LightMES.Api.Endpoints;
using LightMES.Api.Middlewares;
using LightMES.Application;
using LightMES.Application.Common.Interfaces;
using LightMES.Infrastructure;
using LightMES.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApiServices(builder.Configuration);


var app = builder.Build();

await SeedDatabaseAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();
app.MapWorkOrderEndpoints();
app.MapRouteEndpoints();
app.MapRouteStepEndpoints();
app.MapWipEndpoints();
app.MapEquipmentEndpoints();
app.MapMaterialEndpoints();

try
{
    Log.Information("正在启动 LightMES WebApi...");
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "LightMES 启动失败！");
}
finally
{
    Log.CloseAndFlush();
}
async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        logger.LogInformation("正在初始化 MES 默认种子数据...");
        await AppDbContextSeed.SeedDefaultDataAsync(context, passwordHasher);
        logger.LogInformation("种子数据初始化成功！");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "初始化数据库种子数据时发生错误!");
        throw;
    }
}

// Provides a symbol for integration tests that reference the program type.
// For minimal/top-level Program.cs projects, add this partial class so tests can use typeof(Program).
public partial class Program { }
