using System;
using System.Threading;

namespace sorovi.DependencyInjection.AutoRegister
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ServiceAttribute : Attribute
    {
        public Type InterfaceType { get; set; }
        public LifeTimeType LifeTime { get; set; }

        public ServiceAttribute()
        {
        }

        public ServiceAttribute(LifeTimeType lifeTime)
            : this(null, lifeTime)
        {
        }

        public ServiceAttribute(Type interfaceType, LifeTimeType lifeTime = LifeTimeType.Scoped)
        {
            LifeTime = lifeTime;
            InterfaceType = interfaceType;
        }


        public enum LifeTimeType
        {
            Singleton,
            Scoped,
            Transient
        }
    }
}