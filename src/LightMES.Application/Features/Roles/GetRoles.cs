using LightMES.Application.Common.Interfaces;
using LightMES.Application.Features.Roles.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Roles;

public record GetRolesQuery : IRequest<List<RoleDto>>;

public class GetRolesHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IAppDbContext _context;

    public GetRolesHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Roles.AsNoTracking();
        return await query.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description)).ToListAsync(cancellationToken);
    }
}