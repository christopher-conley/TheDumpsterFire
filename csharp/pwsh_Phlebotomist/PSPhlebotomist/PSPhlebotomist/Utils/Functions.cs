using PSPhlebotomist.Common;
using PSPhlebotomist.Core.Helpers;
using Serilog.Context;
using System.Collections;
using System.Runtime.CompilerServices;

namespace PSPhlebotomist.Utils
{
    public class Functions
    {

        [ModuleInitializer]
        public static void InitMod()
        {
            _ = FormatObject("This is useless and only exists to ensure that we get touched on init");
        }

        /// <summary>
        /// Directly formats an object to a string representation, recursively handling collections, 
        /// dictionaries, anonymous types, complex objects, etc
        /// </summary>
        /// <param name="obj">The object to format.</param>
        /// <param name="depth">Current recursion depth (default: 0).</param>
        /// <returns>A formatted string representation of the object.</returns>
        public static string FormatObject(object obj, int depth = 0)
        {
            return PreprocessArgument(obj, depth)?.ToString() ?? "null";
        }

        /// <summary>
        /// Recursively formats objects, like collections and complex types.
        /// </summary>
        public static object PreprocessArgument(object arg, int depth = 0)
        {

            // Prevent infinite recursion
            const int maxDepth = 20;

            if (arg == null || depth > maxDepth)
            {
                return arg?.ToString() ?? "null";
            }

            // Handle strings and other primitives directly
            var type = arg.GetType();

            if (type.IsPrimitive ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(Guid))
            {
                return arg;
            }

            // Handle collections (arrays, lists, etc.) BEFORE checking for dictionaries
            // This prevents arrays from being treated as enumerable first
            if (arg is Array array)
            {
                var items = new List<string>();
                foreach (var item in array)
                {
                    var processedItem = PreprocessArgument(item, depth + 1);
                    items.Add(processedItem?.ToString() ?? "null");
                }

                return $"[{string.Join(", ", items)}]";
            }

            // Handle dictionaries specially
            if (arg is IDictionary dictionary)
            {
                var items = new List<string>();
                foreach (DictionaryEntry entry in dictionary)
                {
                    var key = PreprocessArgument(entry.Key, depth + 1);
                    var value = PreprocessArgument(entry.Value, depth + 1);
                    items.Add($"{key}: {value}");
                }

                return $"{{{string.Join(", ", items)}}}";
            }

            // Handle other enumerable-type shit (lists, etc.)
            if (arg is IEnumerable enumerable && !(arg is string))
            {
                var items = new List<string>();
                foreach (var item in enumerable)
                {
                    var processedItem = PreprocessArgument(item, depth + 1);
                    items.Add(processedItem?.ToString() ?? "null");
                }

                return $"[{string.Join(", ", items)}]";
            }

            // Handle anonymous types and other complex objects
            // Anonymous types have a null namespace, so check for that OR a non-System namespace
            if (!type.IsValueType)
            {
                var properties = type.GetProperties();
                if (properties.Length > 0)
                {
                    // Check if it's an anonymous type or if it's a custom type (non-System namespace)
                    bool isAnonymousType = type.Name.Contains("<>") || type.Name.Contains("AnonymousType");
                    bool isCustomType = type.Namespace == null || !type.Namespace.StartsWith("System");
                    bool isPowerShellType = type.Namespace != null && type.Namespace.StartsWith("System.Management.Automation");

                    if (isAnonymousType || isPowerShellType || isCustomType)
                    {
                        var propStrings = new List<string>();
                        foreach (var prop in properties)
                        {
                            try
                            {
                                var value = prop.GetValue(arg);
                                var processedValue = PreprocessArgument(value, depth + 1);
                                propStrings.Add($"{prop.Name}: {processedValue}");
                            }
                            catch
                            {
                                //Debug.WriteLine($"Failed to get value for property {prop.Name} of type {type.FullName}");
                                Console.WriteLine($"Warning: Failed to get value for property {prop.Name} of type {type.FullName}, skipping.");
                            }
                        }

                        return $"{{ {string.Join(", ", propStrings)} }}";
                    }
                }
            }

