using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Rax.IocTools.Test.Decoration")]
namespace Rax.IocTools.Decoration;
internal static class ServiceDecorator
{
    /// <summary>
    /// Decorates a subject with added feature to add dependencies from services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="decoration"></param>
    /// <typeparam name="T"></typeparam>
    public static void Decorate<T>(IServiceCollection services, Func<T, IServiceProvider, T> decoration)
    {
        var subjectDescriptor = GetRequiredSubjectDescriptor<T>(services);
        services.Remove(subjectDescriptor);
        var newDescriptor = GetDecoratedServiceDescriptor(services, decoration, subjectDescriptor);
        services.Add(newDescriptor);
    }
    
    /// <summary>
    /// Decorates an already added service type (see decorator design pattern).
    /// </summary>
    /// <param name="services"></param>
    /// <param name="decoration">The function input parameter is the implementation to decorate.</param>
    /// <typeparam name="T">The abstraction to decorate</typeparam>
    public static void Decorate<T>(IServiceCollection services, Func<T, T> decoration)
    {
        var subjectDescriptor = GetRequiredSubjectDescriptor<T>(services);
        services.Remove(subjectDescriptor);

        T AdapterDecoration(T subject, IServiceProvider provider)
        {
            return decoration(subject);
        }
        
        var newDescriptor = GetDecoratedServiceDescriptor<T>(services, AdapterDecoration, subjectDescriptor);

        services.Add(newDescriptor);
    }

    private static ServiceDescriptor GetDecoratedServiceDescriptor<T>(
        IServiceCollection services, 
        Func<T, IServiceProvider, T> decoration,
        ServiceDescriptor descriptor)
    {
        ServiceDescriptor newDescriptor = null!;

        if (ObjectDescriptor(descriptor))
        {
            newDescriptor = ObjectDescriptorCreate(decoration,
                (T) descriptor.ImplementationInstance!, descriptor.Lifetime);
        }

        else if (FactoryDescriptor(descriptor))
        {
            newDescriptor = FactoryDecoratorDescriptorCreate(
                decoration, descriptor.ImplementationFactory!, descriptor.Lifetime);
        }

        else if (GenericsDescriptor(descriptor))
        {
            newDescriptor = GenericsDecoratorDescriptorCreate(decoration, descriptor.ImplementationType!, descriptor.Lifetime);
            RepointSubjectDescriptor(services, descriptor.ImplementationType!, descriptor.Lifetime);
        }

        return newDescriptor;
    }

    private static ServiceDescriptor GetRequiredSubjectDescriptor<T>(IServiceCollection services)
    {
        ServiceDescriptor descriptor;

        try
        {
            descriptor = services.Single(x => x.ServiceType == typeof(T));
        }
        catch (Exception e)
        {
            var count = services.Count(x => x.ServiceType == typeof(T));
            throw new ServiceDecorationException($"To be able to decorate exactly one descriptor is allowed with" +
                                                 $" the target service. Target: {typeof(T).Name} count: {count}", e);
        }

        if (ImplementationSameAsServiceType(descriptor))
        {
            throw new ServiceDecorationException(
                $"You can not decorate a descriptor with same service type as " +
                $"implementation. {nameof(ServiceDecorator)}.{nameof(Decorate)} needs abstracted Service Types");
        }

        return descriptor!;
    }
    
    private static ServiceDescriptor FactoryDecoratorDescriptorCreate<T>(
        Func<T, IServiceProvider, T> decoration, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime)
    {
        object Factory(IServiceProvider provider)
        {
            var subject = (T) implementationFactory(provider);
            return decoration(subject, provider)!;
        }

        return new ServiceDescriptor(typeof(T), Factory, lifetime);
    }
    
    private static ServiceDescriptor GenericsDecoratorDescriptorCreate<T>(
        Func<T, IServiceProvider, T> decoration, Type implementationType, ServiceLifetime lifetime)
    {
        object Factory(IServiceProvider provider)
        {
            var subject = GetRequiredImplementation<T>(provider, implementationType);
            return decoration(subject, provider)!;
        }

        return new ServiceDescriptor(typeof(T), Factory, lifetime);
    }

    private static ServiceDescriptor ObjectDescriptorCreate<T>(
        Func<T, IServiceProvider, T> decoration, 
        T instance,
        ServiceLifetime lifetime)
    {
        object Factory(IServiceProvider provider)
        {
            return decoration(instance, provider)!;
        }

        return new ServiceDescriptor(typeof(T), Factory, lifetime);
    }
    
    private static T GetRequiredImplementation<T>(IServiceProvider provider, Type implementationType)
    {
        T subject;
        try
        {
            subject = (T) provider.GetRequiredService(implementationType);
        }
        catch (Exception e)
        {
            throw new ServiceDecorationException(
                $"Failed to get required service subject to decorate. ServiceType: {typeof(T).Name}", e);
        }

        return subject;
    }

    private static bool ImplementationSameAsServiceType(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationType != null && descriptor.ServiceType == descriptor.ImplementationType;
    }

    private static void RepointSubjectDescriptor(
        IServiceCollection services, Type descriptorImplementationType, ServiceLifetime descriptorLifetime)
    {
        services.Add(new ServiceDescriptor(descriptorImplementationType, descriptorImplementationType,
            descriptorLifetime));
    }

    private static bool ObjectDescriptor(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationInstance != null;
    }

    private static bool FactoryDescriptor(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationFactory != null;
    }

    private static bool GenericsDescriptor(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationType != null;
    }
}