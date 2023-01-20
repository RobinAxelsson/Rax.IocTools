using Microsoft.Extensions.DependencyInjection;

namespace Bobbysoft.Extensions.DependencyInjection;

public static class MicrosoftServiceCollectionDecoratorExtensions
{
    /// <sutmmary>
    ///     Decorates an injected dependency in Microsoft.DependencyInjection IServiceCollection. See decorator design pattern.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementationFactory"></param>
    /// <typeparam name="T"></typeparam>
    public static void Decorate<T>(this IServiceCollection services, Func<T, IServiceProvider, T> implementationFactory)
    {
        ServiceDecorator.Decorate<T>(services, implementationFactory);
    }

    /// <summary>
    ///     Decorates an injected dependency in Microsoft.DependencyInjection IServiceCollection. See decorator design pattern.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementationFactory"></param>
    /// <typeparam name="T">The abstraction to decorate</typeparam>
    public static void Decorate<T>(this IServiceCollection services, Func<T, T> implementationFactory)
    {
        ServiceDecorator.Decorate<T>(services, implementationFactory);
    }
}