            // If we've reached this point, I'm not sure if even Jesus knows wtf this thing is,
            // so let's just fuckin' send it
            return arg.ToString();
        }


        public static bool NumIsSignedType(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.Signed ?? false;
        }

        public static bool NumIsUnSignedType(Type type)
        {
            return !NumIsSignedType(type);
        }

        public static bool NumIsNegativeOK(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.IsNegativeOK ?? false;
        }

        public static bool NumIs16bit(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.Is16Bit ?? false;
        }

        public static bool NumIs32bit(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.Is32Bit ?? false;
        }

        public static bool NumIs64bit(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.Is64Bit ?? false;
        }

        // ============================================================================
        // NumGetInstance - Get a sample instance of a numeric type
        // ============================================================================

        /// <summary>
        /// Get a sample instance of a numeric type from a Type parameter.
        /// </summary>
        /// <typeparam name="T">Must be a Type (for compile-time safety)</typeparam>
        /// <returns>Sample instance of the numeric type</returns>
        public static dynamic NumGetInstance<T>() where T : Type
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo?.InstanceOf() ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get a sample instance of a numeric type from a value.
        /// </summary>
        /// <typeparam name="T">Must be a struct (for compile-time safety)</typeparam>
        /// <param name="type">The value to get the type from</param>
        /// <returns>Sample instance of the numeric type</returns>
        public static dynamic NumGetInstance<T>(T type) where T : struct
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo?.InstanceOf() ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get a sample instance of a numeric type from a Type object.
        /// </summary>
        /// <param name="type">The Type to get an instance of</param>
        /// <returns>Sample instance of the numeric type</returns>
        public static dynamic NumGetInstance(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.InstanceOf() ?? throw new InvalidOperationException($"Unknown numeric type: {type}");
        }

        // ============================================================================
        // NumGetFullMetaData - Get the complete metadata object
        // ============================================================================

        /// <summary>
        /// Get the full metadata object for a numeric type from a Type parameter.
        /// </summary>
        /// <typeparam name="T">Must be a Type (for compile-time safety)</typeparam>
        /// <returns>Complete metadata object with all properties</returns>
        public static dynamic NumGetFullMetaData<T>() where T : Type
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get the full metadata object for a numeric type from a value.
        /// </summary>
        /// <typeparam name="T">Must be a struct (for compile-time safety)</typeparam>
        /// <param name="type">The value to get the type from</param>
        /// <returns>Complete metadata object with all properties</returns>
        public static dynamic NumGetFullMetaData<T>(T type) where T : struct
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get the full metadata object for a numeric type from a Type object.
        /// </summary>
        /// <param name="type">The Type to get metadata for</param>
        /// <returns>Complete metadata object with all properties</returns>
        public static dynamic NumGetFullMetaData(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo ?? throw new InvalidOperationException($"Unknown numeric type: {type}");
        }

        // ============================================================================
        // NumGetFloor - Get minimum value
        // ============================================================================

        /// <summary>
        /// Get the minimum value for a numeric type from a Type parameter.
        /// </summary>
        /// <typeparam name="T">Must be a numeric type</typeparam>
        /// <returns>Minimum value for the type</returns>
        public static T NumGetFloor<T>() where T : struct
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo?.MinValue ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get the minimum value for a numeric type from a value.
        /// </summary>
        /// <typeparam name="T">Must be a struct (for compile-time safety)</typeparam>
        /// <param name="type">The value to get the type from</param>
        /// <returns>Minimum value for the type</returns>
        public static T NumGetFloor<T>(T type) where T : struct
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo?.MinValue ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get the minimum value for a numeric type from a Type object.
        /// </summary>
        /// <param name="type">The Type to get the minimum value for</param>
        /// <returns>Minimum value for the type</returns>
        public static dynamic NumGetFloor(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.MinValue ?? throw new InvalidOperationException($"Unknown numeric type: {type}");
        }

        // ============================================================================
        // NumGetCeiling - Get maximum value
        // ============================================================================

