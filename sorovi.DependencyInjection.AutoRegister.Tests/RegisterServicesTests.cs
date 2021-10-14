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
        private const int EXPECTED_SERVICE_COUNT = 8; // 5 Services + KnownTypesClass
        private Predicate<Type> _defaultTypeFilter = type => type.Name != nameof(WithTransientServiceAttributeAndInterface);

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
            var serviceCollection = GetServices();

            serviceCollection.Count.Should().Be(EXPECTED_SERVICE_COUNT);
        }

        [Test]
        public void Should_Not_Register_Services_Twice()
        {
            var serviceCollection = GetServices();

            serviceCollection.RegisterServices(configure => configure
                .WithAssemblies(typeof(JustAClass).Assembly)
                .WithTypeFilter(_defaultTypeFilter)
            );

            serviceCollection.Count.Should().Be(EXPECTED_SERVICE_COUNT);
        }

        [Test]
        public void Should_Add_Services_By_Their_Attributes_And_Lifetime()
        {
            var serviceCollection = GetServices();
            var services = serviceCollection.ToArray();

            services.Should().ContainServiceWithLifetime(nameof(WithSingletonServiceAttribute), ServiceLifetime.Singleton);
            services.Should().ContainServiceWithLifetime(nameof(WithScopedServiceAttribute), ServiceLifetime.Scoped);
            services.Should().ContainServiceWithLifetime(nameof(WithTransientServiceAttribute), ServiceLifetime.Transient);
            services.Should().ContainBackgroundService(nameof(WithBackgroundServiceAttribute));
        }

        [Test]
        public void Should_Contain_WithTransientServiceAttributeAndInheritInterface_With_IService_Interface()
        {
            var serviceCollection = GetServices();
            var services = serviceCollection.ToArray();
            var serviceDescriptor = services.FindByType(nameof(WithTransientServiceAttributeAndInheritInterface));

            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor.ServiceType.Should().Be(typeof(IService));
        }

        private IServiceCollection GetServices() =>
            GetAllServices(_defaultTypeFilter);

        private IServiceCollection GetAllServices(Predicate<Type> typeFilter = null)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.RegisterServices(configure => configure
                .WithAssemblies(typeof(JustAClass).Assembly)
                .WithTypeFilter(typeFilter)
            );

            return serviceCollection;
        }
    }
}