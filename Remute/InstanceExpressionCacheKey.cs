using System;
using System.Linq;
using System.Linq.Expressions;

namespace Remutable
{
    internal class InstanceExpressionCacheKey
    {
        private Type Type { get; }

        private string Value { get; }

        private int HashCode { get; }

        public InstanceExpressionCacheKey(Type type, Expression expression)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type = type;

            var value = expression.ToString();
            var index = value.IndexOf(Type.Delimiter);

            try
            {
                Value = value.Remove(0, index + 1);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse expression '{expression}'.", ex);
            }

            HashCode = Type.GetHashCode() ^ Value.GetHashCode();
        }

        public override bool Equals(object instance)
        {
            return (instance is InstanceExpressionCacheKey that) && (that.Type == this.Type) && (that.Value == this.Value);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }
    }
}