namespace RosettaTools.CLI.KeysCmd.Common.ExtensionMethods
{
    /// <summary>
    /// All extension methods in this class except <see cref="ToFormattableStringArray(string)"/> are currently unused. They are here for
    /// <br>potential use in the future.</br>
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// A method that takes ingests a <see langword="string"/> and returns a <see langword="string"/>[] array.
        /// <br>The input <see langword="string"/> <c>str</c> is split into individual strings using the "Space" character as the delimiter.</br>
        /// <br></br>
        /// <br>This method exists because of the <see cref="Spectre.Console.Cli.IConfigurator.AddExample(string[])"/> method.</br>
        /// <br>The <c>AddExample</c> method takes a <see cref="string"/>[] array as input, not a single <see langword="string"/>.</br>
        /// <br>There is <c><b><u><i>less than zero fucking probability</i></u></b></c> that I'm going to manually maintain that with proper</br>
        /// <br>examples, so I wrote this extension method to save my time and sanity.</br>
        /// </summary>
        /// <param name="str">A <see cref="string"/> which is automatically populated.</param>
        /// <returns><see cref="string"/>[]</returns>

        public static string[] ToFormattableStringArray(this string str)
        {
            return str.Split(' ')
                    .Select(piece => $"{piece}")
                    .ToArray();
        }
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static string ToPascalCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        public static string ToKebabCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
        }

        public static string ToTrainCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x.ToString() : x.ToString())).ToLower();
        }

        public static string ToConstantCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToUpper();
        }

        public static string ToDotCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "." + x.ToString() : x.ToString())).ToLower();
        }

        public static string ToPathCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "/" + x.ToString() : x.ToString())).ToLower();
        }

        public static string ToSentenceCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return char.ToUpper(str[0]) + str.Substring(1) + ".";
        }
    }
}
