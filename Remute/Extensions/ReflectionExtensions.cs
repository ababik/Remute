using System;
using System.Linq;
using System.Reflection;

namespace Remutable.Extensions
{
    internal static class ReflectionExtensions
    {
        public static ConstructorInfo[] GetInstanceConstructors(Type type)
        {
            return type
                .GetTypeInfo()
                .DeclaredConstructors
                .Where(x => x.IsStatic == false)
                .Where(x => x.IsPublic == true)
                .ToArray();
        }

        public static PropertyInfo[] GetInstanceProperties(Type type)
        {
            return type
                .GetTypeInfo()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                .ToArray();
        }

        public static bool CompareInstanceProperties(PropertyInfo member1, PropertyInfo member2)
        {
            return member1.Module == member2.Module && member1.MetadataToken == member2.MetadataToken;
        }
    }
}