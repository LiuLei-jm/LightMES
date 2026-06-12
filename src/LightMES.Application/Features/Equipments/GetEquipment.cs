using LightMES.Application.Common.Interfaces;
using LightMES.Application.Features.Equipments.Dtos;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Equipments;

public record GetEquipmentByIdQuery(Guid Id) : IRequest<EquipmentDto>;

public class GetEquipmentByIdQueryHandler : IRequestHandler<GetEquipmentByIdQuery, EquipmentDto>
{
    private readonly IAppDbContext _context;

    public GetEquipmentByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<EquipmentDto> Handle(
        GetEquipmentByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var dto = await _context
            .Equipments.AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new EquipmentDto(
                x.Id,
                x.EquipmentCode,
                x.EquipmentName,
                x.Status,
                x.Status.ToString(),
                x.Location,
                x.Description,
                x.IsActive,
                x.CreatedBy!,
                x.CreatedOn,
                x.LastModifiedBy,
                x.LastModifiedOn
            ))
            .FirstOrDefaultAsync(cancellationToken);
        return dto == null ? throw new KeyNotFoundException(nameof(Equipment)) : dto;
    }
}

