using System;
using sorovi.DependencyInjection.AutoRegister.Common;

namespace sorovi.DependencyInjection.AutoRegister
{
    public class TransientServiceAttribute : ServiceAttribute
    {
        public TransientServiceAttribute()
        {
        }

        public TransientServiceAttribute(Type interfaceType)
            : base(interfaceType)
        {
        }
    }

#if NET8_0_OR_GREATER
    public class TransientServiceAttribute<TServiceType> : TransientServiceAttribute
    {
        public TransientServiceAttribute()
            : base(typeof(TServiceType))
        {
        }
    }
#endif
}