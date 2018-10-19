using System;

namespace Remutable
{
    internal class ActivationContextCacheKey
    {
        private Type Source { get; }

        private Type Target { get; }

        private int HashCode { get; }

        public ActivationContextCacheKey(Type source, Type target)
        {
            Source = source;
            Target = target;
            HashCode = Source.GetHashCode() ^ Target.GetHashCode();
        }

        public override bool Equals(object instance)
        {
            return (instance is ActivationContextCacheKey that) && (that.Source == this.Source) && (that.Target == this.Target);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }
    }
}