using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Bobbysoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public class ServiceDecorationException : Exception
{
    public ServiceDecorationException()
    {
    }

    public ServiceDecorationException(string message) : base(message)
    {
    }

    public ServiceDecorationException(string message, Exception inner) : base(message, inner)
    {
    }

    public ServiceDecorationException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}
