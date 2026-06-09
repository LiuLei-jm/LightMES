using LightMES.Application.Common.Exceptions;
using LightMES.Application.Common.Interfaces;
using LightMES.Application.Common.Security;
using MediatR;

namespace LightMES.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;

    public AuthorizationBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is ISecuredRequest securedRequest)
        {
            if (!_currentUserService.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("用户未登录");
            }
            var requiredPermission = securedRequest.RequiredPermission;
            var hasPermission = await _currentUserService.HasPermissionAsync(requiredPermission, cancellationToken);
            if (!hasPermission)
            {
                throw new ForbiddenAccessException($"无权执行该操作。需要权限：{requiredPermission}");
            }
        }
        return await next();
    }
}
