using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Remute
{
    public class Remute
    {
        internal Dictionary<Type, ActivationContext> ActivationContextCach { get; }

        public Remute()
        {
            ActivationContextCach = new Dictionary<Type, ActivationContext>();
        }

        public TInstance With<TInstance, TValue>(TInstance instance, Expression<Func<TInstance, TValue>> expression, TValue value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var result = value as object;

            var instanceExpression = expression.Body;

            while (instanceExpression is MemberExpression)
            {
                var propertyExpression = instanceExpression as MemberExpression;
                instanceExpression = propertyExpression.Expression;

                var property = propertyExpression.Member as PropertyInfo;
                var type = property.DeclaringType;

                var activationContext = GetActivationContext(type);

                var lambdaExpression = Expression.Lambda<Func<TInstance, object>>(instanceExpression, expression.Parameters);
                var compiledExpression = lambdaExpression.Compile();
                var currentInstance = compiledExpression.Invoke(instance);

                var arguments = ResolveActivatorArguments(activationContext.ParameterResolvers, property, currentInstance, ref result);
                result = activationContext.Activator.Invoke(arguments);
            }

            return (TInstance)result;
        }

        private ActivationContext GetActivationContext(Type type)
        {
            if (ActivationContextCach.TryGetValue(type, out ActivationContext result))
            {
                return result;
            }

            var constructor = FindConstructor(type);
            var activator = GetActivator(constructor);
            var parameterResolvers = GetParameterResolvers(type, constructor);

            result = new ActivationContext(type, activator, parameterResolvers);
            ActivationContextCach[type] = result;

            return result;
        }

        private static ConstructorInfo FindConstructor(Type type)
        {
            var constructors = type.GetTypeInfo().DeclaredConstructors;

            if (constructors.Count() != 1)
            {
                throw new Exception($"Unable to find appropriate constructor of type '{type.Name}'.");
            }

            return constructors.Single();
        }

        private static PropertyInfo FindProperty(Type type, ParameterInfo parameter, PropertyInfo[] properties)
        {
            properties = properties.Where(x => string.Equals(x.Name, parameter.Name, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (properties.Count() != 1)
            {
                throw new Exception($"Unable to find appropriate property to use as a constructor parameter '{parameter.Name}'. Type '{type.Name}'.");
            }

            return properties.Single();
        }

        private static Activator GetActivator(ConstructorInfo constructor)
        {
            var parameters = constructor.GetParameters();

            var parameterExpression = Expression.Parameter(typeof(object[]));
            var argumentExpressions = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var indexExpression = Expression.Constant(i);
                var paramType = parameters[i].ParameterType;
                var arrayExpression = Expression.ArrayIndex(parameterExpression, indexExpression);
                var arrayExpressionConvert = Expression.Convert(arrayExpression, paramType);
                argumentExpressions[i] = arrayExpressionConvert;
            }

            var constructorExpression = Expression.New(constructor, argumentExpressions);
            var lambdaExpression = Expression.Lambda<Activator>(constructorExpression, parameterExpression);
            var compiledExpression = lambdaExpression.Compile();
            return compiledExpression;
        }

        private static ParameterResolver[] GetParameterResolvers(Type type, ConstructorInfo constructor)
        {
            var properties = type.GetTypeInfo().DeclaredProperties.ToArray();
            var parameters = constructor.GetParameters();

            var parameterResolvers = new ParameterResolver[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var property = FindProperty(type, parameter, properties);

                var expressionParameter = Expression.Parameter(typeof(object));
                var expressionParameterConvert = Expression.Convert(expressionParameter, type);
                var expressionProperty = Expression.Property(expressionParameterConvert, property);
                var expressionPropertyConvert = Expression.Convert(expressionProperty, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(expressionPropertyConvert, expressionParameter);
                var resolver = lambda.Compile();

                var parameterResolver = new ParameterResolver(parameter, property, resolver);
                parameterResolvers[i] = parameterResolver;
            }

            return parameterResolvers;
        }

        private static object[] ResolveActivatorArguments(ParameterResolver[] parameterResolvers, PropertyInfo property, object instance, ref object result)
        {
            var arguments = new object[parameterResolvers.Length];

            for (var i = 0; i < parameterResolvers.Length; i++)
            {
                var parameterResolver = parameterResolvers[i];
                var argument = default(object);

                if (parameterResolver.Property == property)
                {
                    argument = result;
                    result = instance;
                }
                else
                {
                    argument = parameterResolver.Resolver.Invoke(instance);
                }

                arguments[i] = argument;
            }

            return arguments;
        }
    }
}
