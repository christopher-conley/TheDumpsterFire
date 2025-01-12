using Chainwax.CLI.ChainwaxSpectre;
using Chainwax.CLI.Helpers;
using Chainwax.CLI.Interfaces;
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
    public class TestLogCommand : Command<TestLogSettings> {

        private readonly ILogger<TestLogCommand> _logger;

        public ILogger<TestLogCommand> Logger
        {
            get => _logger;
        }
        public TestLogCommand(ILogger<TestLogCommand> logger) {
            _logger = logger;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] TestLogSettings settings) {
            ;
            ;
            
            ;

            Logger.LogInformation("TestLogCommand.Execute() called.");
            Logger.LogInformation("Message is: {settings.Message}", settings.Message);

            return 0;
        }
    }
}
