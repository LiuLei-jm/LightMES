using LightMES.Application.Common.Interfaces;
using LightMES.Application.Features.Users.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Users;

public record GetUsersQuery(bool? ActiveOnly) : IRequest<List<UserDto>>;
public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IAppDbContext _context;

    public GetUsersHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();
        if (request.ActiveOnly == true) query = query.Where(u => u.IsActive);
        return await query.Select(u => new UserDto(
            u.Id,
            u.Username,
            u.FullName,
            u.EmployeeNo,
            u.BadgeNo,
            u.UserRoles,
            u.IsActive)).ToListAsync(cancellationToken);
    }
}
