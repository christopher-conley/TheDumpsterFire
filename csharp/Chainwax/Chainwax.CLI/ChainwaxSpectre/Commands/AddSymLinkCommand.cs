using Chainwax.CLI.ChainwaxSpectre;
using Chainwax.CLI.Helpers;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.ChainwaxSpectre {
    public class AddSymLinkCommand : Command<AddLinkSettings> {

        private readonly ILogger<AddSymLinkCommand> _logger;

        public ILogger<AddSymLinkCommand> Logger
        {
            get => _logger;
        }

        public AddSymLinkCommand(ILogger<AddSymLinkCommand> logger) {
            _logger = logger;
        }
        public override int Execute([NotNull] CommandContext context, [NotNull] AddLinkSettings settings) {

            string itemType;
            Dictionary<string, string> ItemTypes = [];
            if (File.Exists(settings.Target)) {
                itemType = "file";
                ItemTypes.Add(settings.Target, "file");
            }
            else if (Directory.Exists(settings.Target)) {
                itemType = "directory";
                ItemTypes.Add(settings.Target, "directory");
            }
            else {
                Logger.LogCritical("Encountered unsupported item type at [red]{settings.Target}[/], exiting.", settings.Target);
                return 1;
            }

            string? itemName = Path.GetFileName(settings.Target);
            if (itemType == "file") {
                Logger.LogInformation("Creating [green](file)[/] symlink for [green]{itemName}[/] at: [blue]{settings.LinkName}[/]", itemName, settings.LinkName);
            }
            else {
                Logger.LogInformation("Creating [blue](directory)[/] symlink for [green]{itemName}[/] at: [blue]{settings.LinkName}[/]", itemName, settings.LinkName);
            }

            SymlinkHelper.CreateSymlink(ItemTypes, 1, settings.LinkName);

            return SymlinkHelper.Result;
        }
    }
}
