using LightMES.Application.Common.Interfaces;
using LightMES.Application.Features.Materials.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Materials;

public record GetMaterialByIdQuery(Guid Id) : IRequest<MaterialDto?>;

public class GetMaterialByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetMaterialByIdQuery, MaterialDto?>
{
    private readonly IAppDbContext _context = context;

    public async Task<MaterialDto?> Handle(
        GetMaterialByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Materials.AsNoTracking()
            .Where(m => m.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
