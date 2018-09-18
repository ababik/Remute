using System;
using System.Linq.Expressions;

namespace Remutable.Extensions
{
    public static class ExtensionMethods
    {
        public static TInstance Remute<TInstance, TValue>(this TInstance instance, Expression<Func<TInstance, TValue>> expression, TValue value, Remute remute = null)
        {
            if (remute is null)
            {
                remute = Remutable.Remute.Default;
            }

            return remute.With(instance, expression, value);
        }
    }
}
