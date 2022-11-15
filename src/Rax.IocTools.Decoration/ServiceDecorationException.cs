namespace Rax.IocTools.Decoration;

public class ServiceDecorationException : Exception
{
    public ServiceDecorationException() { }
    public ServiceDecorationException(string message) : base(message) { }
    public ServiceDecorationException(string message, Exception inner) : base(message, inner) { }
    public ServiceDecorationException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}