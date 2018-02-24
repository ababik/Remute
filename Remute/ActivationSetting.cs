using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Remutable
{
    internal class ActivationSetting
    {
        public ConstructorInfo Constructor { get; }
        public Dictionary<ParameterInfo, PropertyInfo> Parameters { get; }

        public ActivationSetting(ConstructorInfo constructor, Dictionary<ParameterInfo, PropertyInfo> parameters)
        {
            parameters.ToList().ForEach(x => Validate(constructor, x.Key, x.Value));

            Constructor = constructor;
            Parameters = parameters;
        }

        private static void Validate(ConstructorInfo constructor, ParameterInfo parameter, PropertyInfo property)
        {
            if (parameter.Member != constructor)
            {
                throw new Exception($"Invalid parameter '{parameter.Name}'. Parameter must be a member of '{constructor.DeclaringType}' constructor.");
            }

            if (property.DeclaringType != constructor.DeclaringType)
            {
                throw new Exception($"Invalid property '{property.Name}'. Must be a member of '{constructor.DeclaringType}'.");
            }
        }
    }
}
