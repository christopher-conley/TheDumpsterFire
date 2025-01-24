using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using RosettaTools.Pwsh.Text.RevenantLogger.Common;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;
using System.Reflection;
using System.Diagnostics;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Cmdlets
{
    [Cmdlet(VerbsData.Edit, "RevenantLoggerConfig")]
    [Alias("Update-RevenantLoggerConfig")]
    [OutputType(typeof(void))]
    public class CmdEditRevenantLoggerConfig : RevenantLoggerPSCmdlet
    {
        private string? _configOnDisk;
        private string? _textEditor;

        public new ILogger? CmdletLogger { get => _cmdletLogger; }

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("TextEditor")]
        [AllowNull()]
        //[ValidateTypes(typeof(string))]

        public string? Editor
        {
            get => _textEditor;
            set => _textEditor = value;
        }

        public CmdEditRevenantLoggerConfig()
        {

        }
        protected override void BeginProcessing()
        {
            base.init();
            base.BeginProcessing();

            InitDIContainer<CmdEditRevenantLoggerConfig>();

            _configOnDisk ??= RevenantConfig?.DefaultConfigFile ?? (new Configuration()).DefaultConfigFile;
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            string nullDevice = (RevenantConfig.IsWindows) ? ">NUL" : String.Empty;
            string? preferredEditor = GetFullPath(Editor) ?? Environment.GetEnvironmentVariable("EDITOR") ?? string.Empty;

            if ((String.IsNullOrWhiteSpace(Editor) && String.IsNullOrWhiteSpace(preferredEditor)))
            {

                if (
                    ExistsInPath("vscodium.exe") ||
                    ExistsInPath("codium.exe") ||
                    ExistsInPath("code.exe") ||
                    ((RevenantConfig.IsLinux) && ExistsInPath("vscodium")) ||
                    ((RevenantConfig.IsLinux) && ExistsInPath("codium")) ||
                    ((RevenantConfig.IsLinux) && ExistsInPath("code"))
                    )
                {
                    preferredEditor =
                        GetFullPath("vscodium.exe") ??
                        GetFullPath("codium.exe") ??
                        GetFullPath("code.exe") ??
                        GetFullPath("vscodium") ??
                        GetFullPath("codium") ??
                        GetFullPath("code");
                }
                else
                {
                    if ((RevenantConfig.IsWindows) && (preferredEditor == string.Empty))
                    {
                        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        string localAppDataCode = Path.Combine(localAppData, "Programs", "Microsoft VS Code", "Code.exe");
                        string localAppDataCodium = Path.Combine(localAppData, "Programs", "VSCodium", "VSCodium.exe");

                        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        string appDataCode = Path.Combine(appData, "Programs", "Microsoft VS Code", "Code.exe");
                        string appDataCodium = Path.Combine(appData, "Programs", "VSCodium", "VSCodium.exe");

                        if (
                            ExistsInPath(localAppDataCodium) ||
                            ExistsInPath(appDataCodium) ||
                            ExistsInPath(localAppDataCodium) ||
                            ExistsInPath(localAppDataCode)
                            )
                        {
                            preferredEditor = localAppDataCodium ?? appDataCodium ?? localAppDataCodium ?? localAppDataCode;
                        }
                        else
                        {
                            preferredEditor = GetFullPath("notepad.exe") ?? GetFullPath("notepad");
                        }
                    }
                    else if ((RevenantConfig.IsLinux) && (preferredEditor == string.Empty))
                    {
                        if (
                            ExistsInPath("vim") ||
                            ExistsInPath("vi") ||
                            ExistsInPath("nano") ||
                            ExistsInPath("pico") ||
                            ExistsInPath("emacs")
                            )
                        {
                            preferredEditor =
                                GetFullPath("vim") ??
                                GetFullPath("vi") ??
                                GetFullPath("nano") ??
                                GetFullPath("pico") ??
                                GetFullPath("emacs");
                        }
                        else
                        {
                            // Bruh

                            preferredEditor = GetFullPath("ed");
                        }
                    }
                    else
                    {
                        // We should never get here, but let's default to notepad anyway
                        preferredEditor = GetFullPath("notepad.exe");
                    }
                }
            }

            Process proc = new() {
                StartInfo = new ProcessStartInfo {
                    FileName = preferredEditor,
                    Arguments = $"{RevenantConfig?.DefaultConfigFile} {nullDevice}",
                    UseShellExecute = (RevenantConfig.IsWindows == false),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = (RevenantConfig.IsLinux),
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExitAsync();
            proc.Dispose();

            AnsiConsole.WriteLine();
            WriteObject(null);
        }
    }
}

