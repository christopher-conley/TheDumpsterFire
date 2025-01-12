using RosettaTools.CLI.KeysCmd.Common.ExtensionMethods;
using RosettaTools.CLI.KeysCmd.Common;
using RosettaTools.CLI.KeysCmd.Logging.StyleTypes;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using System.Diagnostics.CodeAnalysis;
using RosettaTools.CLI.KeysCmd.Interfaces;

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{
    public class ShowConfigCommand : Command<ConfigSettings>
    {

        private readonly ILogger<ShowConfigCommand> _logger;
        private IKeysCmdConfiguration _config;
        private string _configFile;

        public ILogger<ShowConfigCommand> Logger => _logger;

        public IKeysCmdConfiguration Config
        {
            get => _config;
            protected internal set => _config = value;
        }
        public string ConfigFile => _configFile;
        public ShowConfigCommand(ILogger<ShowConfigCommand> logger, IKeysCmdConfiguration config)
        {
            _logger = logger;
            _config = config;
            _configFile = Config.DefaultConfigFile;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] ConfigSettings settings)
        {
            bool originalLogToConsole = Bootstrap.ShouldLogToConsole;
            Bootstrap.ShouldLogToConsole = true;

            try
            {
                _configFile = File.ReadAllText(Config.DefaultConfigFile);
            }
            catch (Exception ex)
            {
                Logger.CLogError("{error}: Error reading configuration file at {Config.DefaultConfigFile}", Utilities.FormatCaller(), ErrorMessage.Value, Config.DefaultConfig);
                var errorTable = new Table();
                errorTable.Border = TableBorder.HeavyEdge;
                errorTable.AddColumn("[red]Error Message[/]").Centered();
                errorTable.AddColumn(new TableColumn("[lightsalmon3_1]Stack Trace[/]")).Centered();

                errorTable.AddRow(ex.Message ?? "No error message", ex.StackTrace ?? "No stack trace")
                    .Border(TableBorder.Ascii)
                    .Centered();

                AnsiConsole.Write(errorTable);
                Bootstrap.ShouldLogToConsole = originalLogToConsole;
                return 1;
            }

            JsonText configText = new(ConfigFile);
            configText.BracesStyle(new Style(foreground: Color.Fuchsia, null, Decoration.Bold))
                .BracketColor(Color.Chartreuse1)
                .ColonStyle(new Style(foreground: Color.Red, null, Decoration.Bold | Decoration.Dim))
                .CommaStyle(new Style(foreground: Color.White, null, Decoration.Bold | Decoration.Italic))
                .MemberStyle(new Style(foreground: Color.CornflowerBlue, null))
                .NullStyle(new Style(foreground: Color.Grey46, background: Color.Red, Decoration.Strikethrough | Decoration.SlowBlink))
                .StringColor(Color.FromHex("CE9178"))
                .BooleanStyle(new Style(foreground: Color.Red, null, Decoration.Bold | Decoration.Underline))
                .NumberStyle(new Style(Color.Green1, default, Decoration.Bold));

            AnsiConsole.Write(
                new Rule()
                    .Centered()
                    .RuleStyle(Color.DarkGoldenrod)
            );

            AnsiConsole.Write(
                new Panel(configText)
                .Header("KeysCmd Configuration")
                .HeaderAlignment(Justify.Right)
                .Collapse()
                .RoundedBorder()
                .BorderColor(Color.DarkGoldenrod)
                .PadRight(1)
                .HeaderAlignment(Justify.Right)
                .Border(BoxBorder.Double)
                .Expand()
            );

            AnsiConsole.Write(
                new Rule($"Configuration file at: [green]{Config.DefaultConfigFile}[/]")
                    .Centered()
                    .RuleStyle(Color.DarkGoldenrod)
            );

            Bootstrap.ShouldLogToConsole = originalLogToConsole;
            return 0;
        }
    }
}
