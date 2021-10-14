using System;
using sorovi.DependencyInjection.AutoRegister.Common;

namespace sorovi.DependencyInjection.AutoRegister
{
    public class ScopedServiceAttribute : ServiceAttribute
    {
        public ScopedServiceAttribute()
        {
        }

        public ScopedServiceAttribute(Type interfaceType)
            : base(interfaceType)
        {
        }
    }
}