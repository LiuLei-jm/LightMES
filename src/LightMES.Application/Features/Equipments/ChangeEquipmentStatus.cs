using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Constants;
using LightMES.Domain.Entities;
using LightMES.Domain.Enums;
using MediatR;

namespace LightMES.Application.Features.Equipments;

public record ChangeEquipmentStatusCommand(Guid Id, EquipmentStatus NewStatus) : IRequest<Unit>;

public class ChangeEquipmentStatusCommandHandler
    : IRequestHandler<ChangeEquipmentStatusCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ChangeEquipmentStatusCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUserService
    )
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(
        ChangeEquipmentStatusCommand request,
        CancellationToken cancellationToken
    )
    {
        var entity = await _context.Equipments.FindAsync(
            new object[] { request.Id },
            cancellationToken
        );
        if (entity == null)
            throw new KeyNotFoundException(nameof(Equipment));
        var currentUser = _currentUserService.Username ?? SystemConst.User.DefaultUser;
        entity.ChangeStatus(request.NewStatus, currentUser);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
