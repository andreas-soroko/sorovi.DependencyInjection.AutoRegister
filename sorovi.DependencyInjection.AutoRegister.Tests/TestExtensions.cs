using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace sorovi.DependencyInjection.AutoRegister.Tests
{
    public static class TestExtensions
    {
        public static AndWhichConstraint<GenericCollectionAssertions<ServiceDescriptor>, ServiceDescriptor> ContainService(
            this GenericCollectionAssertions<ServiceDescriptor> sAssertions,
            string name
        ) =>
            sAssertions
                .Contain(s => s.ImplementationType != null && s.ImplementationType.Name == name);

        public static AndWhichConstraint<GenericCollectionAssertions<ServiceDescriptor>, ServiceDescriptor> ContainServiceWithLifetime(
            this GenericCollectionAssertions<ServiceDescriptor> sAssertions,
            Type implementationType,
            ServiceLifetime lifetime
        ) =>
            sAssertions
                .Contain(s => s.ImplementationType != null && s.ImplementationType == implementationType && s.Lifetime == lifetime);

        public static ServiceDescriptor FindByType(this ServiceDescriptor[] serviceDescriptors, string name) =>
            serviceDescriptors.FirstOrDefault(s => s.ImplementationType != null && s.ImplementationType.Name == name);
    }
}