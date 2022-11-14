using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rax.IocTools.Decoration;

namespace Rax.IocTools.Test.Decoration;

//For more theory see Decorator Design Pattern
public class ServiceDecoratorTests
{

    [Fact]
    public void Decorate_abstract_class_with_handler()
    {
        //ARRANGE

        var services = new ServiceCollection();
        var decorator = ArrangeHelloMessageWithBase(services);

        //ACT
        decorator.Decorate<BaseMessageProvider>(subject => new MessageProviderDecorator(subject));
        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Hello");
    }

    [Fact]
    public void Decorate_with_string_extra_dependencies()
    {
        //ARRANGE
        var services = new ServiceCollection();
        var decorator = ArrangeHelloMessageWithBase(services);

        //ACT
        decorator.Decorate<BaseMessageProvider>(subject =>
            new MessageProviderDecoratorExtraDependencies(subject, "Extra: "));

        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Extra: Hello");
    }

    [Fact]
    public void Decorate_with_injected_extra_dependencies()
    {
        //ARRANGE
        var services = new ServiceCollection();
        var decorator = ArrangeServicesWithExtraText(services);

        //ACT
        decorator.Decorate<BaseMessageProvider>((subject, provider) =>
            new MessageProviderDecoratorExtraDependencies(subject, provider.GetRequiredService<ExtraText>()));

        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Extra injected: Hello");
    }

    [Fact]
    public void Decorate_with_double_decorators_by_new()
    {
        //ARRANGE
        var services = new ServiceCollection();
        var decorator = ArrangeServicesWithExtraText(services);

        //ACT
        decorator.Decorate<BaseMessageProvider>((subject) =>
            new MessageProviderDecorator(
            new MessageProviderDecorator(subject)));

        var provider = services.BuildServiceProvider();
        var service =  provider.GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Decorated: Hello");
    }

    private ServiceDecorator ArrangeServicesWithExtraText(IServiceCollection services)
    {
        services.AddSingleton<Message>();
        services.AddSingleton<ExtraText>();
        services.AddSingleton<BaseMessageProvider, HelloMessageProvider>();
        return new ServiceDecorator(services);
    }

    private static ServiceDecorator ArrangeHelloMessageWithBase(IServiceCollection services)
    {
        services.AddSingleton<Message>();
        services.AddSingleton<BaseMessageProvider, HelloMessageProvider>();
        var decorator = new ServiceDecorator(services);
        return decorator;
    }
}

internal abstract class BaseMessageProvider
{
    public abstract string GetMessage();
}

internal interface IMessageProvider
{
    public string GetMessage();
}

internal class ExtraText
{
    public string Value { get; } = "Extra injected: ";
}

internal class MessageProviderDecoratorExtraDependencies : BaseMessageProvider
{
    private readonly BaseMessageProvider _baseMessageProvider;
    private readonly string _extra;

    public MessageProviderDecoratorExtraDependencies(BaseMessageProvider baseMessageProvider, string extra)
    {
        _baseMessageProvider = baseMessageProvider;
        _extra = extra;
    }

    public MessageProviderDecoratorExtraDependencies(BaseMessageProvider baseMessageProvider, ExtraText text)
    {
        _baseMessageProvider = baseMessageProvider;
        _extra = text.Value;
    }

    public override string GetMessage()
    {
        return _extra + _baseMessageProvider.GetMessage();
    }
}

internal class MessageProviderDecorator : BaseMessageProvider
{
    private readonly BaseMessageProvider _provider;

    public MessageProviderDecorator(BaseMessageProvider provider)
    {
        _provider = provider;
    }

    public override string GetMessage()
    {
        return "Decorated: " + _provider.GetMessage();
    }
}

internal class InjectionMessageProviderDecorator : BaseMessageProvider
{
    private readonly BaseMessageProvider _provider;

    public InjectionMessageProviderDecorator(BaseMessageProvider provider)
    {
        _provider = provider;
    }

    public override string GetMessage()
    {
        return "Injection " + _provider.GetMessage();
    }
}

internal class HelloMessageProvider : BaseMessageProvider
{
    public override string GetMessage()
    {
        return "Hello";
    }
}

internal class Message
{
    public string Content { get; }

    public Message()
    {

    }

    public Message(string content)
    {
        Content = content;
    }

    public Message(BaseMessageProvider messageProvider)
    {
        Content = messageProvider.GetMessage();
    }

    public Message(IMessageProvider messageProvider)
    {
        Content = messageProvider.GetMessage();
    }
}