using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Constants;
using LightMES.Domain.Entities;
using LightMES.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.Materials;

public record CreateMaterialCommand(
    string MaterialCode,
    string MaterialName,
    string? Specification,
    string Unit,
    MaterialType MaterialType
) : IRequest<Guid>;

public class CreateMaterialCommandValidator : AbstractValidator<CreateMaterialCommand>
{
    public CreateMaterialCommandValidator()
    {
        RuleFor(x => x.MaterialCode)
            .NotEmpty()
            .WithMessage("物料编码不能为空")
            .MaximumLength(50)
            .WithMessage("物料编码不能超过50个字符");
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

public class CreateMaterialCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateMaterialCommand, Guid>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Guid> Handle(
        CreateMaterialCommand request,
        CancellationToken cancellationToken
    )
    {
        var codeExists = await _context.Materials.AnyAsync(
            m => m.MaterialCode == request.MaterialCode.ToUpper(),
            cancellationToken
        );
        if (codeExists)
            throw new InvalidOperationException($"物料编码 {request.MaterialCode} 已存在.");
        var currentUser = _currentUserService.Username ?? SystemConst.User.DefaultUser;
        var material = new Material(
            Guid.NewGuid(),
            request.MaterialCode,
            request.MaterialName,
            request.Specification,
            request.Unit,
            request.MaterialType,
            currentUser
        );
        await _context.Materials.AddAsync(material, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return material.Id;
    }
}
