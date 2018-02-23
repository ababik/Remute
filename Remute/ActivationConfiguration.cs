using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Remute
{
    public class ActivationConfiguration
    {
        internal Dictionary<Type, ActivationSetting> Settings { get; }

        public ActivationConfiguration()
        {
            Settings = new Dictionary<Type, ActivationSetting>();
        }

        public ActivationConfiguration Configure(ConstructorInfo constructor)
        {
            return Configure(constructor, new Dictionary<ParameterInfo, PropertyInfo>());
        }

        public ActivationConfiguration Configure(ConstructorInfo constructor, Dictionary<ParameterInfo, PropertyInfo> parameters)
        {
            var type = constructor.DeclaringType;
            Settings[type] = new ActivationSetting(constructor, parameters);

            return this;
        }

        public ActivationConfiguration Configure<T>(Expression<Func<T, T>> expression)
        {
            var constructorExpression = expression.Body as NewExpression;

            if (constructorExpression == null)
            {
                throw new Exception($"Expression must specify constructor of '{typeof(T)}'.");
            }

            var constructor = constructorExpression.Constructor;
            var constructorParameters = constructor.GetParameters();
            var expressionParameters = constructorExpression.Arguments;
            var parameters = new Dictionary<ParameterInfo, PropertyInfo>();

            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var constructorParameter = constructorParameters[i];
                var expressionParameter = expressionParameters[i];
                var propertyExpression = expressionParameter as MemberExpression;
                var property = propertyExpression?.Member as PropertyInfo;

                parameters[constructorParameter] = property 
                    ?? throw new Exception($"Parameter {expressionParameter} must be a property of '{typeof(T)}'.");
            }

            return Configure(constructor, parameters);
        }
    }
}
