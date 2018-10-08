namespace Remutable
{
    internal delegate object Activator(params object[] args);

    internal class ActivationContext
    {
        public Activator Activator { get; }

        public ParameterResolver[] ParameterResolvers { get; }

        public ActivationContext(Activator activator, ParameterResolver[] parameterResolvers)
        {
            Activator = activator;
            ParameterResolvers = parameterResolvers;
        }
    }
}
