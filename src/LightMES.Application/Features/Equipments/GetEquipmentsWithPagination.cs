using LightMES.Application.Common.Interfaces;
using LightMES.Application.Common.Models;
using LightMES.Application.Features.Equipments.Dtos;
using LightMES.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Equipments;

public record GetEquipmentsWithPaginationQuery : IRequest<PaginatedList<EquipmentDto>>
{
    public string? SearchText { get; init; }
    public EquipmentStatus? Status { get; init; }
    public bool? IsActive { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetEquipmentsWithPaginationQueryHandler
    : IRequestHandler<GetEquipmentsWithPaginationQuery, PaginatedList<EquipmentDto>>
{
    private readonly IAppDbContext _context;

    public GetEquipmentsWithPaginationQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<EquipmentDto>> Handle(
        GetEquipmentsWithPaginationQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _context.Equipments.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var search = request.SearchText.Trim();
            query = query.Where(x =>
                x.EquipmentCode.Contains(search) || x.EquipmentName.Contains(search)
            );
        }
        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }
        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }
        query = query.OrderByDescending(x => x.CreatedOn);
        var dtoQuery = query.Select(x => new EquipmentDto(
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
        ));
        return await PaginatedList<EquipmentDto>.CreateAsync(
            dtoQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );
    }
}

