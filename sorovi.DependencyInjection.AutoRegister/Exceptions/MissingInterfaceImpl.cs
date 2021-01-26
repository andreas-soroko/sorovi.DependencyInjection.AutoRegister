using System;

namespace sorovi.DependencyInjection.AutoRegister.Exceptions
{
    public class MissingInterfaceImplException : Exception
    {
        public Type ClassType { get; }
        public Type InterfaceType { get; }

        public MissingInterfaceImplException(Type classType, Type interfaceType)
            : base($"Class('{classType.Name}') does not implement referenced Interface('{interfaceType.Name}')")
        {
            ClassType = classType;
            InterfaceType = interfaceType;
        }
    }
}