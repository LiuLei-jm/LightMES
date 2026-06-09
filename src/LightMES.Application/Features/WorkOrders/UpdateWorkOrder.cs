using FluentValidation;
using LightMES.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LightMES.Application.Features.WorkOrders;

public record UpdateWorkOrderCommand(Guid Id, int PlanQty, DateTime PlanStartTime, DateTime PlanEndTime) : IRequest<UpdateResult>;
public record UpdateResult(bool Success, string ErrorMessage = "");
public class UpdateWorkOrderValidator : AbstractValidator<UpdateWorkOrderCommand>
{
    public UpdateWorkOrderValidator()
    {
        RuleFor(x => x.PlanQty).GreaterThan(0).WithMessage("计划产量必须大于0");
        RuleFor(x => x.PlanStartTime).LessThan(x => x.PlanEndTime).WithMessage("计划开工时间必须早于完工时间");
    }
}
public class UpdateWorkOrderHandler : IRequestHandler<UpdateWorkOrderCommand, UpdateResult>
{
    private readonly IAppDbContext _context;

    public UpdateWorkOrderHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateResult> Handle(UpdateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var wo = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wo == null)
        {
            return new UpdateResult(false, "工单不存在");
        }
        try
        {
            wo.UpdateDetails(request.PlanQty, request.PlanStartTime, request.PlanEndTime);
            await _context.SaveChangesAsync(cancellationToken);
            return new UpdateResult(true);
        }
        catch (InvalidOperationException ex)
        {
            return new UpdateResult(false, ex.Message);
        }
    }
}
