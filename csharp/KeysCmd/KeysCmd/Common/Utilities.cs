using System.Runtime.CompilerServices;

namespace RosettaTools.CLI.KeysCmd.Common
{

    /// <summary>
    /// General utility catchall/garbage can for methods and other shit that don't fit anywhere else.
    /// <br></br>
    /// <br>Contains the following methods:</br>
    /// <list type="bullet">
    /// <item><see cref="FormatCaller" /></item>
    /// </list>
    /// </summary>

    internal static class Utilities
    {
        /// <summary>
        /// <see langword="internal static readonly char" /> class variable holding the value of
        /// <br>the ASCII NULL character. Written to stdout as an indicator to OpenSSH that the</br>
        /// <br>key listing is finished.</br>
        /// </summary>

        internal static readonly char NULLCHAR = char.MinValue;

        /// <summary>
        /// A method that returns a <see cref="Spectre.Console"/>-formatted <see cref="string"/> containing the name of the calling method.
        /// </summary>
        /// <param name="caller">A <see langword="string"/> which contains the name of the calling method.
        /// <br>This parameter is automatically populated by the compiler, there shouldn't ever be a need to manually specify it.</br></param>
        /// <returns><see cref="string"/></returns>

        public static string FormatCaller([CallerMemberName] string caller = "Unknown")
        {
            return $"[bold yellow]{caller}[/]";
        }
    }
}
