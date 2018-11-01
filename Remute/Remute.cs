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

        private delegate object ResolveInstanceDelegate<T>(T source, int[] indexes);
        private delegate void AssignIndexDelegate<T>(T source, int[] indexes, object value);
        private delegate int ResolveIndexParameterDelegate();

        private ActivationConfiguration ActivationConfiguration { get; }
        private Dictionary<ActivationContextCacheKey, ActivationContext> ActivationContextCache { get; }
        private Dictionary<InstanceExpressionCacheKey, Delegate> ResolveInstanceExpressionCache { get; }
        private Dictionary<InstanceExpressionCacheKey, Delegate> AssignIndexExpressionCache { get; }
        private Dictionary<InstanceExpressionCacheKey, ResolveIndexParameterDelegate> ResolveIndexParameterExpressionCache { get; }

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
            ActivationContextCache = new Dictionary<ActivationContextCacheKey, ActivationContext>();
            ResolveInstanceExpressionCache = new Dictionary<InstanceExpressionCacheKey, Delegate>();
            AssignIndexExpressionCache = new Dictionary<InstanceExpressionCacheKey, Delegate>();
            ResolveIndexParameterExpressionCache = new Dictionary<InstanceExpressionCacheKey, ResolveIndexParameterDelegate>();
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

            ExtractIndexExpressions(processContext);

            while (processContext.InstanceExpression != processContext.SourceParameterExpression)
            {
                if (TryProcessMemberExpression(processContext)) continue;
                if (TryProcessArrayIndexExpression(processContext)) continue;

                throw new NotSupportedException($"Unable to process expresion. Expression: '{processContext.InstanceExpression}'.");
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
            if (processContext.InstanceExpression is MemberExpression memberExpression)
            {
                processContext.InstanceExpression = memberExpression.Expression;

                var result = processContext.Target;
                var property = memberExpression.Member as PropertyInfo;
                var type = property.DeclaringType;
                var instance = ResolveInstance(processContext);
                var activationContext = GetActivationContext(type, type);
                var arguments = ResolveActivatorArguments(activationContext.ParameterResolvers, property, instance, ref result);

                processContext.Target = activationContext.Activator.Invoke(arguments);

                if (processContext.IsLastProcessedIndex == false)
                {
                    processContext.AffectedProperties.Add(property.Name);
                }

                processContext.IsLastProcessedIndex = false;

                return true;
            }

            return false;
        }

        private bool TryProcessArrayIndexExpression<TSource>(ProcessContext<TSource> processContext)
        {
            if (processContext.InstanceExpression is IndexExpression indexExpression && indexExpression.Object is MemberExpression memberExpression)
            {
                processContext.InstanceExpression = memberExpression;

                var property = memberExpression.Member as PropertyInfo;

                var key = new InstanceExpressionCacheKey(typeof(TSource), processContext.InstanceExpression);
                var compiledExpression = default(AssignIndexDelegate<TSource>);

                if (AssignIndexExpressionCache.TryGetValue(key, out var assignIndexDelegate))
                {
                    compiledExpression = (AssignIndexDelegate<TSource>)assignIndexDelegate;
                }
                else
                {
                    var elementType = property.PropertyType.GetElementType();
                    var propertyExpression = Expression.Property(memberExpression.Expression, property);
                    var arrayAccessExpression = Expression.ArrayAccess(propertyExpression, indexExpression.Arguments[0]);
                    var parameterExpression = Expression.Parameter(typeof(object));
                    var parameterConvertExpression = Expression.Convert(parameterExpression, elementType);
                    var assignExpression = Expression.Assign(arrayAccessExpression, parameterConvertExpression);
                    var lambdaExpression = Expression.Lambda<AssignIndexDelegate<TSource>>(assignExpression, processContext.SourceParameterExpression, processContext.IndexParameterExpression, parameterExpression);
                    compiledExpression = lambdaExpression.Compile();

                    AssignIndexExpressionCache[key] = compiledExpression;
                }

                compiledExpression.Invoke(processContext.Source, processContext.IndexParameterValues.ToArray(), processContext.Target);

                processContext.Target = ResolveInstance(processContext);

                var lastIndex = processContext.IndexParameterValues.Count - 1;
                var lastValue = processContext.IndexParameterValues[lastIndex];
                processContext.IndexParameterValues.RemoveAt(lastIndex);

                processContext.AffectedProperties.Add(property.Name + "[" + lastValue + "]");
                processContext.IsLastProcessedIndex = true;

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
                var lambdaExpression = Expression.Lambda<ResolveInstanceDelegate<TSource>>(instanceConvertExpression, processContext.SourceParameterExpression, processContext.IndexParameterExpression);
                compiledExpression = lambdaExpression.Compile();
                ResolveInstanceExpressionCache[key] = compiledExpression;
            }

            return compiledExpression.Invoke(processContext.Source, processContext.IndexParameterValues.ToArray());
        }

        private int ResolveIndexParameter(Expression expression)
        {
            var key = new InstanceExpressionCacheKey(null, expression);

            if (ResolveIndexParameterExpressionCache.TryGetValue(key, out var resolveIndexParameterDelegate) == false)
            {
                var lambdaExpression = Expression.Lambda<ResolveIndexParameterDelegate>(expression);
                resolveIndexParameterDelegate = lambdaExpression.Compile();
                ResolveIndexParameterExpressionCache[key] = resolveIndexParameterDelegate;
            }

            var result = resolveIndexParameterDelegate.Invoke();

            return result;
        }

        private void ExtractIndexExpressions<TSource>(ProcessContext<TSource> processContext)
        {
            var properties = new List<PropertyInfo>();
            var indexes = new List<int?>();
            var expression = processContext.InstanceExpression;

            while (expression != processContext.SourceParameterExpression)
            {
                {
                    if (expression is MemberExpression memberExpression &&
                        memberExpression.Member is PropertyInfo property)
                    {
                        expression = memberExpression.Expression;
                        properties.Add(property);
                        indexes.Add(null);
                        continue;
                    }
                }

                {
                    if (expression is BinaryExpression binaryExpression &&
                        binaryExpression.Left is MemberExpression memberExpression &&
                        memberExpression.Member is PropertyInfo property)
                    {
                        var constantExpression = default(ConstantExpression);
                        var index = default(int?);

                        if (binaryExpression.Right is ConstantExpression)
                        {
                            constantExpression = binaryExpression.Right as ConstantExpression;
                            index = (int)constantExpression.Value;
                        }
                        if (binaryExpression.Right is MemberExpression fieldExpression)
                        {
                            index = ResolveIndexParameter(fieldExpression);
                        }
                        if (index.HasValue)
                        {
                            expression = memberExpression.Expression;
                            properties.Add(property);
                            indexes.Add(index.Value);
                            continue;
                        }
                    }
                }

                throw new NotSupportedException($"Unable to process expression. Expression: '{expression}'.");
            }

            processContext.InstanceExpression = processContext.SourceParameterExpression;
            processContext.IndexParameterExpression = Expression.Parameter(typeof(int[]), "index");
            processContext.IndexParameterValues = new List<int>();

            properties.Reverse();
            indexes.Reverse();

            var indexCounter = 0;

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var index = indexes[i];

                if (index.HasValue)
                {
                    var arrayAccessExpression = Expression.ArrayAccess(processContext.IndexParameterExpression, Expression.Constant(indexCounter));
                    indexCounter++;

                    var propertyExpression = Expression.Property(processContext.InstanceExpression, property);
                    processContext.InstanceExpression = Expression.ArrayAccess(propertyExpression, arrayAccessExpression);

                    processContext.IndexParameterValues.Add(index.Value);
                }
                else
                {
                    processContext.InstanceExpression = Expression.Property(processContext.InstanceExpression, property);
                }
            }
        }
    }
}
