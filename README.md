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

### First interface

### Last interface

### Specific interface

### Self

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