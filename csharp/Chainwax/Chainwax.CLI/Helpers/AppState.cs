using Chainwax.CLI.Interfaces;
using Chainwax.CLI.Services;
using Chainwax.CLI.ChainwaxSpectre;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Chainwax.CLI.Helpers {


    public partial class AppState : IAppState {

        private const int _SW_HIDE = 0;
        private const int _SW_SHOW = 5;

        private Hashtable _environmentVariables;
        private ConfigDefinition.ConfigRoot _runningConfig;
        private IAppTruthTable _truthTable;
        private IntPtr _hWndConsole;
        //private SymbolIcon _errorSymbol;
        private Hashtable _versionInfo;
        private string _shutdownReason;
        private IBareLogger _logWrapper;
        private readonly ILogger<AppState> _logger;
        //private IConfigHelper _configHelper;

        public static int SW_HIDE
        {
            get => _SW_HIDE;
        }

        public static int SW_SHOW
        {
            get => _SW_SHOW;
        }

        public Hashtable EnvironmentVariables
        {
            get => _environmentVariables;
            set => _environmentVariables = value;
        }

        public ConfigDefinition.ConfigRoot RunningConfig
        {
            get; private set;
            //get => TypeResolver.GetService<IConfigHelper>().RunningConfig;
            //get => App.GetService<IConfigHelper>().RunningConfig.Config.App;

            //get => _configHelper.RunningConfig.Config.App;
            //set => _configHelper.RunningConfig.Config.App = value;
            //set => App.GetService<IConfigHelper>().RunningConfig.Config.App = value;
        }

        public IAppTruthTable TruthTable
        {
            get => _truthTable;
            set => _truthTable = value;
        }

        public IntPtr HWndConsole
        {
            get {
                if (null == _hWndConsole || _hWndConsole == IntPtr.Zero) {
                    //_hWndConsole = ConsoleHelper.GetConsoleHWnd();
                    return _hWndConsole;
                }
                else {
                    return _hWndConsole;
                }
            }
            set => _hWndConsole = value;
        }

        //public SymbolIcon ErrorSymbol
        //{
        //    get => _errorSymbol;
        //    set => _errorSymbol = value;
        //}

        public Hashtable VersionInfo
        {
            get => _versionInfo;
        }

        public string ShutdownReason
        {
            get => _shutdownReason;
            set => _shutdownReason = value;
        }

        public IBareLogger LogWrapper
        {
            get => _logWrapper;
            set => _logWrapper = value;
        }

        public AppState(ILogger<AppState> logger) {
            _logger = logger;
        }


        public AppState(ILogger<AppState> logger, IAppTruthTable appTruthTable) {
            _logger = logger;
            _environmentVariables = (Hashtable)Environment.GetEnvironmentVariables();
            _truthTable = appTruthTable;
        }

        public void SetDefaultVars() {
            //ErrorSymbol = new SymbolIcon { Symbol = SymbolRegular.ErrorCircle24 };
            _versionInfo = Utilities.GetApplicationVersionInfo();
            //_doConsoleToggle += EventHandlers.OS_VisibilityChangeRequested;
            //ShowConsole = ShowConsole;
            //_bootstrapping = false;

            //StringBuilder sb = new StringBuilder();
            //foreach (DictionaryEntry entry in _environmentVariables) {
            //    sb.AppendLine($"{entry.Key}: {entry.Value}");
            //}
            //Clipboard.SetText(sb.ToString());
            ;
        }

        public void RefreshEnvironmentVariables() {
            EnvironmentVariables = (Hashtable)Environment.GetEnvironmentVariables();
        }
    }

        public partial class AppTruthTable(ILogger<AppTruthTable> logger) : IAppTruthTable {
        private bool _isConsoleVisible;
        private bool _isConsoleAlloced = true;
        private bool _isFirstRun = false;
        private const string _fallbackAppTitle = "Chainwax";

        private readonly ILogger<AppTruthTable> _logger;
        //private IConfigHelper _configHelper;
        //private IAppState _appState = appState;

        public ILogger<AppTruthTable> Logger
        {
            get => _logger;
        }
        public string FallbackAppTitle
        {
            get => _fallbackAppTitle;
        }
        public bool IsConsoleVisible
        {
            get => _isConsoleVisible;
            set => _isConsoleVisible = value;
        }

        public bool IsConsoleAlloced
        {
            get => _isConsoleAlloced;
            set => _isConsoleAlloced = value;
        }

        public bool IsFirstRun
        {
            get => _isFirstRun;
            set => _isFirstRun = value;
        }
    }
}
