using System;

namespace Remutable
{
    internal delegate object Activator(params object[] args);

    internal class ActivationContext
    {
        public Type Type { get; }

        public Activator Activator { get; }

        public ParameterResolver[] ParameterResolvers { get; }

        public ActivationContext(Type type, Activator activator, ParameterResolver[] parameterResolvers)
        {
            Type = type;
            Activator = activator;
            ParameterResolvers = parameterResolvers;
        }
    }
}
