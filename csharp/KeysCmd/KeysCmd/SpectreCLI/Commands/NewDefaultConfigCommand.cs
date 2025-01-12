using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using RosettaTools.CLI.KeysCmd.Interfaces;
using System.Diagnostics;
using Spectre.Console;
using RosettaTools.CLI.KeysCmd.Common.ExtensionMethods;
using static RosettaTools.CLI.KeysCmd.Common.Utilities;

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{
    public class NewDefaultConfigCommand : Command<ConfigSettings>
    {

        private readonly ILogger<NewDefaultConfigCommand> _logger;
        private IKeysCmdConfiguration _config;

        public ILogger<NewDefaultConfigCommand> Logger => _logger;

        public IKeysCmdConfiguration Config
        {
            get => _config;
            protected internal set => _config = value;
        }
        public NewDefaultConfigCommand(ILogger<NewDefaultConfigCommand> logger, IKeysCmdConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] ConfigSettings settings)
        {
            Bootstrap.ShouldLogToConsole = true;
            bool configExists = File.Exists(Path.Combine(Config.ConfigHome, Config.DefaultConfigFilename));

            if (configExists)
            {
                string userPrompt = "The default configuration file already exists. Overwriting it will erase any changes made to it. Do you want to continue?";
                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>(userPrompt)
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(false)
                        .DefaultValueStyle(new Style(foreground: Color.Red))
                        .WithConverter(choice => choice ? "yes" : "no")
                        .WithConverter(choice => choice ? "y" : "n")
                        .InvalidChoiceMessage("Please select a valid option."));

                if (!confirmation)
                {
                    Logger?.CLogInformation("User chose not to overwrite the default configuration file. Exiting.", FormatCaller());
                    return 0;
                }
            }

            Config.SaveConfig(ConfigToSave: Config.GetDefaultConfig(), SavePath: Config.DefaultConfigFile);
            Logger?.CLogInformation("Default configuration file written to: {Config.DefaultConfigFile}?", FormatCaller(), Config.DefaultConfigFile);

            string launchPrompt = $"Launch default editor to edit the configuration file at {Config.DefaultConfigFile}?";
            var launchConfirm = AnsiConsole.Prompt(
                    new TextPrompt<bool>(launchPrompt)
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(true)
                        .DefaultValueStyle(new Style(foreground: Color.Green))
                        .WithConverter(choice => choice ? "yes" : "no")
                        .WithConverter(choice => choice ? "y" : "n")
                        .InvalidChoiceMessage("Please select a valid option."));

            if (launchConfirm)
            {
                Logger?.CLogInformation("Launching default editor to edit the configuration file at: {Config.DefaultConfigFile}", FormatCaller(), Config.DefaultConfigFile);
                EditConfigCommand.EditConfig(Config);
            }

            return 0;
        }
    }
}
