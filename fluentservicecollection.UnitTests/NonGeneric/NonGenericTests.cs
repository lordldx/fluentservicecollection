using FluentAssertions;
using fluentservicecollection.UnitTests.NonGeneric.TestClasses;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace fluentservicecollection.UnitTests.NonGeneric
{
    public class NonGenericTests
    {
        [Fact]
        public void CanAdd_WithServiceFirstInterface()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddImplementationsOfType<IStore>()
                .WithServiceFirstInterface()
                .Scoped();

            // assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IProductStore>().Should().BeOfType<ProductStore>();
            provider.GetService<IOrderStore>().Should().BeOfType<OrderStore>();
            provider.GetService<ICheckoutStore>().Should().BeOfType<CombinedStore>();
        }

        [Fact]
        public void CanAdd_WithServiceDefaultInterface()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddImplementationsOfType<IStore>()
                .WithServiceDefaultInterface()
                .Scoped();

            // assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IProductStore>().Should().BeOfType<ProductStore>();
            provider.GetService<IOrderStore>().Should().BeOfType<OrderStore>();
            provider.GetService<ICheckoutStore>().Should().BeNull("the default interface of CombinedStore is ICombinedStore");
        }

        [Fact]
        public void CanAdd_WithServiceLastInterface()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddImplementationsOfType<IStore>()
                .WithServiceLastInterface()
                .Scoped();

            // assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IProductStore>().Should().BeOfType<CombinedStore>("GetInterfaces() works depth first, so ordered interfaces of CombinedStore is ICheckoutStore, IStore, IOrderStore, IProductStore");
            provider.GetService<IOrderStore>().Should().BeNull("GetInterfaces() works depth first, so last interface of OrderStore is IStore");
            provider.GetService<ICheckoutStore>().Should().BeNull("GetInterfaces() works depth first, so CombinedStore gets installed as IProductStore");
        }

        [Fact]
        public void CanAdd_WithService()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddImplementationsOfType<IStore>()
                .WithService<ICheckoutStore>()
                .Scoped();

            // assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IProductStore>().Should().BeNull("Nothing was installed as IProductStore");
            provider.GetService<IOrderStore>().Should().BeNull("nothing was installed as IOrderStore");
            provider.GetService<ICheckoutStore>().Should().BeOfType<CombinedStore>();
        }

        [Fact]
        public void CanAdd_CanExclude()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddImplementationsOfType<IStore>()
                .WithService<IProductStore>()
                .Except<ProductStore>()
                .Scoped();

            // assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IProductStore>().Should().BeOfType<CombinedStore>();
        }

        [Fact]
        public void CanAdd_AsSelf()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddImplementationsOfType<IStore>()
                .AsSelf()
                .Scoped();

            // assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IProductStore>().Should().BeNull();
            provider.GetService<ProductStore>().Should().BeOfType<ProductStore>();
            provider.GetService<IOrderStore>().Should().BeNull();
            provider.GetService<OrderStore>().Should().BeOfType<OrderStore>();
            provider.GetService<ICheckoutStore>().Should().BeNull();
            provider.GetService<CombinedStore>().Should().BeOfType<CombinedStore>();
        }
    }
}
