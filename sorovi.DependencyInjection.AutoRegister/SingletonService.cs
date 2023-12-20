using System;
using sorovi.DependencyInjection.AutoRegister.Common;

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

#if NET8_0_OR_GREATER
    public class SingletonServiceAttribute<TServiceType> : SingletonServiceAttribute
    {
        public SingletonServiceAttribute()
            : base(typeof(TServiceType))
        {
        }
    }
#endif
}