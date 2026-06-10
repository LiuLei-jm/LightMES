using LightMES.Application.Common.Interfaces;
using LightMES.Application.Features.Users.Dtos;
using MediatR;

namespace LightMES.Application.Features.Users;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IAppDbContext _context;

    public GetUserByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.Id }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("用户未找到");
        return new UserDto(
            user.Id,
            user.Username,
            user.FullName,
            user.EmployeeNo,
            user.BadgeNo,
            user.UserRoles,
            user.IsActive);
    }
}