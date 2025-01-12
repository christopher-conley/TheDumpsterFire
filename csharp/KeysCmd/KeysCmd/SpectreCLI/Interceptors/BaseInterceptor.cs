using RosettaTools.CLI.KeysCmd.Common.ExtensionMethods;
using Spectre.Console.Cli;
using Microsoft.Extensions.Logging;
using static RosettaTools.CLI.KeysCmd.Common.Utilities;

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{

    /// <summary>
    /// Base command interceptor that handles common pre and post-command execution tasks.
    /// <br></br>
    /// Primary responsibility is acting as a failsafe for console output behavior based on OpenSSH compatibility mode.
    /// </summary>
    public class BaseInterceptor : ICommandInterceptor
    {

        private readonly ILogger<BaseInterceptor> _logger;

        /// <summary>
        /// Gets the logger instance for this interceptor.
        /// </summary>
        public ILogger<BaseInterceptor> Logger => _logger;

        /// <summary>
        /// Initializes a new instance of the BaseInterceptor class.
        /// <br>This is handled by <see cref="Spectre.Console.Cli"/></br>
        /// </summary>
        /// <param name="logger">Logger for this interceptor instance.</param>
        public BaseInterceptor(ILogger<BaseInterceptor> logger)
        {
            _logger = logger;
            Logger.BeginScope(FormatCaller());
            Logger.CLogDebug("BaseInterceptor initialized", FormatCaller());

        }

        /// <summary>
        /// The safety net to ensure OpenSSH doesn't get fed bad data.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <param name="settings">The command settings.</param>
        public void Intercept(CommandContext context, CommandSettings settings)
        {

            // This shouldn't actually be necessary since it's the first thing that's checked
            // when the program launches, but measure twice, cut once.

            if (context.Arguments.Contains("=o") || context.Arguments.Contains("--openssh"))
            {
                Bootstrap.ShouldLogToConsole = false;
            }
        }

        /// <summary>
        /// Intercepts command result before returning to caller.
        /// Currently not implemented.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <param name="settings">The command settings.</param>
        /// <param name="result">The command result code.</param>
        public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
        {

        }
    }
}
