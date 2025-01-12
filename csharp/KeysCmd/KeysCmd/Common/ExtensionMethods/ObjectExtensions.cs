namespace RosettaTools.CLI.KeysCmd.Common.ExtensionMethods
{
    /// <summary>
    /// All extension methods in this class are currently unused. They are here for
    /// <br>potential use in the future.</br>
    /// </summary>
    public static class ObjectExtensions
    {
        public static string GetTypeNamespace(this object obj)
        {
            return $"{obj.GetType().Namespace}";
        }
        public static string GetTypeFullyQualifiedName(this object obj)
        {
            return $"{obj.GetType().Namespace}.{obj.GetType().Name}";
        }
    }
}
