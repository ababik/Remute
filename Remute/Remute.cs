using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Remutable
{
    /// <summary>
    /// Contructs immutable objects mapping existing object properties to constructor parameters.
    /// Helpful working with types without parameterless contructor.
    /// </summary>
    public class Remute
    {
        /// <summary>
        /// Remute default instance.
        /// Expects immutable type to have single constructor with parameters matching property names (case-insensetive). 
        /// Use <see cref="ActivationConfiguration"/> to change this default behaviour.
        /// </summary>
        public static Remute Default { get; } = new Remute();

        public delegate void EmitEventHandler(object source, object target, object value, string[] affectedProperties);
        public event EmitEventHandler OnEmit;

        private ActivationConfiguration ActivationConfiguration { get; }
        private Dictionary<Guid, ActivationContext> ActivationContextCache { get; }

        /// <summary>
        /// Creates Remute instance.
        /// By default expects immutable type to have single constructor with parameters matching property names (case-insensetive). 
        /// Use overloaded constructor with <see cref="ActivationConfiguration"/> to change this default behaviour.
        /// </summary>
        public Remute()
            : this(new ActivationConfiguration())
        {
        }

        /// <summary>
        /// Creates Remute instance.
        /// Configuring which constructor to use (if there are multiple) and how to map property to parameter names (if names do not match).
        /// </summary>
        /// <param name="activationConfiguration">Immutable object creation configurations.</param>
        public Remute(ActivationConfiguration activationConfiguration)
        {
            ActivationConfiguration = activationConfiguration ?? throw new ArgumentNullException(nameof(activationConfiguration));
            ActivationContextCache = new Dictionary<Guid, ActivationContext>();
        }

        /// <summary>
        /// Contructs immutable object from existing one with changed property specified by lambda expression.
        /// </summary>
        /// <typeparam name="TInstance">Immutable object type.</typeparam>
        /// <typeparam name="TValue">Value to set type.</typeparam>
        /// <param name="source">Original immutable object.</param>
        /// <param name="expression">Navigation property specifying what to change.</param>
        /// <param name="value">Value to set in the resulting object.</param>
        /// <returns>Immutable object with changed property.</returns>
        public TInstance With<TInstance, TValue>(TInstance source, Expression<Func<TInstance, TValue>> expression, TValue value)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var result = value as object;
            var instanceExpression = expression.Body;

            var affectedProperties = new List<string>();

            while (instanceExpression is MemberExpression propertyExpression)
            {
                instanceExpression = propertyExpression.Expression;
                var instanceConvertExpression = Expression.Convert(instanceExpression, typeof(object));

                if (!(propertyExpression.Member is PropertyInfo property))
                {
                    throw new Exception($"Type member '{propertyExpression.Member.Name}' is expected to be a property.");
                }

                var type = property.DeclaringType;

                var activationContext = GetActivationContext(type, type);

                var lambdaExpression = Expression.Lambda<Func<TInstance, object>>(instanceConvertExpression, expression.Parameters);
                var compiledExpression = lambdaExpression.Compile();
                var currentInstance = compiledExpression.Invoke(source);

                var arguments = ResolveActivatorArguments(activationContext.ParameterResolvers, property, currentInstance, ref result);
                result = activationContext.Activator.Invoke(arguments);

                affectedProperties.Add(property.Name);
            }

            var target = (TInstance)result;

            affectedProperties.Reverse();
            OnEmit?.Invoke(source, target, value, affectedProperties.ToArray());

            return target;
        }

        /// <summary>
        /// Constructs immutable object from any other object. 
        /// Helpful cloning immutable object or converting POCO, DTO, anonymous type, dynamic ect.
        /// </summary>
        /// <typeparam name="TInstance">Immutable object type.</typeparam>
        /// <param name="source">Original object.</param>
        /// <returns>Immutable object.</returns>
        public TInstance With<TInstance>(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var sourceType = source.GetType();
            var targetType = typeof(TInstance);

            var activationContext = GetActivationContext(sourceType, targetType);

            var result = default(object);
            var arguments = ResolveActivatorArguments(activationContext.ParameterResolvers, null, source, ref result);
            result = activationContext.Activator.Invoke(arguments);

            var target = (TInstance)result;

            OnEmit?.Invoke(source, target, null, null);

            return target;
        }

        private ActivationContext GetActivationContext(Type source, Type target)
        {
            var key = GetActivationContextCacheKey(source, target);

            if (ActivationContextCache.TryGetValue(key, out var result))
            {
                return result;
            }

            var constructor = FindConstructor(target);
            var activator = GetActivator(constructor);
            var parameterResolvers = GetParameterResolvers(source, constructor);

            result = new ActivationContext(activator, parameterResolvers);
            ActivationContextCache[key] = result;

            return result;
        }

        private ConstructorInfo FindConstructor(Type type)
        {
            if (ActivationConfiguration.Settings.TryGetValue(type, out var setting))
            {
                return setting.Constructor;
            }

            var constructors = type.GetTypeInfo().DeclaredConstructors;

            if (constructors.Count() != 1)
            {
                throw new Exception($"Unable to find appropriate constructor of type '{type.Name}'. Consider to use {nameof(ActivationConfiguration)} parameter.");
            }

            return constructors.Single();
        }

        private PropertyInfo FindProperty(Type type, ParameterInfo parameter, PropertyInfo[] properties)
        {
            if (ActivationConfiguration.Settings.TryGetValue(type, out var setting))
            {
                if (setting.Parameters.TryGetValue(parameter, out var property))
                {
                    return property;
                }
            }

            properties = properties.Where(x => string.Equals(x.Name, parameter.Name, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (properties.Count() != 1)
            {
                throw new Exception($"Unable to find appropriate property to use as a constructor parameter '{parameter.Name}'. Type '{type.Name}'. Consider to use {nameof(ActivationConfiguration)} parameter.");
            }

            return properties.Single();
        }

        private Activator GetActivator(ConstructorInfo constructor)
        {
            var parameters = constructor.GetParameters();

            var parameterExpression = Expression.Parameter(typeof(object[]));
            var argumentExpressions = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var indexExpression = Expression.Constant(i);
                var paramType = parameters[i].ParameterType;
                var arrayExpression = Expression.ArrayIndex(parameterExpression, indexExpression);
                var arrayConvertExpression = Expression.Convert(arrayExpression, paramType);
                argumentExpressions[i] = arrayConvertExpression;
            }

            var constructorExpression = Expression.New(constructor, argumentExpressions);
            var constructorConvertExpression = Expression.Convert(constructorExpression, typeof(object));
            var lambdaExpression = Expression.Lambda<Activator>(constructorConvertExpression, parameterExpression);
            var compiledExpression = lambdaExpression.Compile();
            return compiledExpression;
        }

        private ParameterResolver[] GetParameterResolvers(Type type, ConstructorInfo constructor)
        {
            var properties = type.GetTypeInfo().DeclaredProperties.ToArray();
            var parameters = constructor.GetParameters();

            var parameterResolvers = new ParameterResolver[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                var property = FindProperty(type, parameter, properties);

                var parameterExpression = Expression.Parameter(typeof(object));
                var parameterConvertExpression = Expression.Convert(parameterExpression, type);
                var propertyExpression = Expression.Property(parameterConvertExpression, property);
                var propertyConvertExpression = Expression.Convert(propertyExpression, typeof(object));
                var lambdaExpression = Expression.Lambda<Func<object, object>>(propertyConvertExpression, parameterExpression);
                var compiledExpression = lambdaExpression.Compile();

                var parameterResolver = new ParameterResolver(parameter, property, compiledExpression);
                parameterResolvers[i] = parameterResolver;
            }

            return parameterResolvers;
        }

        private object[] ResolveActivatorArguments(ParameterResolver[] parameterResolvers, PropertyInfo property, object instance, ref object result)
        {
            var arguments = new object[parameterResolvers.Length];
            var match = false;

            for (var i = 0; i < parameterResolvers.Length; i++)
            {
                var parameterResolver = parameterResolvers[i];
                var argument = default(object);

                if (parameterResolver.Property == property)
                {
                    argument = result;
                    result = instance;
                    match = true;
                }
                else
                {
                    argument = parameterResolver.Resolver.Invoke(instance);
                }

                arguments[i] = argument;
            }

            if (property != null && match == false)
            {
                throw new Exception($"Unable to construct object of type '{property.DeclaringType.Name}'. There is no constructor parameter matching property '{property.Name}'.");
            }

            return arguments;
        }

        private static Guid GetActivationContextCacheKey(Type type1, Type type2)
        {
            var array1 = type1.GetTypeInfo().GUID.ToByteArray();
            var array2 = type2.GetTypeInfo().GUID.ToByteArray();

            var num1 = BitConverter.ToUInt64(array1, 0) ^ BitConverter.ToUInt64(array2, 8);
            var num2 = BitConverter.ToUInt64(array1, 8) ^ BitConverter.ToUInt64(array2, 0);

            return new Guid(BitConverter.GetBytes(num1).Concat(BitConverter.GetBytes(num2)).ToArray());
        }
    }
}
