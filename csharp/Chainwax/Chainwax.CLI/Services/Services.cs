using Chainwax.CLI.Interfaces;
using Chainwax.CLI.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Services {
    public class Services {
        protected internal static IBareLogger _logger;
        protected internal static IConfigHelper _configHelper;
        protected internal static IAppTruthTable _truthTable;
        protected internal static IAppState _appState;

        public static IBareLogger Logger
        {
            get; private set;
        }
        public static IConfigHelper ConfigHelper
        {
            get; private set;
        }
        public static IAppTruthTable TruthTable
        {
            get; private set;
        }
        public static IAppState AppState
        {
            get; private set;
        }

        //public Services(ILogger<Logger> logger) {
        //    _logger = new Logger(logger);
        //    _truthTable = new AppTruthTable(_logger);
        //    _configHelper = new ConfigHelper(_truthTable, logger);
        //    _appState = new AppState(_logger, _truthTable);
        //}
    }
}
