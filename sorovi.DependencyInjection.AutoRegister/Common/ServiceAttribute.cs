using System;
using System.Threading;

namespace sorovi.DependencyInjection.AutoRegister
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ServiceAttribute : Attribute
    {
        public Type InterfaceType { get; set; }

        public ServiceAttribute()
        {
        }

        public ServiceAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}