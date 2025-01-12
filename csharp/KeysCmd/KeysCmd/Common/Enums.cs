namespace RosettaTools.CLI.KeysCmd.Common
{

    /// <summary>
    /// These directly map to their corresponding log levels in the <see cref="Microsoft.Extensions.Logging"/> namespace.
    /// <br>They're just shortened versions of the same name for the file logger.</br>
    /// </summary>
    public enum ShortLogLevel
    {
        trace = 0,
        debug = 1,
        info = 2,
        warn = 3,
        error = 4,
        crit = 5,
        none = 6,
    }
}
