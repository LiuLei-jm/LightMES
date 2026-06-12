using FluentValidation;
using LightMES.Application.Common.Interfaces;
using LightMES.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LightMES.Application.Features.WorkOrders;

public record SyncErpWorkOrderCommand(
    string ErpOrderNo,
    string ProductCode,
    string ProductName,
    int Qty,
    Guid RouteId
) : IRequest<bool>;

public class SyncErpWorkOrderValidator : AbstractValidator<SyncErpWorkOrderCommand>
{
    public SyncErpWorkOrderValidator()
    {
        RuleFor(x => x.ErpOrderNo).NotEmpty();
        RuleFor(x => x.ProductCode).NotEmpty();
        RuleFor(x => x.Qty).GreaterThan(0);
    }
}

public class SyncErpWorkOrderHandler : IRequestHandler<SyncErpWorkOrderCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<SyncErpWorkOrderHandler> _logger;

    public SyncErpWorkOrderHandler(IAppDbContext context, ILogger<SyncErpWorkOrderHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(
        SyncErpWorkOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "==================[ERP同步]收到ERP工单同步请求： {ErpNo} ===============",
            request.ErpOrderNo
        );
        if (
            await _context.WorkOrders.AnyAsync(
                w => w.OrderNo == request.ErpOrderNo,
                cancellationToken
            )
        )
        {
            _logger.LogInformation(
                "[ERP同步] 工单 {ErpNo} 在MES中已存在，忽略本次同步。",
                request.ErpOrderNo
            );
            return true;
        }
        var route = await _context
            .Routes.Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);
        if (route == null)
        {
            _logger.LogError("[ERP同步]失败！MES中不存在对应工艺路线：{RouteId}", request.RouteId);
            return false;
        }

        var systemUser = await _context.Users.FirstOrDefaultAsync(
            u => u.Username == "ERP_INTEGRATOR",
            cancellationToken
        );
        var creatorId = systemUser?.Id ?? Guid.NewGuid();
        var workOrder = new WorkOrder(
            Guid.NewGuid(),
            request.ErpOrderNo,
            request.ProductCode,
            request.ProductCode,
            request.Qty,
            request.RouteId,
            creatorId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3)
        );
        workOrder.InitProgress(route.Steps);
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "[ERP同步]成功！ERP工单 {ErpNo} 已转为MES草稿工单，工艺工序数：{StepCount}",
            workOrder.OrderNo,
            route.Steps.Count
        );
        return true;
    }
}

