using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.ChainwaxSpectre {
    public class TestLogSettings : CommandSettings {

        [Description("Message to test logging with.")]
        [CommandOption("-m|--message")]
        public string? Message { get; init; }

        //[CommandOption("-c|--caller")]
        //public string? Caller { get; init; }

    }
}
