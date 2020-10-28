using System;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using sorovi.DependencyInjection.AutoRegister.TestAssembly;

namespace sorovi.DependencyInjection.AutoRegister.Tests
{
    public class Tests
    {
        [Test]
        public void Should_Find_Service_In_My_Test()
        {
            var serviceCollection = A.Fake<IServiceCollection>();

            serviceCollection.RegisterServices(typeof(Tests).Assembly);


            // TODO should validate that "Service" was registered
            A.CallTo(() => serviceCollection.Add(A<ServiceDescriptor>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public void Should_Find_Five_Services_In_Test_Assembly()
        {
            var serviceCollection = A.Fake<IServiceCollection>();

            serviceCollection.RegisterServices(typeof(JustAClass).Assembly);


            // TODO should validate 
            A.CallTo(() => serviceCollection.Add(A<ServiceDescriptor>.Ignored))
                .MustHaveHappened(5, Times.Exactly);
        }
    }
}