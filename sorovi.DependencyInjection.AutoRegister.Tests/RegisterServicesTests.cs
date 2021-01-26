using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using sorovi.DependencyInjection.AutoRegister.Exceptions;
using sorovi.DependencyInjection.AutoRegister.TestAssembly;

namespace sorovi.DependencyInjection.AutoRegister.Tests
{
    public class RegisterServicesTests
    {
        [Test]
        public void Should_Throw_On_Interface_Ref_Without_Impl()
        {
            Action action = () => GetAllServices();

            var ex = action.Should().Throw<MissingInterfaceImplException>();
            ex.And.ClassType.Should().Be(typeof(WithTransientServiceAttributeAndInterface));
            ex.And.InterfaceType.Should().Be(typeof(IService));
        }

        [Test]
        public void Should_Find_N_Services_In_Test_Assembly()
        {
            const int services = 5;

            var (_, fakeServiceCollection) = GetServices();

            A.CallTo(() => fakeServiceCollection.Add(A<ServiceDescriptor>.Ignored))
                .MustHaveHappened(services, Times.Exactly);
        }

        [Test]
        public void Should_Add_Services_By_Their_Attributes_And_Lifetime()
        {
            var (services, _) = GetServices();


            services.Should().ContainServiceWithLifetime(nameof(WithSingletonServiceAttribute), ServiceLifetime.Singleton);
            services.Should().ContainServiceWithLifetime(nameof(WithScopedServiceAttribute), ServiceLifetime.Scoped);
            services.Should().ContainServiceWithLifetime(nameof(WithTransientServiceAttribute), ServiceLifetime.Transient);
            services.Should().ContainBackgroundService(nameof(WithBackgroundServiceAttribute));
        }

        [Test]
        public void Should_Contain_WithTransientServiceAttributeAndInheritInterface_With_IService_Interface()
        {
            var (services, _) = GetServices();
            var serviceDescriptor = services.FindByType(nameof(WithTransientServiceAttributeAndInheritInterface));

            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor.ServiceType.Should().Be(typeof(IService));
        }

        private (ServiceDescriptor[] Services, IServiceCollection fakeServiceCollection) GetServices() =>
            GetAllServices(type => type.Name != nameof(WithTransientServiceAttributeAndInterface));

        private (ServiceDescriptor[] Services, IServiceCollection fakeServiceCollection) GetAllServices(Predicate<Type> typeFilter = null)
        {
            var serviceCollection = A.Fake<IServiceCollection>();
            var services = new List<ServiceDescriptor>();

            A.CallTo(() => serviceCollection.Add(A<ServiceDescriptor>.Ignored))
                .Invokes((ServiceDescriptor serviceDescriptor) => { services.Add(serviceDescriptor); });

            serviceCollection.RegisterServices(configure => configure
                .WithAssemblies(typeof(JustAClass).Assembly)
                .WithTypeFilter(typeFilter)
            );

            return (services.ToArray(), serviceCollection);
        }
    }
}