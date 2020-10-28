using System;

namespace sorovi.DependencyInjection.AutoRegister
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BackgroundServiceAttribute : Attribute
    {
    }
}