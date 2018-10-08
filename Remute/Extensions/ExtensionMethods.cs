using System;
using System.Linq.Expressions;

namespace Remutable.Extensions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Contructs immutable object from existing one with changed property specified by lambda expression.
        /// </summary>
        /// <typeparam name="TInstance">Immutable object type.</typeparam>
        /// <typeparam name="TValue">Value to set type.</typeparam>
        /// <param name="instance">Original immutable object.</param>
        /// <param name="expression">Navigation property specifying what to change.</param>
        /// <param name="value">Value to set in the resulting object.</param>
        /// <param name="remute">Configuration to use. Default if not specified.</param>
        /// <returns></returns>
        public static TInstance Remute<TInstance, TValue>(this TInstance instance, Expression<Func<TInstance, TValue>> expression, TValue value, Remute remute = null)
        {
            if (remute is null)
            {
                remute = Remutable.Remute.Default;
            }

            return remute.With(instance, expression, value);
        }

        /// <summary>
        /// Constructs immutable object from any other object. 
        /// Helpful cloning immutable object or converting POCO, DTO, anonymous type, dynamic ect.
        /// </summary>
        /// <typeparam name="TInstance">Immutable object type.</typeparam>
        /// <param name="source">Original object.</param>
        /// <param name="remute"></param>
        /// <returns>Configuration to use. Default if not specified.</returns>
        public static TInstance Remute<TInstance>(this object source, Remute remute = null)
        {
            if (remute is null)
            {
                remute = Remutable.Remute.Default;
            }

            return remute.With<TInstance>(source);
        }
    }
}
