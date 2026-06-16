using LightMES.Application.Common.Interfaces;
using LightMES.Application.Features.Materials.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Materials;

public record GetMaterialsPagedQuery(string? SearchTerm, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult>;

public record PagedResult(List<MaterialDto> Items, int TotalCount, int PageNumber, int PageSize);

public class GetMaterialsPagedQueryHandler : IRequestHandler<GetMaterialsPagedQuery, PagedResult>
{
    private readonly IAppDbContext _context;

    public GetMaterialsPagedQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult> Handle(
        GetMaterialsPagedQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _context.Materials.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(m =>
                m.MaterialCode.ToLower().Contains(search)
                || m.MaterialName.ToLower().Contains(search)
            );
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(m => m.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MaterialDto(
                m.Id,
                m.MaterialCode,
                m.MaterialName,
                m.Specification,
                m.Unit,
                m.MaterialType,
                m.IsActive,
                m.CreatedBy,
                m.CreatedOn,
                m.LastModifiedBy,
                m.LastModifiedOn
            ))
            .ToListAsync(cancellationToken);
        return new PagedResult(items, totalCount, request.PageNumber, request.PageSize);
    }
}
