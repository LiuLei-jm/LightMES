using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Constants;
using LightMES.Domain.Entities;
using MediatR;

namespace LightMES.Application.Features.Equipments;

public record DeactivateEquipmentCommand(Guid Id) : IRequest<Unit>;

public class DeactivateEquipmentCommandHandler : IRequestHandler<DeactivateEquipmentCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateEquipmentCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUserService
    )
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(
        DeactivateEquipmentCommand request,
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
        entity.Deactivate(currentUser);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
