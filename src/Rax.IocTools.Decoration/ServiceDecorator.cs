using Microsoft.Extensions.DependencyInjection;

namespace Rax.IocTools.Decoration;

public sealed class ServiceDecorator
{
    private readonly IServiceCollection _services;
    public ServiceDecorator(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Decorates a subject with added feature to add dependencies from services.
    /// </summary>
    /// <param name="decoration"></param>
    /// <typeparam name="T"></typeparam>
    public void Decorate<T>(Func<T, IServiceProvider, T> decoration)
    {
        var descriptor = _services.FirstOrDefault(x => x.ServiceType == typeof(T)) ??
                         _services.FirstOrDefault(x => x.ImplementationType == typeof(T));

        ArgumentNullException.ThrowIfNull(descriptor, $"Type: {typeof(T).Name} not found in services");

        var descriptorBuilder = new DescriptorFactory(descriptor);
        _services.Remove(descriptor);

        var implementationDescriptor = descriptorBuilder.CreateUnAbstractedDescriptor();
        _services.Add(implementationDescriptor);

        object Factory(IServiceProvider provider)
        {
            var subject = provider.GetRequiredService(implementationDescriptor!.ServiceType);
            return decoration((T)subject, provider)!;
        }

        descriptorBuilder.SetFactory(Factory);

        var decoratedDescriptor = descriptorBuilder.Create();
        _services.Add(decoratedDescriptor);
    }

    /// <summary>
    /// Decorates an already added service type (see decorator design pattern).
    /// </summary>
    /// <param name="decoration">The function input parameter is the implementation to decorate.</param>
    /// <typeparam name="T">The abstraction to decorate</typeparam>
    public void Decorate<T>(Func<T, T> decoration)
    {
        T Factory(T subject, IServiceProvider provider)
        {
            return decoration(subject);
        }

        Decorate<T>(Factory);
    }
}