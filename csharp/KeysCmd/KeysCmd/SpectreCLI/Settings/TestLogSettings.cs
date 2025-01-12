using Spectre.Console.Cli;
using System.ComponentModel;

// This file exists just because it's an easy template for adding
// command settings in the future.

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{
    public class TestLogSettings : CommandSettings
    {

        [Description("Message to test logging with.")]
        [CommandOption("-m|--message")]
        public string? Message { get; init; }

    }
}
