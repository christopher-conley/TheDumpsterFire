using Chainwax;
using static Chainwax.CLI.Bootstrap;
using Chainwax.CLI.Helpers;
using Chainwax.CLI.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Chainwax.CLI.ChainwaxSpectre {

    public class BaseInterceptor : ICommandInterceptor {

        private readonly ILogger<BaseInterceptor> _logger;

        public ILogger<BaseInterceptor> Logger
        {
            get => _logger;
        }
        public BaseInterceptor(ILogger<BaseInterceptor> logger) {

            _logger = logger;
            Logger.LogInformation("{success}: BaseInterceptor initialized", SuccessMessage.Value);
            ;
            ;
        }

        //public BaseInterceptor(ILogger<IBareLogger> logger) {
        //    SymlinkHelper.Initialize(logger);
        //    CBase.BareLogger = logger;

        //    logger.LogInformation("{success} BaseInterceptor initialized", SuccessMessage.Value);
        //    ;
        //    ;
        //}

        public void Intercept(CommandContext context, CommandSettings settings) {
            AnsiConsole.WriteLine();
        }

        public void InterceptResult(CommandContext context, CommandSettings settings, ref int result) {
            AnsiConsole.WriteLine();
        }
    }
}
