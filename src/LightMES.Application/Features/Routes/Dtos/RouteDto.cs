namespace LightMES.Application.Features.Routes.Dtos;

public record RouteListDto(
    Guid Id,
    string RouteCode,
    string RouteName,
    string Version,
    bool IsActive,
    int StepCount
);

public record RouteStepDetailDto(
    Guid Id,
    string StepCode,
    string StepName,
    int Sequence,
    int StandardCycleTime,
    bool IsRequired,
    string? Description
);

public record RouteDetailDto(
    Guid Id,
    string RouteCode,
    string RouteName,
    string Version,
    bool IsActive,
    List<RouteStepDetailDto> Steps
);
