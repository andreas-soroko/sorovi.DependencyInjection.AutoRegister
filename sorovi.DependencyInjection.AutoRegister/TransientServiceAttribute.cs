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
}