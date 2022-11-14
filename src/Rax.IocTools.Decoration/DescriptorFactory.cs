using Microsoft.Extensions.DependencyInjection;

namespace Rax.IocTools.Decoration;

internal sealed class DescriptorFactory
{
    private Type ServiceType { get; }
    private Type? ImplementationType { get; set; }
    public ServiceLifetime Lifetime { get; }
    private Func<IServiceProvider, object>? Factory { get; set; }
    public DescriptorFactory(ServiceDescriptor descriptor)
    {
        ServiceType = descriptor.ServiceType;
        ImplementationType = descriptor.ImplementationType;
        Factory = descriptor.ImplementationFactory;
        Lifetime = descriptor.Lifetime;
    }

    public ServiceDescriptor Create()
    {
        if (IsFactoryDescriptor())
            return new ServiceDescriptor(ServiceType, Factory!, Lifetime);

        if (IsSimpleDescriptor())
            return new ServiceDescriptor(ServiceType, ImplementationType!, Lifetime);

        throw new ArgumentException("Uncovered condition");
    }

    public ServiceDescriptor CreateUnAbstractedDescriptor()
    {
        ArgumentNullException.ThrowIfNull(ImplementationType);

        return new ServiceDescriptor(ImplementationType, ImplementationType, Lifetime);
    }

    public void SetFactory(Func<IServiceProvider, object> factory)
    {
        if (Factory != null) throw new ArgumentException("Double wrapping of factories is not implemented");

        ImplementationType = null;
        Factory = factory;
    }

    private bool IsFactoryDescriptor()
    {
        return Factory != null && ImplementationType == null;
    }

    private bool IsSimpleDescriptor()
    {
        return ImplementationType != null && Factory == null;
    }
}