using System.ComponentModel;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rax.IocTools.Decoration;

namespace Rax.IocTools.Test.Decoration;

//For more theory see Decorator Design Pattern
public class ServiceDecoratorTests
{

    [Fact]
    public void Decorate_generic_descriptor()
    {
        //ARRANGE

        var services = new ServiceCollection();
        services.AddTransient<Message>();
        
        //Creates a "generic" descriptor
        services.AddTransient<BaseMessageProvider, HelloMessageProvider>();

        //ACT
        ServiceDecorator.Decorate<BaseMessageProvider>(services, subject => new MessageProviderDecorator(subject));
        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Hello");
    }

    [Fact]
    public void Decorate_single_generic_descriptor_should_throw()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<HelloMessageProvider>();

        //ACT & ASSERT
        FluentActions.Invoking(
            () => ServiceDecorator.Decorate<HelloMessageProvider>(services,
                subject => new HelloMessageProviderDecorator(subject)))
            .Should().Throw<ServiceDecorationException>();
    }
    
    [Fact]
    public void Decorate_abstract_subject_more_then_one_descriptor_per_service_type_should_throw()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddScoped<BaseMessageProvider, HelloMessageProvider>();
        services.AddScoped<BaseMessageProvider, MessageProviderDecorator>();

        //ACT & ASSERT
        FluentActions.Invoking(
                () => ServiceDecorator.Decorate<BaseMessageProvider>(services,
                    subject => new MessageProviderDecorator(subject)))
            .Should().Throw<ServiceDecorationException>();
    }

    [Fact]
    public void Decorate_non_abstract_subject_with_object_descriptor()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<Letter>();
        services.AddSingleton(new HelloMessageProvider());
        
        //ACT
        ServiceDecorator.Decorate<HelloMessageProvider>(services,(subject) =>
            new HelloMessageProviderDecorator(subject));

        var provider = services.BuildServiceProvider();
        
        //ASSERT
        var service = provider.GetRequiredService<Letter>();
        service.Content.Should().Be("HelloHello");
    }
    
    [Fact]
    public void Decorate_non_abstract_subject_with_factory_descriptor()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<Letter>();
        services.AddSingleton(provider => new HelloMessageProvider());
        
        //ACT
        ServiceDecorator.Decorate<HelloMessageProvider>(services,(subject) =>
            new HelloMessageProviderDecorator(subject));

        var provider = services.BuildServiceProvider();
        
        //ASSERT
        var service = provider.GetRequiredService<Letter>();
        service.Content.Should().Be("HelloHello");
    }

    [Fact]
    public void Decorate_factory_descriptor()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<Message>();
        services.AddSingleton<BaseMessageProvider>(
            provider => new HelloMessageProvider());
        
        //ACT
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject) =>
            new MessageProviderDecorator(subject));

        var provider = services.BuildServiceProvider();
        var service =  provider.GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Hello");
    }

    [Fact]
    public void Decorate_generic_descriptor_twice_with_new()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<Message>();
        services.AddSingleton<ExtraText>();
        services.AddSingleton<BaseMessageProvider, HelloMessageProvider>();

        //ACT
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject) =>
            new MessageProviderDecorator(
                new MessageProviderDecorator(subject)));

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Decorated: Hello");
    }

    [Fact]
    public void Decorate_generic_descriptor_with_provider()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddScoped<Message>();
        services.AddScoped<ExtraText>();
        services.AddScoped<BaseMessageProvider, HelloMessageProvider>();

        //ACT
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject, provider) =>
            new MessageProviderDecoratorExtraDependencies(subject, provider.GetRequiredService<ExtraText>()));

        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Extra injected: Hello");
    }
    
    [Fact]
    public void Decorate_object_descriptor_with_provider()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<Message>();
        services.AddSingleton<BaseMessageProvider>(new HelloMessageProvider());

        //ACT
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject, provider) =>
            new MessageProviderDecorator(subject));

        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Hello");
    }
    
    [Fact]
    public void Decorate_factory_descriptor_with_provider()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<Message>();
        services.AddSingleton<BaseMessageProvider>(provider => new HelloMessageProvider());

        //ACT
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject, provider) =>
            new MessageProviderDecorator(subject));

        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Hello");
    }
    
    [Fact]
    public void Decorate_provider_factory_descriptor_repeatedly()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<Message>();
        services.AddSingleton<BaseMessageProvider>(provider => new HelloMessageProvider());

        //ACT
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject, provider) =>
            new MessageProviderDecorator(subject));
        
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject, provider) =>
            new MessageProviderDecorator(subject));
        
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject, provider) =>
            new MessageProviderDecorator(subject));

        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Decorated: Decorated: Hello");
    }
    
    [Fact]
    public void Decorate_generic_descriptor_repeatedly()
    {
        //ARRANGE
        var services = new ServiceCollection();
        services.AddSingleton<Message>();
        services.AddSingleton<BaseMessageProvider,HelloMessageProvider>();

        //ACT
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject) =>
            new MessageProviderDecorator(subject));
        
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject) =>
            new MessageProviderDecorator(subject));
        
        ServiceDecorator.Decorate<BaseMessageProvider>(services,(subject) =>
            new MessageProviderDecorator(subject));

        var service = services.BuildServiceProvider().GetRequiredService<Message>();

        //ASSERT
        service.Content.Should().Be("Decorated: Decorated: Decorated: Hello");
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

internal class HelloMessageProvider : BaseMessageProvider
{
    public override string GetMessage()
    {
        return "Hello";
    }
}

internal class HelloMessageProviderDecorator : HelloMessageProvider
{
    private readonly HelloMessageProvider _helloMessageProvider;

    public HelloMessageProviderDecorator(HelloMessageProvider helloMessageProvider)
    {
        _helloMessageProvider = helloMessageProvider;
    }
    public override string GetMessage()
    {
        return "Hello" + _helloMessageProvider.GetMessage();
    }
}

internal class Letter
{
    public string Content { get; }

    public Letter(HelloMessageProvider messageProvider)
    {
        Content = messageProvider.GetMessage();
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