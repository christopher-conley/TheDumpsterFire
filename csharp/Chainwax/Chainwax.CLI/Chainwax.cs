using Chainwax.CLI.ChainwaxSpectre;
using Chainwax.CLI.Helpers;
using Chainwax.CLI.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax {
    public class ChainwaxBase {

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        protected internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", EntryPoint = "GetConsoleWindow", SetLastError = true)]
        protected internal static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected internal static extern bool IsWindowVisible(IntPtr hWnd);

        protected internal nint? _consoleHWND;
        protected internal bool? _isConsoleVisible;
        protected internal ILogger<IBareLogger> _logger;
        protected internal IConfigHelper _configHelper;
        protected internal IAppTruthTable _truthTable;
        protected internal IAppState _appState;
        protected internal IHostBuilder _genericHost;
        protected internal TypeRegistrar _appTypeRegistrar;
        protected internal CommandApp _spectreCommandApp;

        public nint ConsoleHWND
        {
            get {
                if (null == _consoleHWND || _consoleHWND == 0) {
                    _consoleHWND = GetConsoleWindow();
                    return (nint)_consoleHWND;
                }
                else {
                    return (nint)_consoleHWND;
                }
            }

            set {
                _consoleHWND = value;
            }
        }

        public bool? IsConsoleVisible
        {
            get {
                if (!_isConsoleVisible.HasValue) {
                    _isConsoleVisible = IsWindowVisible(ConsoleHWND);
                    return _isConsoleVisible;
                }
                else {
                    return _isConsoleVisible;

                }
            }
        }

        protected internal IHostBuilder GenericHost {
            get => _genericHost;
            set => _genericHost = value;
        }

        protected internal ILogger<IBareLogger> Logger
        {
            get => _logger;
            set => _logger = value;
        }

        protected internal TypeRegistrar AppTypeRegistrar
        {
            get => _appTypeRegistrar;
            set => _appTypeRegistrar = value;
        }

        protected internal CommandApp SpectreCommandApp
        {
            get => _spectreCommandApp;
            set => _spectreCommandApp = value;
        }

    }
}
