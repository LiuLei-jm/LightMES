using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Constants;
using LightMES.Domain.Enums;
using MediatR;

namespace LightMES.Application.Features.Materials;

public record UpdateMaterialCommand(
    Guid Id,
    string MaterialName,
    string? Specification,
    string Unit,
    MaterialType MaterialType
) : IRequest<Unit>;

public class UpdateMaterialCommandValidator : AbstractValidator<UpdateMaterialCommand>
{
    public UpdateMaterialCommandValidator()
    {
        RuleFor(x => x.MaterialName)
            .NotEmpty()
            .WithMessage("物料名称不能为空")
            .MaximumLength(100)
            .WithMessage("物料名称不能超过100个字符");
        RuleFor(x => x.Unit)
            .NotEmpty()
            .WithMessage("计量单位不能为空")
            .MaximumLength(10)
            .WithMessage("计量单位不能超过10个字符");
        RuleFor(x => x.MaterialType).IsInEnum().WithMessage("无效的物料类型");
    }
}

public class UpdateMaterialCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUserService
) : IRequestHandler<UpdateMaterialCommand, Unit>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Unit> Handle(
        UpdateMaterialCommand request,
        CancellationToken cancellationToken
    )
    {
        var material =
            await _context.Materials.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"未找到 ID 未 {request.Id} 的物料.");
        var currentUser = _currentUserService.Username ?? SystemConst.User.DefaultUser;
        material.Activate(currentUser);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
