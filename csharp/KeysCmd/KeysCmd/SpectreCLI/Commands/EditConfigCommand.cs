using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using RosettaTools.CLI.KeysCmd.Interfaces;
using System.Diagnostics;

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{
    public class EditConfigCommand : Command<ConfigSettings>
    {

        private readonly ILogger<EditConfigCommand> _logger;
        private IKeysCmdConfiguration _config;

        public ILogger<EditConfigCommand> Logger => _logger;

        public IKeysCmdConfiguration Config
        {
            get => _config;
            protected internal set => _config = value;
        }
        public EditConfigCommand(ILogger<EditConfigCommand> logger, IKeysCmdConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] ConfigSettings settings)
        {
            EditConfig(Config);
            return 0;
        }


        public static void EditConfig(IKeysCmdConfiguration Config, string? textEditor = null)
        {
            string preferredEditor = string.Empty;

            if (null == textEditor)
            {
                preferredEditor = Environment.GetEnvironmentVariable("EDITOR") ?? string.Empty;

                if (Config.IsWindows && preferredEditor == string.Empty)
                {
                    preferredEditor = "notepad";
                }
                else if (Config.IsLinux && preferredEditor == string.Empty)
                {
                    preferredEditor = "vim";
                }
                else
                {
                    // We should never get here, but let's default to notepad anyway
                    preferredEditor = "notepad";
                }
            }

            Process proc = new();

            proc.StartInfo = new ProcessStartInfo
            {
                FileName = preferredEditor,
                Arguments = Config.DefaultConfigFile,
                UseShellExecute = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                CreateNoWindow = true
            };

            proc.Start();
            proc.WaitForExit();
            proc.Dispose();

        }
    }
}
