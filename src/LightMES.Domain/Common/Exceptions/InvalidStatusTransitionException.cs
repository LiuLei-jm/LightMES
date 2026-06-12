namespace LightMES.Domain.Common.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException() : base()
    {
    }
    public InvalidStatusTransitionException(string message) : base(message) { }
}
