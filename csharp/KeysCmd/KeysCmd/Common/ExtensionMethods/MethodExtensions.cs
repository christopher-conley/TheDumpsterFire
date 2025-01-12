using System.Reflection;

namespace RosettaTools.CLI.KeysCmd.Common.ExtensionMethods
{
    /// <summary>
    /// All extension methods in this class are currently unused. They are here for
    /// <br>potential use in the future.</br>
    /// </summary>
    public static class MethodExtensions
    {

        public static string GetMethodName(this MethodBase method)
        {
            return method.Name;
        }
        public static string GetClassName(this MethodBase method)
        {
            return method?.DeclaringType?.Name == null ? "Unknown" : method.DeclaringType.Name;
        }
        public static string GetNamespace(this MethodBase method)
        {
            return method?.DeclaringType?.Namespace == null ? "Unknown" : method.DeclaringType.Namespace;
        }
        public static string GetFullyQualifiedName(this MethodBase method)
        {
            Type? declaringType = method?.DeclaringType == null ? null : method.DeclaringType;
            return $"{declaringType?.Namespace}.{declaringType?.Name}.{method?.Name}";
        }
    }
}
