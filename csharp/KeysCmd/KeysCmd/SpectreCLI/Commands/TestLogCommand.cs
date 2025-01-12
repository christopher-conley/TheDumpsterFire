using RosettaTools.CLI.KeysCmd.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using static RosettaTools.CLI.KeysCmd.Common.Utilities;

// This file exists just because it's an easy template for adding
// commands in the future.

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{
    public class TestLogCommand : Command<TestLogSettings>
    {

        private readonly ILogger<TestLogCommand> _logger;

        public ILogger<TestLogCommand> Logger
        {
            get => _logger;
        }
        public TestLogCommand(ILogger<TestLogCommand> logger)
        {
            _logger = logger;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] TestLogSettings settings)
        {

            ;

            Logger.CLogInformation("TestLogCommand.Execute() called.", FormatCaller());
            Logger.CLogInformation("Message is: {settings.Message}", FormatCaller(), settings.Message);

            return 0;
        }
    }
}
