using LightMES.Api.Infrastructure;
using LightMES.Application.Common.Interfaces;
using LightMES.Infrastructure.Persistence;

namespace LightMES.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("LightMES.Infrastructure"));
        });
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        var secretKey = configuration.GetSection("JwtSettings")["secret"];
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("Jwt secret key is missin in configuration!");
        services.AddAuthentication(options =>
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
                    ValidIssuer = configuration.GetSection("JwtSettings")["Issuer"],
                    ValidAudience = configuration.GetSection("JwtSettings")["Audience"],
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
        services.AddAuthorization();
        return services;
    }
}
