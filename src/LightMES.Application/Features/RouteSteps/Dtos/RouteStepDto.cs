namespace LightMES.Application.Features.RouteSteps.Dtos;

public record RouteStepDto(
Guid Id,
Guid RouteId,
string StepCode,
string StepName,
int Sequence,
int StandardCycleTime,
bool IsRequired,
string? Description);
