using System;

namespace sorovi.DependencyInjection.AutoRegister.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ServiceAttribute : Attribute
    {
        public Type InterfaceType { get; set; }
        public Mode Mode { get; set; } = Mode.TryAdd;

        public ServiceAttribute()
        {
        }

        public ServiceAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}