using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using System.Diagnostics.CodeAnalysis;
using RosettaTools.Pwsh.Text.RevenantLogger.Common;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;
using System.Reflection;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "RevenantLoggerConfig")]
    [Alias("Show-RevenantLoggerConfig")]
    [OutputType(typeof(string))]
    public class CmdGetRevenantLoggerConfig : RevenantLoggerPSCmdlet
    {
        private string? _configOnDisk;

        public new ILogger? CmdletLogger { get => _cmdletLogger; }

        public CmdGetRevenantLoggerConfig()
        {

        }
        protected override void BeginProcessing()
        {
            base.init();
            base.BeginProcessing();

            InitDIContainer<CmdGetRevenantLoggerConfig>();

            _configOnDisk ??= RevenantConfig?.DefaultConfigFile ?? (new Configuration()).DefaultConfigFile;
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            if (String.IsNullOrWhiteSpace(_configOnDisk))
            {
                CmdletLogger?.RLogError("{error}: Configuration file path is empty", FailMessage.Value.ToString());
                return;
            }

            try
            {
                if ((null == RevenantConfig?.DefaultConfigFile) || (File.Exists(RevenantConfig?.DefaultConfigFile) == false))
                {
                    CmdletLogger?.RLogError("{error}: Config file path is null, or config file at \"{Config.DefaultConfigFile}\" does not exist or is inaccessible.", FailMessage.Value.ToString(), RevenantConfig?.DefaultConfig);
                    throw new ArgumentNullException(nameof(RevenantConfig.DefaultConfigFile), "Configuration file path is null");
                }
                _configOnDisk = File.ReadAllText(RevenantConfig.DefaultConfigFile);
            }
            catch (Exception ex)
            {
                CmdletLogger?.RLogError("{error}: Error reading configuration file at {Config.DefaultConfigFile}", FailMessage.Value.ToString(), RevenantConfig?.DefaultConfig);
                var errorTable = new Table();
                errorTable.Border = TableBorder.HeavyEdge;
                errorTable.AddColumn("[red]Error Message[/]").Centered();
                errorTable.AddColumn(new TableColumn("[lightsalmon3_1]Stack Trace[/]")).Centered();

                errorTable.AddRow(ex.Message ?? "No error message", ex.StackTrace ?? "No stack trace")
                    .Border(TableBorder.Ascii)
                    .Centered();

                AnsiConsole.Write(errorTable);
                WriteObject(ex);
            }

            JsonText configText = new(_configOnDisk);
            configText.BracesStyle(new Style(foreground: Color.Fuchsia, null, Decoration.Bold))
                .BracketColor(Color.Chartreuse1)
                .ColonStyle(new Style(foreground: Color.Red, null, Decoration.Bold | Decoration.Dim))
                .CommaStyle(new Style(foreground: Color.White, null, Decoration.Bold | Decoration.Italic))
                .MemberStyle(new Style(foreground: Color.CornflowerBlue, null))
                .NullStyle(new Style(foreground: Color.Grey46, background: Color.Red, Decoration.Strikethrough | Decoration.SlowBlink))
#if NET8_0_OR_GREATER
                .StringColor(Color.FromHex("CE9178"))
#else
                .StringColor(Color.Salmon1)
#endif
                .BooleanStyle(new Style(foreground: Color.Red, null, Decoration.Bold | Decoration.Underline))
                .NumberStyle(new Style(Color.Green1, default, Decoration.Bold));

            AnsiConsole.Write(
                new Rule()
                    .Centered()
#if NET8_0_OR_GREATER
                    .RuleStyle(Color.DarkGoldenrod)
#else
                    .RuleStyle(new Style(Color.DarkGoldenrod))
#endif
            );

            AnsiConsole.Write(
                new Panel(configText)
                .Header("Revenant Logger Configuration")
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
                new Rule($"Configuration file at: [green]{RevenantConfig.DefaultConfigFile}[/]")
                    .Centered()
#if NET8_0_OR_GREATER
                    .RuleStyle(Color.DarkGoldenrod)
#else
                    .RuleStyle(new Style(Color.DarkGoldenrod))
#endif
            );
        }
    }
}
