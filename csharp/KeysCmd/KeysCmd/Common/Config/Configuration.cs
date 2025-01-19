using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RosettaTools.CLI.KeysCmd.Interfaces;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace RosettaTools.CLI.KeysCmd.Common
{
    /// <summary>
    /// Class that holds configuration data like platform information, a <see cref="ConfigDefinition"/> object of the default
    /// <br>config file, a <see cref="ConfigDefinition"/> object of the running configuration (merge of config file</br>
    /// <br>on disk + dynamic runtime settings), etc.</br>
    /// <br></br>
    /// <br>Contains the following methods:</br>
    /// <list type="bullet">
    /// <item><see cref="Init" /></item>
    /// <item><see cref="ParseConnectionInfo" /></item>
    /// <item><see cref="LoadConfig" /></item>
    /// <item><see cref="SaveConfig" /></item>
    /// <item><see cref="GetDefaultConfig" /></item>
    /// </list>
    /// </summary>
    public class Configuration : IKeysCmdConfiguration
    {
        private readonly ILogger<Configuration>? logger;
        /// <summary>
        /// A <see langword="string"/>[] of built-in special accounts that are used to determine the default configuration and logging location.
        /// <br>If the program determines that it's running as one of these accounts, config/logs will be saved to <c>C:\ProgramData\keyscmd</c></br>
        /// <br>on Windows, and <c>/etc/keyscmd</c> on Linux.</br>
        /// </summary>
        private static readonly string[] _builtinSpecialAccounts = [
            "nt authority\\system",
            "nt authority\\network service",
            "nt authority\\localservice",
            "nt service\trustedinstaller",
            "root"
            ];

        /// <summary>
        /// Holds a <see cref="string"/> that contains the username of the executing user.
        /// <br>If launched by OpenSSH on Windows to retrieve ssh keys, this will probably be NT AUTHORITY\System</br>
        /// <seealso cref="_builtinSpecialAccounts"/>
        /// </summary>
        private static string? _whoAmI;

        /// <summary>
        /// Holds the date and time that the <see cref="Configuration"/> object was instantiated.
        /// </summary>
        private static DateTime _creationTime;

        /// <summary>
        /// A <see cref="string"/> that contains the path to the directory holding the application JSON configuration file.
        /// <br>The order of preference is:</br>
        /// <list type="number">
        /// <item>A configuration file placed in the same directory as the executable</item>
        /// <item><c>C:\ProgramData\keyscmd</c> on Windows, and <c>/etc/keyscmd</c> on Linux
        /// <br>(Caveat: This is the #1 preference when launched as a user in the <see cref="_builtinSpecialAccounts"/> array.)</br></item>
        /// <item>The directory listed in the <c>XDG_HOME</c> environment variable (Linux or Windows)</item>
        /// <item><c>$ENV:APPDATA\keyscmd</c> on Windows, or <c>~/.config/keyscmd</c> on Linux</item>
        /// </list>
        /// </summary>
        private string _configHome = string.Empty;

        /// <summary>
        /// A <see cref="string"/> containing the filename of the application config. This file will reside in the directory
        /// <b>listed in the <see cref="_configHome"/> variable.</b>
        /// </summary>
        private string _defaultConfigFilename = "keyscmd.config.json";

        /// <summary>
        /// A <see cref="string"/> representation of the full path to the JSON configuration file.
        /// </summary>
        private string _defaultConfigFile;

        /// <summary>
        /// A <see cref="string"/> listing the directory where logs will be stored.<br></br>
        /// <br><c><b>NOTE:</b></c> This path is relative to the <c>KeysCmd</c> executable file, it is not an absolute path.</br>
        /// <br>I will most likely add an option later to allow this to be interpreted as an absolute path, it's just not high</br>
        /// <br>on my priority list right now.</br>
        /// </summary>
        private string _logPath = string.Empty;

        /// <summary>
        /// A strongly-typed <see cref="ConfigDefinition.ConfigRoot"/> representation of the default configuration values.
        /// <br>If the config file does not exist, a new config file is created by serializing this object and writing it out to disk.</br>
        /// </summary>
        private ConfigDefinition.ConfigRoot? _defaultConfig;

        /// <summary>
        /// A strongly-typed <see cref="ConfigDefinition.ConfigRoot"/> representation of the configuration loaded from disk, and any
        /// <br>other runtime modifications that may have been made along the way. This object is deserialized and written to disk</br>
        /// <br>as the JSON configuration file.</br>
        /// </summary>
        private ConfigDefinition.ConfigRoot _runningConfig;

        /// <summary>
        /// A <see cref="bool"/> value that evaluates to <c>true</c> if the host operating system is Windows.
        /// </summary>
        private bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// A <see cref="bool"/> value that evaluates to <c>true</c> if the host operating system is Linux.
        /// </summary>
        private bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// A <see cref="string"/> containing the name of the operating system. Currently unused.
        /// </summary>
        private string _os;

        private string _commonAppDataDir;

        private string _appDataDir;

        /// <summary>
        /// A <see cref="string"/> containing the path to the "CommonApplicationData" OS directory.<br></br>
        /// This generally resolves to "C:\ProgramData" on Windows and "/usr/share" on Linux, but we're <br></br>
        /// going to prefer "/etc" on Linux instead of the default if the <c>GetFolderPath</c> call resolves.
        /// </summary>
        public string CommonAppDataDir {
            get {
                if (null == _commonAppDataDir)
                {
                    if (_isLinux)
                    {
                        _commonAppDataDir = "/etc";
                        return _commonAppDataDir;
                    }

                    if (null == Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))
                    {
                        if (_isWindows)
                        {
                            _commonAppDataDir = @"C:\ProgramData";
                        }
                        else
                        {
                            throw new PlatformNotSupportedException($"The current platform: \"{RuntimeInformation.OSDescription}\" is not supported.");
                        }
                    }
                    else
                    {
                        _commonAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    }
                }

                return _commonAppDataDir;
            }
        }

        /// <summary>
        /// A <see cref="string"/> containing the path to the "ApplicationData" OS directory.<br></br>
        /// This generally resolves to "%USERPROFILE%\AppData\Roaming" on Windows and doesn't always resolve on<br></br>
        /// Linux, but we're going to prefer "$HOME/.config" on Linux instead of the default if the <c>GetFolderPath</c>
        /// call does resolve.
        /// </summary>

#pragma warning disable CA1416
        public string AppDataDir
        {
            get {
                if (null == _appDataDir)
                {
                    if (_isLinux)
                    {
                        _appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                        if (Directory.Exists(_appDataDir) == false)
                        {
#if NET7_0_OR_GREATER
                            Directory.CreateDirectory(_appDataDir,
                                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                                UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                                );
#else
                        Directory.CreateDirectory(_appDataDir);
#endif
                        }
                        return _appDataDir;
                    }

                    if (null == Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                    {
                        if (_isWindows)
                        {
                            _appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming");
                            if (Directory.Exists(_appDataDir) == false)
                            {
                                Directory.CreateDirectory(_appDataDir);
                            }
                        }
                        else
                        {
                            throw new PlatformNotSupportedException($"The current platform: \"{RuntimeInformation.OSDescription}\" is not supported.");
                        }
                    }
                    else
                    {
                        _appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    }
                }

                return _appDataDir;
            }
        }
#pragma warning restore CA1416

        /// <summary>
        /// A <see cref="DateTime"/> public property backed by the <see cref="_creationTime"/> class variable.
        /// </summary>
        public DateTime CreationTime { get => _creationTime; }

        /// <summary>
        /// A <see cref="ConfigDefinition.ConfigRoot"/> public property backed by the <see cref="_defaultConfig"/> class variable.
        /// <br>If this property is accessed before <c>_defaultConfig</c> is initialized, it calls <see cref="GetDefaultConfig"/></br>
        /// <br>to populate the class variable and avoid a null dereference.</br>
        /// </summary>
        public ConfigDefinition.ConfigRoot DefaultConfig
        {
            get
            {
                _defaultConfig ??= GetDefaultConfig();
                return _defaultConfig;
            }
            private set => _defaultConfig = value;
        }

        /// <summary>
        /// A public property backed by the <see cref="_runningConfig"/> class variable, which holds the running configuration.
        /// <br>Like <c>DefaultConfig</c>, this property initializes its corresponding class variable by calling <see cref="GetDefaultConfig"/></br>
        /// <br>to avoid a null dereference.</br>
        /// </summary>
        public ConfigDefinition.ConfigRoot RunningConfig
        {
            get
            {
                _runningConfig ??= GetDefaultConfig();
                return _runningConfig;
            }
            private set => _runningConfig = value;
        }

        /// <summary>
        /// A convenience property that returns the <see cref="ConfigDefinition.LoggingRoot"/> object from the <see cref="_runningConfig"/> object.
        /// </summary>
        public ConfigDefinition.LoggingRoot LoggingConfig { get => _runningConfig.Logging; }

        /// <summary>
        /// A convenience property that returns the <see cref="ConfigDefinition.DomainInfoRoot"/> object from the <see cref="_runningConfig"/> object.
        /// </summary>
        public ConfigDefinition.DomainInfoRoot DomainInfo { get => _runningConfig.DomainInfo; }

        /// <summary>
        /// Public property that returns the username of the executing user. Backed by the <see cref="_whoAmI"/> class variable.
        /// </summary>
        public string? WhoAmI { get => _whoAmI; }

        /// <summary>
        /// Public property that returns the path to the configuration directory. Backed by the <see cref="_configHome"/> class variable.
        /// </summary>
        public string ConfigHome { get => _configHome; private set => _configHome = value; }

        /// <summary>
        /// Public property that returns the filename of the default configuration file. Backed by the <see cref="_defaultConfigFilename"/> class variable.
        /// </summary>
        public string DefaultConfigFilename { get => _defaultConfigFilename; private set => _defaultConfigFilename = value; }

        /// <summary>
        /// Public property that returns a <see cref="string"/> representation of the full path to the default configuration file.<br></br>
        /// Backed by the <see cref="_defaultConfigFile"/> class variable.
        /// </summary>
        public string DefaultConfigFile { get => _defaultConfigFile; private set => _defaultConfigFile = value; }

        /// <summary>
        /// A convenience property that returns an array of domain names from the <see cref="_runningConfig"/> object.
        /// </summary>
        public string[] Domains { get => _runningConfig.DomainInfo.MatchDomains; }

        /// <summary>
        /// A convenience property that returns the username regex from the <see cref="_runningConfig"/> object.
        /// </summary>
        public string UsernameRegex { get => _runningConfig.DomainInfo.UsernameRegex; }

        /// <summary>
        /// A convenience property that returns the SSH key attribute from the <see cref="_runningConfig"/> object.
        /// </summary>
        public string SSHKeyAttribute { get => _runningConfig.DomainInfo.ConnectionInfo.SSHKeyAttribute; }

        /// <summary>
        /// A convenience property that returns whether logging is enabled or not. The value is pulled from the <see cref="_runningConfig"/> object.
        /// </summary>
        public bool LoggingEnabled { get => _runningConfig.Logging.Enabled; set => _runningConfig.Logging.Enabled = value; }

        /// <summary>
        /// A convenience property that returns the path where logs are stored. The value is pulled from the <see cref="_logPath"/> class variable.
        /// </summary>
        public string LogPath { get => _logPath; set => _logPath = value; }

        /// <summary>
        /// A convenience property that evaluates to <c>true</c> if the host operating system is Windows. Backed by the <see cref="_isWindows"/> class variable.
        /// </summary>
        public bool IsWindows { get => _isWindows; private set => _isWindows = value; }

        /// <summary>
        /// A convenience property that evaluates to <c>true</c> if the host operating system is Linux. Backed by the <see cref="_isLinux"/> class variable.
        /// </summary>
        public bool IsLinux { get => _isLinux; private set => _isLinux = value; }

        /// <summary>
        /// A useless convenience property that returns the name of the operating system. Currently unused.
        /// </summary>
        public string OS { get => _os; private set => _os = value; }

        /// <summary>
        /// <see cref="Configuration"/> default constructor.
        /// </summary>

        public Configuration(ILogger<Configuration> _logger)
        {
            logger = _logger;
            Init();
        }
        public Configuration()
        {
            Init();
        }

        /// <summary>
        /// A method to do shit that we may not want to do in a constructor or make sure<br></br>
        /// that some code is always executed in all constructors.
        /// </summary>
        protected internal void Init()
        {
            _defaultConfigFile = String.Empty;
            _defaultConfig = GetDefaultConfig();
            _runningConfig ??= GetDefaultConfig();
            _os = _isWindows ? "Windows" : _isLinux ? "Linux" : "Unknown";


#pragma warning disable CA1416 // Validate platform compatibility
            // This calls below are guarded by the bool being checked, but
            // the compiler just won't stfu. It's like, brother, come on.
            // You can see the fucking value of _isWindows and how it's set
            // and infer that it can never evaluate to "true" on any platform other
            // than fucking Windows! And it's set at class instantiation too!
            //
            // Go home Roslyn, you're drunk.

            if (_isWindows)
            {
                WindowsIdentity? winIdentity = WindowsIdentity.GetCurrent();
                _whoAmI = winIdentity.Name.ToLower() ?? "default";
                winIdentity?.Dispose();
            }
#pragma warning restore CA1416

            else
            {
                Process proc = new();
                try
                {
                    string helperBinary = File.Exists("/usr/bin/env")
                        ? "/usr/bin/env" : File.Exists("/usr/bin/whoami")
                        ? "/usr/bin/whoami" : "whoami";

                    proc.StartInfo.FileName = helperBinary;
                    if (helperBinary == "/usr/bin/env")
                    {
                        proc.StartInfo.Arguments = "whoami";
                    }
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.Start();
                    proc.WaitForExit();
                    _whoAmI = proc.StandardOutput.ReadToEnd() ?? "default";
                }
                catch
                {
                    _whoAmI = Environment.UserName ?? "default";
                }
                finally
                {
                    proc.Dispose();
                    _whoAmI = _whoAmI?.Trim().ToLower();
                }
            }

            // Prefer the current executing directory if a config file exists there or the running user
            // couldn't be determined
            if (File.Exists(_defaultConfigFilename) || _whoAmI == "default")
            {
                _configHome = Directory.GetCurrentDirectory();
            }

            else if (File.Exists(Path.Combine(CommonAppDataDir, "keyscmd", _defaultConfigFilename)))
            {
                _configHome = Path.Combine(CommonAppDataDir, "keyscmd");
            }

            else if (File.Exists(Path.Combine((Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? AppDataDir), "keyscmd", _defaultConfigFilename)))
            {
                _configHome = Path.Combine((Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? AppDataDir), "keyscmd");
            }
            else
            {
                // Config file doesn't exist
                // Prefer "ProgramData" on Windows and "/etc" on Linux if running as privileged user
                if (_builtinSpecialAccounts.Contains(_whoAmI))
                {
                    _configHome = Path.Combine(CommonAppDataDir, "keyscmd");
                }

                else
                {
                    _configHome = Path.Combine((Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? AppDataDir), "keyscmd");
                }
            }

            _defaultConfigFile = Path.Combine(_configHome, _defaultConfigFilename);

#pragma warning disable CA1416 // Validate platform compatibility

            if (!Directory.Exists(_configHome))
            {
                if (_isLinux)
                {

#if NET7_0_OR_GREATER
                    Directory.CreateDirectory(_configHome,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                        );
#else
                    Directory.CreateDirectory(_configHome);
#endif
                }
                else
                {
                    Directory.CreateDirectory(_configHome);
                }
            }
#pragma warning restore CA1416 // Validate platform compatibility

            LoadConfig();

            if (String.IsNullOrWhiteSpace(_runningConfig.Logging.TimestampFormat))
            {
                _runningConfig.Logging.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fff";
            }
            else
            {
                bool isValidFormat = false;
                isValidFormat = DateTime.TryParse(_runningConfig.Logging.TimestampFormat, out DateTime validatedDateTime);
                _runningConfig.Logging.TimestampFormat = isValidFormat ? _runningConfig.Logging.TimestampFormat : "yyyy-MM-ddTHH:mm:ss.fff";
            }

            _logPath = Path.Combine(_configHome, _runningConfig.Logging.LogDirectory);

            ParseConnectionInfo();
            _creationTime = DateTime.Now;
        }

        /// <summary>
        /// The wall of assignments in this method is fucking ugly, but it's the quickest way to do data validation on what
        /// <br>is inherently untrusted data read from the config file.</br>
        /// </summary>
        private void ParseConnectionInfo()
        {
            ConfigDefinition.DomainInfoRoot.ConnectionInfoRoot tempConnectionInfo = _runningConfig.DomainInfo.ConnectionInfo;
            _runningConfig.DomainInfo.ConnectionInfo.AttemptAutoBind = (tempConnectionInfo.AttemptAutoBind != true && tempConnectionInfo.AttemptAutoBind != false) ? true : tempConnectionInfo.AttemptAutoBind;
            _runningConfig.DomainInfo.ConnectionInfo.AuthType = string.IsNullOrWhiteSpace(tempConnectionInfo.AuthType) ? AuthType.Negotiate.ToString() : tempConnectionInfo.AuthType;
            _runningConfig.DomainInfo.ConnectionInfo.EncryptConnection = (tempConnectionInfo.EncryptConnection != true && tempConnectionInfo.EncryptConnection != false) || tempConnectionInfo.EncryptConnection;
            _runningConfig.DomainInfo.ConnectionInfo.Domain = string.IsNullOrWhiteSpace(tempConnectionInfo.Domain) ? "KeysCmdDefaultValue" : tempConnectionInfo.Domain;
            _runningConfig.DomainInfo.ConnectionInfo.DomainController = string.IsNullOrWhiteSpace(tempConnectionInfo.DomainController) ? "KeysCmdDefaultValue" : tempConnectionInfo.DomainController;
            _runningConfig.DomainInfo.ConnectionInfo.Port = (tempConnectionInfo.Port is <= 0 or > 65535) ? 389 : tempConnectionInfo.Port;
            _runningConfig.DomainInfo.ConnectionInfo.BaseDN = string.IsNullOrWhiteSpace(tempConnectionInfo.BaseDN) ? "KeysCmdDefaultValue" : tempConnectionInfo.BaseDN;
            _runningConfig.DomainInfo.ConnectionInfo.SearchFilter = string.IsNullOrWhiteSpace(tempConnectionInfo.SearchFilter) ? "(sAMAccountName=KeysCmdUsernameHere)" : tempConnectionInfo.SearchFilter;
            _runningConfig.DomainInfo.ConnectionInfo.SSHKeyAttribute = string.IsNullOrWhiteSpace(tempConnectionInfo.SSHKeyAttribute) ? "altSecurityIdentities" : tempConnectionInfo.SSHKeyAttribute;
        }

        /// <summary>
        /// Loads the configuration file from disk and deserializes it into a strongly-typed <see cref="ConfigDefinition.ConfigRoot"/> object.
        /// <br>If the config file does not exist, a default configuration is written out to disk, then read back in to the <see cref="_defaultConfig"/></br>
        /// <br>variable. This is to ensure that the config location is accessible/can be written to.</br>
        /// </summary>
        public void LoadConfig()
        {
            if (!File.Exists(_defaultConfigFile))
            {
                SaveConfig();
            }

            try
            {
                string json = File.ReadAllText(_defaultConfigFile);
                _runningConfig = JsonConvert.DeserializeObject<ConfigDefinition.ConfigRoot>(json) ?? GetDefaultConfig();
            }
            catch (Exception ex)
            {
                logger?.LogCritical("Failed to load configuration file from disk: {ex.Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Saves the current configuration to disk by serializing the <see cref="_runningConfig"/> object and writing it to the
        /// <br>config file location stored in <see cref="_defaultConfigFile"/></br>
        /// </summary>
        public void SaveConfig()
        {
            SaveConfig(_runningConfig, _defaultConfigFile);
        }

        public void SaveConfig(ConfigDefinition.ConfigRoot _incomingConfig)
        {
            SaveConfig(_incomingConfig, _defaultConfigFile);
        }

        public void SaveConfig(string _savePath)
        {
            SaveConfig(_runningConfig, _savePath);
        }

        public void SaveConfig(ConfigDefinition.ConfigRoot _incomingConfig, string _savePath)
        {
            string json = JsonConvert.SerializeObject(_incomingConfig, Formatting.Indented);
            try
            {
                File.WriteAllText(_savePath, json);
            }
            catch (Exception ex)
            {
                logger?.LogCritical("Failed to write configuration file to disk: {ex.Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Returns a <see cref="ConfigDefinition.ConfigRoot"/> object instantiated with the default config values.
        /// </summary>
        /// <returns>The <see cref="ConfigDefinition.ConfigRoot"/></returns>
        public ConfigDefinition.ConfigRoot GetDefaultConfig()
        {

            return new ConfigDefinition.ConfigRoot
            {
                DomainInfo = new ConfigDefinition.DomainInfoRoot
                {
                    ConnectionInfo = new ConfigDefinition.DomainInfoRoot.ConnectionInfoRoot
                    {
                        AttemptAutoBind = true,
                        AuthType = AuthType.Negotiate.ToString(),
                        EncryptConnection = true,
                        Domain = "KeysCmdDefaultValue",
                        DomainController = "KeysCmdDefaultValue",
                        Port = 389,
                        BaseDN = "KeysCmdDefaultValue",
                        SearchFilter = "(sAMAccountName=KeysCmdUsernameHere)",
                        SearchScope = SearchScope.Subtree.ToString(),
                        SSHKeyAttribute = "altSecurityIdentities"
                    },
                    UsernameRegex = @"(?<Domain>^[a-zA-z0-9]+)\\(?<User>\w+)$",
                    MatchDomains = ["example.com"]
                },
                Logging = new ConfigDefinition.LoggingRoot
                {
                    Enabled = true,
                    LogDirectory = "logs",
                    LogFilename = "keyscmd.log",
                    TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fff",
                    UTC = false,
                    MinimumLogLevel = "Information"
                }
            };
        }
    }
}
