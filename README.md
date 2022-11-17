# Rax.IocTools

## ServiceDecorator

This library class is used to decorate already dependency injected classes inside the Microsoft.DependencyInjection IOC.

### Example
```csharp
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
```

For more info:

[GOF Decorator Design pattern](https://refactoring.guru/design-patterns/decorator)
[Excellent explanation by Shiv Kumar youtube](https://www.youtube.com/watch?v=auaEZS-bAQQ)
