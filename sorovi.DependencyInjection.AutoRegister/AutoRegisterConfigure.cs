using System;
using System.Reflection;

namespace sorovi.DependencyInjection.AutoRegister
{
    public class AutoRegisterConfigure
    {
        internal Assembly[] Assemblies;
        internal Predicate<Type> TypeFilter;

        public AutoRegisterConfigure WithAssemblies(params Assembly[] assemblies)
        {
            Assemblies = assemblies;
            return this;
        }

        public AutoRegisterConfigure WithTypeFilter(Predicate<Type> typeFilter)
        {
            TypeFilter = typeFilter;
            return this;
        }
    }
}