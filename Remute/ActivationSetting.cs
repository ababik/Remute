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
            var properties = Remute.GetUsableProperties(constructor.DeclaringType);
            parameters.ToList().ForEach(x => Validate(constructor, x.Key, x.Value, properties));

            Constructor = constructor;
            Parameters = parameters;
        }

        private static void Validate(ConstructorInfo constructor, ParameterInfo parameter, PropertyInfo property, PropertyInfo[] properties)
        {
            if (parameter.Member != constructor)
            {
                throw new Exception($"Invalid parameter '{parameter.Name}'. Parameter must be a member of '{constructor.DeclaringType}' constructor.");
            }

            if (properties.SingleOrDefault(x => Remute.SameMembers(x, property)) is null)
            {
                throw new Exception($"Invalid property '{property.Name}'. Must be a member of '{constructor.DeclaringType}'.");
            }
        }
    }
}
