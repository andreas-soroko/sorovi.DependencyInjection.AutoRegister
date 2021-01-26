using System;

namespace sorovi.DependencyInjection.AutoRegister
{
    public class SingletonServiceAttribute : ServiceAttribute
    {
        public SingletonServiceAttribute()
        {
        }

        public SingletonServiceAttribute(Type interfaceType)
            : base(interfaceType)
        {
        }
    }
}