        /// <summary>
        /// Get the maximum value for a numeric type from a Type parameter.
        /// </summary>
        /// <typeparam name="T">Must be a numeric type</typeparam>
        /// <returns>Maximum value for the type</returns>
        public static T NumGetCeiling<T>() where T : struct
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo?.MaxValue ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get the maximum value for a numeric type from a value.
        /// </summary>
        /// <typeparam name="T">Must be a struct (for compile-time safety)</typeparam>
        /// <param name="type">The value to get the type from</param>
        /// <returns>Maximum value for the type</returns>
        public static T NumGetCeiling<T>(T type) where T : struct
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo?.MaxValue ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get the maximum value for a numeric type from a Type object.
        /// </summary>
        /// <param name="type">The Type to get the maximum value for</param>
        /// <returns>Maximum value for the type</returns>
        public static dynamic NumGetCeiling(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.MaxValue ?? throw new InvalidOperationException($"Unknown numeric type: {type}");
        }

        // ============================================================================
        // NumNameAsString - Get type name as string
        // ============================================================================

        /// <summary>
        /// Get the name of a numeric type as a string from a Type parameter.
        /// </summary>
        /// <typeparam name="T">Must be a Type (for compile-time safety)</typeparam>
        /// <returns>String name of the type</returns>
        public static string NumNameAsString<T>() where T : Type
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo?.Name ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get the name of a numeric type as a string from a value.
        /// </summary>
        /// <typeparam name="T">Must be a struct (for compile-time safety)</typeparam>
        /// <param name="type">The value to get the type name from</param>
        /// <returns>String name of the type</returns>
        public static string NumNameAsString<T>(T type) where T : struct
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(typeof(T));
            return typeInfo?.Name ?? throw new InvalidOperationException($"Unknown numeric type: {typeof(T)}");
        }

        /// <summary>
        /// Get the name of a numeric type as a string from a Type object.
        /// </summary>
        /// <param name="type">The Type to get the name of</param>
        /// <returns>String name of the type</returns>
        public static string NumNameAsString(Type type)
        {
            dynamic? typeInfo = Statics.FuckinNumbers.Types.GetTypeInfo(type);
            return typeInfo?.Name ?? throw new InvalidOperationException($"Unknown numeric type: {type}");
        }

        public static Type GetArchSignedIntType()
        {
            return (IntPtr.Size >= 8) ? typeof(Int64) : typeof(Int32);
        }

        public static Type GetArchUnsignedIntType()
        {
            return (IntPtr.Size >= 8) ? typeof(UInt64) : typeof(UInt32);
        }

        public static bool IsNumericType<T>() where T : Type
        {
            return Statics.FuckinNumbers.Types.AllTypesArray.Contains(typeof(T));
        }

        public static bool IsNumericType<T>(T type) where T : struct
        {
            return Statics.FuckinNumbers.Types.AllTypesArray.Contains(type.GetType());
        }

        public static bool IsNumericType(dynamic type)
        {
            Type incomingType = type.GetType();
            return Statics.FuckinNumbers.Types.AllTypesArray.Contains(incomingType);
        }

        public static bool AddToGlobalContext<T>(string key, object value) where T : class
        {
            T contextProperty = (T)Activator.CreateInstance(typeof(T), value);
            //T contextProperty = (T)value;

            try
            {
                GlobalLogContext.PushProperty(key, contextProperty);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected internal static bool ExistsInPath(string? fileName = null)
        {
            return GetFullPath(fileName) != null;
        }

        protected internal static bool ExistsInPath(string? fileName, out string path)
        {
            if (fileName.IsNullOrWhiteSpace())
            {
                path = String.Empty;
                return false;
            }

            path = GetFullPath(fileName) ?? String.Empty;
            return !path.IsNullOrWhiteSpace();
        }

        protected internal static string? GetFullPath(string? fileName = null)
        {
            if (fileName.IsNullOrEmpty())
            {
                return null;
            }

            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }

            string values = Environment.GetEnvironmentVariable("PATH") ?? String.Empty;

            foreach (string path in values.Split(Path.PathSeparator))
            {
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }

        public static void StartExternalConsult(Dictionary<string, dynamic> patientInfo)
        {
            ProcessHelper.RelaunchAsAdmin(patientInfo);
            return;
        }
    }
}
