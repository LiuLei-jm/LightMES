using FluentValidation;
using LightMES.Application.Common.Interfaces;
using MediatR;

namespace LightMES.Application.Features.Equipments;

public record UpdateEquipmentCommand(
    Guid Id,
    string EquipmentName,
    string? Location,
    string? Description) : IRequest<Unit>;
public class UpdateEquipmentCommandValidator : AbstractValidator<UpdateEquipmentCommand>
{
    public UpdateEquipmentCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty().WithMessage("设备ID不能为空.");
        RuleFor(v => v.EquipmentName)
            .NotEmpty().WithMessage("设备名称不能为空.")
            .MaximumLength(100).WithMessage("设备名称最多100个字符.");
    }
}
public class UpdateEquipmentCommandHandler : IRequestHandler<UpdateEquipmentCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateEquipmentCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Equipments.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) throw new KeyNotFoundException(nameof(Domain.Entities.Equipment));
        var currentUser = _currentUserService.Username ?? "System";
        entity.UpdateInfo(
            request.EquipmentName,
            request.Location,
            request.Description,
            currentUser);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}