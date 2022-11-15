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

        ServiceDescriptor? newDescriptor = null;

        if (IsObjectDescriptor(descriptor))
        {
            object Factory(IServiceProvider provider)
            {
                return decoration((T)descriptor!.ImplementationInstance!, provider)!;
            }

            newDescriptor = new ServiceDescriptor(typeof(T), Factory, descriptor.Lifetime);
        }

        if (IsFactoryDescriptor(descriptor))
        {

            object Factory(IServiceProvider provider)
            {
                return decoration((T)descriptor!.ImplementationFactory!(provider), provider)!;
            }

            newDescriptor = new ServiceDescriptor(typeof(T), Factory, descriptor.Lifetime);
        }

        if (IsGenericsDescriptor(descriptor))
        {

            var subjectDescriptor = new ServiceDescriptor(descriptor.ImplementationType!, descriptor.ImplementationType!);
            _services.Add(subjectDescriptor);

            object Factory(IServiceProvider provider)
            {
                return decoration(provider!.GetRequiredService<T>(), provider)!;
            }

            newDescriptor = new ServiceDescriptor(typeof(T), Factory, descriptor.Lifetime);
        }

        _services.Remove(descriptor);
        _services.Add(newDescriptor!);

    }

    private bool IsObjectDescriptor(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationInstance != null;
    }

    private bool IsFactoryDescriptor(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationFactory != null;
    }

    private bool IsGenericsDescriptor(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationType != null;
    }
}