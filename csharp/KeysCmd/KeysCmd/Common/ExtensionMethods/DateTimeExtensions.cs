namespace RosettaTools.CLI.KeysCmd.Common.ExtensionMethods
{

    /// <summary>
    /// Class containing extension methods for the <see cref="DateTime"/> class.
    /// <br></br>
    /// <br>Contains the following methods:</br>
    /// <list type="bullet">
    /// <item><see cref="ToISO8601" /></item>
    /// <item><see cref="ToISO8601WithMs" /></item>
    /// <item><see cref="ToISO8601WithTZ" /></item>
    /// <item><see cref="ToISO8601TZWithTZMs" /></item>
    /// <item><see cref="ToRFC1123" /></item>
    /// <item><see cref="ToSortable" /></item>
    /// <item><see cref="ToUniversalSortable" /></item>
    /// </list>
    /// </summary>

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convenience method to convert a <see cref="DateTime"/> object to a <see cref="string"/> in ISO8601 format.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> object to format.</param>
        /// <returns><see cref="string"/></returns>
        public static string ToISO8601(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        /// <summary>
        /// Convenience method to convert a <see cref="DateTime"/> object to a <see cref="string"/> in ISO8601 format, with milliseconds.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> object to format.</param>
        /// <returns><see cref="string"/></returns>
        public static string ToISO8601WithMs(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }

        /// <summary>
        /// Convenience method to convert a <see cref="DateTime"/> object to a <see cref="string"/> in ISO8601 format, with time zone.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> object to format.</param>
        /// <returns><see cref="string"/></returns>
        public static string ToISO8601WithTZ(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }

        /// <summary>
        /// Convenience method to convert a <see cref="DateTime"/> object to a <see cref="string"/> in ISO8601 format, with time zone and milliseconds.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> object to format.</param>
        /// <returns><see cref="string"/></returns>
        public static string ToISO8601TZWithTZMs(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
        }

        /// <summary>
        /// Convenience method to convert a <see cref="DateTime"/> object to a <see cref="string"/> in RFC1123 format.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> object to format.</param>
        /// <returns><see cref="string"/></returns>
        public static string ToRFC1123(this DateTime dt)
        {
            return dt.ToString("R");
        }

        /// <summary>
        /// Convenience method to convert a <see cref="DateTime"/> object to a <see cref="string"/> in a sortable format.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> object to format.</param>
        /// <returns><see cref="string"/></returns>
        public static string ToSortable(this DateTime dt)
        {
            return dt.ToString("s");
        }

        /// <summary>
        /// Convenience method to convert a <see cref="DateTime"/> object to a <see cref="string"/> in a universal sortable format.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> object to format.</param>
        /// <returns><see cref="string"/></returns>
        public static string ToUniversalSortable(this DateTime dt)
        {
            return dt.ToString("u");
        }
    }
}
