namespace LightMES.Application.Common.Security;

public interface ISecuredRequest
{
    string RequiredPermission { get; }
}
