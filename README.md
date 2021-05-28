![Build status](https://github.com/lordldx/fluentservicecollection/actions/workflows/ci.yml/badge.svg)
![Nuget](https://img.shields.io/nuget/dt/fluentservicecollection?color=lightgreen&label=nuget%20downloads)
![GitHub issues](https://img.shields.io/github/issues/lordldx/fluentservicecollection?label=open%20issues)

# fluentservicecollection
fluentservicecollection allows to add multiple services with a mutual ancestor to the IServiceCollection at once via a fluent syntax.


# Examples

Installing all classes that implement `IStore` scoped
```C#
services.AddImplementationsOfType<IStore>()
                .WithServiceFirstInterface()
                .Scoped();
```

Installing all classes that implement `IFactory` as singleton, except the `TranslationFactory`
```C#
services.AddImplementationsOfType(typeof(IFactory))
                .Except<TranslationFactory>()
                .WithServiceFirstInterface()
                .AsSingleton();
```

# Reference

Using fluentservicecollection occurs in four steps:
1. Select the basetype of which you want to install the implementations.
2. (Optional) Do some filtering on the list of selected types
3. Determine how you want to install each type
4. Determine the lifetype with which to install the selected types

## Step 1: Select your types
We start by selecting a basetype. 
fluentservicecollection will then scan **all assemblies in the current AppDomain** for any type which implements the selected type.

```C#
services.AddImplementationsOfType<IBaseType>() // regular type

services.AddImplementationsOfType<IBaseType<SomeOtherType>> // generic type

services.AddImplementationsOfType(typeof(IBaseType)) // using the non-generic interface to select implementations of a regular type
```

## Step 2: Optionally filter out some types
If you want to exclude a couple of types from the list, for example because you don't want to install it, or because you want to install it with different lifetime, then you can do this via the `Except` function.

For example, here we'll select all types which implement `IStore`, except the `UserStore`, because we want to install that one as a singleton.

```C#
services.AddImplementationsOfType<IStore>()
        .Except<UserStore>()
```

## Step 3: Determine how you want to install the type.
Now, if you want the installed types to be injected properly, we don't all want to install them with their top-level interface of course.
If we would for example install two stores, e.g. `ProductStore` and `OrderStore` both as `IStore`, then dotnet would not know which store to inject if we would have an `IStore` dependency somewhere in a constructor

```C#
services.AddImplementationsOfType<IStore>()
        .Scoped();

public class ArticleService
{
    public ArticleService(IStore articleStore) // will generate an exception upon creation.
    {
    }
}
```

Therefor, we need to specify how we want to install each selected type in the IoC container. Currently, fluentservicecollection supports five different strategies to install the selected types:

### Default interface
When installing the selected classes as _Default Interface_ the classes get installed into the container with a service equal to their name prepended with a capital letter I.

When no such an interface exists, then a runtime exception will be thrown.

```C#
public interface IStore {}

public interface IProductStore : IStore
{
    Product GetProductById(string id);
}

public interface IOrderStore : IStore
{
    void CreateOrder();
}

public class ProductStore : IProductStore
{
    public Product GetProductById(string id)
    {
        // product fetching logic
    }
}

public class OrderStore : IOrderStore
{
    public void CreateOrder()
    {
        // order creation logic
    }
}

public interface IService {}

public interface ICheckoutService : IService
{
    void CheckOut();
}

public class MyCheckoutService : ICheckoutService 
{
    public void Checkout()
    {
        // checkout logic
    }
}

// in Startup.cs Configure
services.AddImplementationsOfType<IStore>()
        .WithServiceDefaultInterface()
        .Scoped(); // Will install ProductStore as IProductStore and OrderStore as IOrderStore

services.AddImplementationsOfType<IService>()
        .WithServiceDefaultInterface()
        .Scoped(); // Will generate an exception, because there is no interface named IMyCheckoutService
```

### First interface
When installing the selected classes as _First Interface_ the classes get installed into the container with a service equal to the first interface to appear in the list of interfaces from which they derive

```C#
public interface IService {}

public interface ICheckoutService
{
    void CheckOut();
}

public interface IPaymentService
{
    void Pay();
}

public class CheckoutAndPaymentService : ICheckoutService, IService, IPaymentService
{
    public void Checkout()
    {
        // checkout logic
    }

    public void Pay()
    {
        // payment logic
    }
}

// in Startup.cs Configure
services.AddImplementationsOfType<IService>()
        .WithServiceFirstInterface()
        .Scoped(); // CheckoutAndPaymentService will be installed as ICheckoutService
```

### Last interface
When installing the selected classes as _First Interface_ the classes get installed into the container with a service equal to the last interface to appear in the list of interfaces from which they derive

```C#
public interface IService {}

public interface ICheckoutService
{
    void CheckOut();
}

public interface IPaymentService
{
    void Pay();
}

public class CheckoutAndPaymentService : ICheckoutService, IService, IPaymentService
{
    public void Checkout()
    {
        // checkout logic
    }

    public void Pay()
    {
        // payment logic
    }
}

// in Startup.cs Configure
services.AddImplementationsOfType<IService>()
        .WithServiceLastInterface()
        .Scoped(); // CheckoutAndPaymentService will be installed as IPaymentService
```    

### Specific interface
When specifying a specific service, all selected classes will be installed as the same service.
This can come in handy if you want to support different implementations being supported, for example with a _Processor_ pattern.

```C#
public class IMessageProcessor
{
    bool CanProcess(Message msg);

    void Process(Message msg);
}

public class CreateOrderMessageProcessor : IMessageProcessor
{
    public bool CanProcess(Message msg)
    {
        return msg.Type = MessageTypes.CreateOrder;
    }

    public void Process(Message msg)
    {
        // Create order message processing logic
    }
}

public class UpdateOrderMessageProcessor : IMessageProcessor
{
    public bool CanProcess(Message msg)
    {
        return msg.Type = MessageTypes.UpdateOrder;
    }

    public void Process(Message msg)
    {
        // UPdate order message processing logic
    }
}

public class CancelOrderMessageProcessor : IMessageProcessor
{
    public bool CanProcess(Message msg)
    {
        return msg.Type = MessageTypes.CancelOrder;
    }

    public void Process(Message msg)
    {
        // Cancel order message processing logic
    }
}

public class MessageProcessor
{
    private readonly IEnumerable<IMessageProcessor> _processors;

    public MessageProcessor(IEnumerable<IMessageProcessor> processors)
    {
        _processors = processors;
    }

    public void ProcessMessage(Message msg)
    {
        var processor = _processors.FirstOrDefault(x => x.CanProcess(msg));
        if (processor != default)
            processor.Process(msg);
    }
}

// in Startup.cs Configure
services.AddImplementationsOfType<IMessageProcesor>()
        .WtithService<IMessageProcessor>()
        .Scoped();
```

### Self
Finally, you can choose not to install the selected classes with as a specific interface, but rather just as itself as service.

```C#
public interface IStore {}

public interface IProductStore : IStore
{
    Product GetProductById(string id);
}

public interface IOrderStore : IStore
{
    void CreateOrder();
}

public class ProductStore : IProductStore
{
    public Product GetProductById(string id)
    {
        // product fetching logic
    }
}

public class OrderStore : IOrderStore
{
    public void CreateOrder()
    {
        // order creation logic
    }
}

// in Startup.cs Configure
services.AddImplementationsOfType<IStore>()
        .AsSelf()
        .Scoped(); // Will install ProductStore as ProductStore and OrderStore as OrderStore
```

The above example is essentially the same as
```C#
services.AddScoped<ProductStore>();
services.AddScoped<OrderStore>();
```

## Step 4: Determine Lifetime
Finally, we need to instruct fluentservicecollection with which lifetime the selected types must be installed.
Fluentservicecollection currently supports two lifetimes:

### Singleton
If you use this lifetime, then each selected class will be installed in the container as a singleton.
 
Essentially, fluentservicecollection will execute `services.AddSingleton<IService, Implemetation>()` for each selected type.

An example, again with our two stores:
```C#
services.AddImplementationsOfType<IStore>()
        .WithServiceDefaultInterface()
        .AsSingleton();
```
is essentially the same as
```C#
services.AddSingleton<IProductStore, ProductStore>();
services.AddSingleton<IOrderStore, OrderStore>();
```

### Scoped
if you use this lifetime, then each selected class will be installed in the container with a scoped lifetime.

Essentially, fluentservicecollection will execute `services.AddScoped<IService, Implemetation>()` for each selected type.

An example, again with our two stores:
```C#
services.AddImplementationsOfType<IStore>()
        .WithServiceDefaultInterface()
        .Scoped();
```
is essentially the same as
```C#
services.AddScoped<IProductStore, ProductStore>();
services.AddScoped<IOrderStore, OrderStore>();
```