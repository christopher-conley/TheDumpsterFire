using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.ChainwaxSpectre {
    public class AddLinkSettings : CommandSettings {

        [Description("The source file or directory to create a link to")]
        [CommandOption("-t|--target")]
        public string? Target { get; init; }

        [Description("The destination directory to create the link in")]
        [CommandOption("-l|--link-name")]
        public string? LinkName { get; init; }

    }
}
