using System;
using System.Reflection;

namespace Remutable
{
    internal class ParameterResolver
    {
        public ParameterInfo Parameter { get; }

        public PropertyInfo Property { get; }

        public Func<object, object> Resolver { get; }

        public ParameterResolver(ParameterInfo parameter, PropertyInfo property, Func<object, object> resolver)
        {
            Parameter = parameter;
            Property = property;
            Resolver = resolver;
        }
    }
}
