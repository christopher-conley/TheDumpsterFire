using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.ChainwaxSpectre {
    public class AddHardLinkCommand : Command<AddLinkSettings> {

        public override int Execute([NotNull] CommandContext context, [NotNull] AddLinkSettings settings) {
            return 0;
        }
    }
}