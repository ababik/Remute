using System;
using System.Linq;
using System.Linq.Expressions;

namespace Remutable
{
    internal class PropertyDelegateCacheKey
    {
        private Type Type { get; }

        private string Value { get; }

        private int HashCode { get; }

        public PropertyDelegateCacheKey(Type type, MemberExpression memberExpression)
        {
            Type = type;

            var value = memberExpression.ToString();
            var index = value.IndexOf(Type.Delimiter);

            try
            {
                Value = value.Remove(0, index + 1);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse expression '{memberExpression}'. Property expression is expected.", ex);
            }

            HashCode = Type.GetHashCode() ^ Value.GetHashCode();
        }

        public override bool Equals(object instance)
        {
            return (instance is PropertyDelegateCacheKey that) && (that.Type == this.Type) && (that.Value == this.Value);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }
    }
}