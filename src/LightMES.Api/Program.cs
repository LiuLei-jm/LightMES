using LightMES.Api.Endpoints;
using LightMES.Api.Infrastructure;
using LightMES.Api.Middlewares;
using LightMES.Application;
using LightMES.Application.Common.Interfaces;
using LightMES.Infrastructure;
using LightMES.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("LightMES.Infrastructure"));
});
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var secretKey = builder.Configuration.GetSection("JwtSettings")["secret"];
if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("Jwt secret key is missin in configuration!");
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetSection("JwtSettings")["Issuer"],
            ValidAudience = builder.Configuration.GetSection("JwtSettings")["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[JWT验证失败]:{context.Exception.Message}");
                if (context.Exception.InnerException != null)
                {
                    Console.WriteLine(
                        $"[JWT验证失败详情]:{context.Exception.InnerException.Message}"
                    );
                }
                Console.ResetColor();
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[JWT验证成功]:用户已成功登录！");
                Console.ResetColor();
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();

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
