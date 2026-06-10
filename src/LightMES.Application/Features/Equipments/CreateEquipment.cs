using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Equipments;

public record CreateEquipmentCommand(
    string EquipmentCode,
    string EquipmentName,
    string? Location,
    string? Description) : IRequest<Guid>;
public class CreateEquipmentCommandValidator : AbstractValidator<CreateEquipmentCommand>
{
    private readonly IAppDbContext _context;
    public CreateEquipmentCommandValidator(IAppDbContext context)
    {
        _context = context;
        RuleFor(v => v.EquipmentCode)
            .NotEmpty().WithMessage("设备编码不能为空.")
            .MaximumLength(50).WithMessage("设备编码最多50个字符");
        RuleFor(v => v.EquipmentName)
            .NotEmpty().WithMessage("设备名称不能为空.")
            .MaximumLength(100).WithMessage("设备名称最多100个字符.");
        When(x => !string.IsNullOrWhiteSpace(x.EquipmentCode), () =>
        {
            RuleFor(v => v.EquipmentCode)
            .MustAsync(BeUniqueCode)
            .WithMessage("设备编码已存在.");
        });
    }

    private async Task<bool> BeUniqueCode(string code, CancellationToken token)
    {
        return !await _context.Equipments
            .AnyAsync(x => x.EquipmentCode == code, token);
    }
}
public class CreateEquipmentCommandHandler : IRequestHandler<CreateEquipmentCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateEquipmentCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserService.Username ?? "System";

        var equipment = new Equipment(
            Guid.NewGuid(),
            request.EquipmentCode.Trim().ToUpper(),
            request.EquipmentName,
            request.Location,
            request.Description,
            currentUser
            );
        await _context.Equipments.AddAsync(equipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return equipment.Id;
    }
}
