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

#if NET8_0_OR_GREATER
    public class ScopedServiceAttribute<TServiceType> : ScopedServiceAttribute
    {
        public ScopedServiceAttribute()
            : base(typeof(TServiceType))
        {
        }
    }
#endif
}