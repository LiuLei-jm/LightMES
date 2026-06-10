using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LightMES.Domain.Common.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException() : base()
    {
    }
    public InvalidStatusTransitionException(string message) : base(message) { }
}
