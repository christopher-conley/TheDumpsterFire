using Chainwax.CLI.Interfaces;
using Chainwax.CLI.ChainwaxSpectre;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.ChainwaxSpectre.Commands {
    public sealed class DefaultCommand : Command<DefaultCommand.Settings> {
        private readonly IGreeter _greeter;

        public sealed class Settings : CommandSettings {
            [CommandOption("-n|--name <NAME>")]
            [Description("The person or thing to greet.")]
            [DefaultValue("World")]
            public string Name { get; set; }
        }

        public DefaultCommand(IGreeter greeter) {
            _greeter = greeter ?? throw new ArgumentNullException(nameof(greeter));
        }

        public override int Execute(CommandContext context, Settings settings) {
            _greeter.Greet(settings.Name);
            return 0;
        }
    }
}
