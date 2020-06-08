using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Remutable.Extensions.ReflectionExtensions;

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

        private delegate object ResolveInstanceDelegate<T>(T source);

        private ActivationConfiguration ActivationConfiguration { get; }
        private ConcurrentDictionary<ActivationContextCacheKey, ActivationContext> ActivationContextCache { get; }
        private ConcurrentDictionary<InstanceExpressionCacheKey, Delegate> ResolveInstanceExpressionCache { get; }

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
            ActivationContextCache = new ConcurrentDictionary<ActivationContextCacheKey, ActivationContext>();
            ResolveInstanceExpressionCache = new ConcurrentDictionary<InstanceExpressionCacheKey, Delegate>();
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

            var processContext = new ProcessContext<TInstance>()
            {
                Source = source,
                Target = value as object,
                SourceParameterExpression = expression.Parameters.Single(),
                InstanceExpression = expression.Body
            };

            var actualValue = (TValue)ResolveInstance(processContext);
            if (object.Equals(actualValue, value))
            {
                return source;
            }

            while (processContext.InstanceExpression != processContext.SourceParameterExpression)
            {
                if (TryProcessMemberExpression(processContext)) continue;

                throw new NotSupportedException($"Unable to process expression. Expression: '{processContext.InstanceExpression}'.");
            }

            var target = (TInstance)processContext.Target;

            processContext.AffectedProperties.Reverse();
            OnEmit?.Invoke(source, target, value, processContext.AffectedProperties.ToArray());

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

        private bool TryProcessMemberExpression<TSource>(ProcessContext<TSource> processContext)
        {
            if (processContext.InstanceExpression is MemberExpression memberExpression && memberExpression.Member is PropertyInfo property)
            {
                processContext.InstanceExpression = memberExpression.Expression;

                var result = processContext.Target;
                var type = memberExpression.Expression.Type;
                var instance = ResolveInstance(processContext);
                var activationContext = GetActivationContext(type, type);
                var arguments = ResolveActivatorArguments(activationContext.ParameterResolvers, property, instance, ref result);

                processContext.Target = activationContext.Activator.Invoke(arguments);
                processContext.AffectedProperties.Add(property.Name);

                return true;
            }

            return false;
        }

        private ActivationContext GetActivationContext(Type source, Type target)
        {
            var key = new ActivationContextCacheKey(source, target);

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

            var constructors = GetInstanceConstructors(type);

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
            var properties = GetInstanceProperties(type);
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

                if (property != null && CompareInstanceProperties(parameterResolver.Property, property))
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

        private object ResolveInstance<TSource>(ProcessContext<TSource> processContext)
        {
            var key = new InstanceExpressionCacheKey(typeof(TSource), processContext.InstanceExpression);
            var compiledExpression = default(ResolveInstanceDelegate<TSource>);

            if (ResolveInstanceExpressionCache.TryGetValue(key, out var resolveInstanceDelegate))
            {
                compiledExpression = (ResolveInstanceDelegate<TSource>)resolveInstanceDelegate;
            }
            else
            {
                var instanceConvertExpression = Expression.Convert(processContext.InstanceExpression, typeof(object));
                var lambdaExpression = Expression.Lambda<ResolveInstanceDelegate<TSource>>(instanceConvertExpression, processContext.SourceParameterExpression);
                compiledExpression = lambdaExpression.Compile();
                ResolveInstanceExpressionCache[key] = compiledExpression;
            }

            return compiledExpression.Invoke(processContext.Source);
        }
    }
}
