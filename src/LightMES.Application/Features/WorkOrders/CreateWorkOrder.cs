using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LightMES.Application.Features.WorkOrders;

public record CreateWorkOrderCommand(
    string OrderNo,
    string ProductCode,
    string ProductName,
    int PlanQty,
    Guid RouteId,
    Guid CreatedByUserId,
    DateTime PlanStartTime,
    DateTime PlanEndTime) : IRequest<Result<Guid>>;

public record Result<T>(bool IsSuccess, T? Value, string? Error = null)
{
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(x => x.OrderNo).NotEmpty().MaximumLength(50).WithMessage("工单号不能为空且长度不能超过50");
        RuleFor(x => x.ProductCode).NotEmpty().WithMessage("产品编码不能为空");
        RuleFor(x => x.ProductName).NotEmpty().WithMessage("产品名称不能为空");
        RuleFor(x => x.PlanQty).GreaterThan(0).WithMessage("计划生产数量必须大于 0");
        RuleFor(x => x.RouteId).NotEmpty().WithMessage("必须指定工艺路线");
        RuleFor(x => x.CreatedByUserId).NotEmpty().WithMessage("创建人不能为空");
        RuleFor(x => x.PlanStartTime).LessThan(x => x.PlanEndTime).WithMessage("计划开始时间必须早于结束时间");
    }
}

public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<CreateWorkOrderCommandHandler> _logger;

    public CreateWorkOrderCommandHandler(IAppDbContext context, ILogger<CreateWorkOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("开始处理创建工单业务，工单号: {OrderNo}, 产品： {ProductName}", request.OrderNo, request.ProductName);
        var routeExists = await _context.Routes.AnyAsync(r => r.Id == request.RouteId, cancellationToken);
        if (!routeExists)
        {
            _logger.LogWarning("创建工单失败：工艺路线 {RouteId} 不存在", request.RouteId);
            return Result<Guid>.Failure("指定的工艺路线不存在");
        }
        var creatorExists = await _context.Users.AnyAsync(u => u.Id == request.CreatedByUserId, cancellationToken);
        if (!creatorExists)
        {
            _logger.LogWarning("创建工单失败：创建人用户 {UserId} 不存在", request.CreatedByUserId);
            return Result<Guid>.Failure("指定的创建人不存在");
        }
        if (await _context.WorkOrders.AnyAsync(w => w.OrderNo == request.OrderNo, cancellationToken))
        {
            _logger.LogWarning("创建工单失败：工单号 {OrderNo} 已存在", request.OrderNo);
            return Result<Guid>.Failure("工单号已存在");
        }
        var workOrder = new WorkOrder(
            Guid.NewGuid(),
            request.OrderNo,
            request.ProductCode,
            request.ProductName,
            request.PlanQty,
            request.RouteId,
            request.CreatedByUserId,
            request.PlanStartTime,
            request.PlanEndTime);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("工单 {OrderNo} 创建成功，分配 ID：{WorkOrderId}", workOrder.OrderNo, workOrder.Id);
        return Result<Guid>.Success(workOrder.Id);
    }
}
