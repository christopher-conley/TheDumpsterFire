using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Common;
using static RosettaTools.Pwsh.Text.RevenantLogger.Common.StaticStrings;
using RosettaTools.Pwsh.Text.RevenantLogger.Helpers;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.TypeFormatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Spectre.Console;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;
using System.Collections;
using System.Diagnostics;
using Vertical.SpectreLogger.Rendering;
using Vertical.SpectreLogger.Core;
using System.Globalization;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Helpers {
    internal class Utilities : RevenantLoggerPSCmdlet {
        private readonly ILogger<Utilities>? _logger;

        public ILogger<Utilities>? Logger
        {
            get => _logger;
        }

        public static IRevenantConfiguration Config {
            get {
                return RevenantConfig ?? new Configuration();
            }
        }

        public Utilities()
        {
            _config ??= RevenantConfig ?? new Configuration();
        }
        public Utilities(ILogger<Utilities> logger)
        {
            _logger = logger;
        }

        public Utilities(ILogger<Utilities> logger, IRevenantConfiguration config) {
            _config = config;
            _logger = logger;
        }

        // Filter "Application started" blah blah blah messages from hostbuilder.
        // Necessary because apparently IHostBuilder does not respect the
        // ASPNETCORE_SUPPRESSSTATUSMESSAGES environment variable
        public static bool FilterChattyASPNET(in LogEventContext context)
        {
            return (context.CategoryName != "Microsoft.Hosting.Lifetime");
        }
        public static ILoggerFactory? NewLoggerFactory()
        {
            return NewLoggerFactory(Config);
        }
        public static ILoggerFactory? NewLoggerFactory(IRevenantConfiguration LoggerConfig) {
            string dateFormat = LoggerConfig.LoggingConfig.DateFormat;
            string timeFormat = LoggerConfig.LoggingConfig.TimeFormat;

            //string outputTemplate = $"{OpenBracket.Value}{FormattedTimestamp.Value} {{LogLevel}}{CloseBracket.Value} {{Message}}\n{{Exception}}";
            //string outputTemplate = $"{OpenBracket.Value}[dim cyan]{{DateTime:{dateFormat}}}[/]{Sep.Value}[dim cyan]{{DateTime:{timeFormat}}}[/] {{LogLevel}}{CloseBracket.Value} {{Message}}\n{{Exception}}";
            //string outputTemplate = $"{OpenBracket.Value}{DateFormatStamp()}{DateTimeSep}{TimeFormatStamp()} {{LogLevel}}{CloseBracket.Value} {{Message}}\n{{Exception}}";

            //string outputTemplate = $"{OpenBracket.Value}[dim cyan]{{DateTime:{dateFormat}}}[/][dim grey]{Sep.Value}[/][cyan]{{DateTime:{timeFormat}}}[/] {{LogLevel}}{CloseBracket.Value} {{Message}}\n{{Exception}}";
            string outputTemplate = $"{OpenBracket.Value}{{DateTime:{dateFormat}}}[{ColorConfig.TimestampSeperator}]{Sep.Value}{CloseTag}{{DateTime:{timeFormat}}} {{LogLevel}}{CloseBracket.Value} {{Message}}\n{{Exception}}";

            return LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                if (LoggerConfig.FileLoggingEnabled)
                {
                    builder.AddProvider(new FileLogProvider(config: LoggerConfig));
                }
                
                builder.AddSpectreConsole(config => {
                    config.SetLogEventFilter(new LogEventFilterDelegate(Utilities.FilterChattyASPNET));
                    config.AddTemplateRenderers()
                    .WriteInForeground()
                    .ConfigureProfiles(profiles => {
                        profiles.PreserveMarkupInFormatStrings = true;
                        profiles.AddTypeStyle<SuccessMessage>("[green1]");
                        profiles.AddTypeStyle<WarnMessage>("[yellow1]");
                        profiles.AddTypeStyle<FailMessage>("[red1]");
                        profiles.AddTypeStyle<DateTimeRenderer.Value>($"[{ColorConfig.Timestamp}]");
                        profiles.AddValueStyle(false, $"[{ColorConfig.BoolFalse}]");
                        profiles.AddValueStyle(true, $"[{ColorConfig.BoolTrue}]");

                        foreach (KeyValuePair<LogLevel, string> kvp in LogLevelColors)
                        {
                            profiles.AddValueStyle(kvp.Key, kvp.Value);
                        }

                        profiles.ConfigureOptions<DateTimeRenderer.Options>(renderer => {
                            if (LoggerConfig.LoggingConfig.UTC)
                            {
                                renderer.ValueFactory = () => DateTime.UtcNow;
                            }
                            else
                            {
                                renderer.ValueFactory = () => DateTime.Now;
                            }
                        });
                        profiles.OutputTemplate = outputTemplate.ToString();
                    });
                    //config.ConfigureProfile(LogLevel.Information, profile => {
                    //    profile.OutputTemplate = "[grey85][[{DateTime:T} [red]Info[/]]] {Message}{NewLine+}{Exception}[/]";
                    //});
                });
            });
        }

        public static ILogger? NewLogger(Type type)
        {

            var loggerFactory = NewLoggerFactory(Config);

            var builtLogger = loggerFactory?.CreateLogger(type);
            return builtLogger;
        }
        public static ILogger? NewLogger(Type type, IRevenantConfiguration LoggerConfig) {

            var loggerFactory = NewLoggerFactory(LoggerConfig);

            var builtLogger = loggerFactory?.CreateLogger(type);
            return builtLogger;
        }

        public static ILogger? NewLogger(string categoryName, ILoggerFactory? factory)
        {
            var builtLogger = factory?.CreateLogger(categoryName);
            return builtLogger;
        }

        public static ILogger? NewLogger(Type type, ILoggerFactory? factory) {
            var builtLogger = factory?.CreateLogger(type);
            return builtLogger;
        }

        //public static ILogger? NewLogger<TLogger>(ILoggerFactory? factory) where TLogger : class
        //{
        //    var builtLogger = factory?.CreateLogger(typeof(TLogger));
        //    return builtLogger;
        //}

        public static ILogger<TLogger>? NewLogger<TLogger>(ILoggerFactory? factory) where TLogger : class
        {
            var builtLogger = factory?.CreateLogger<TLogger>();
            return builtLogger;
        }

        public static Hashtable GetApplicationVersionInfo() {
            Hashtable returnObject = [];
            Assembly? selfAssembly = Assembly.GetExecutingAssembly();
            FileVersionInfo? version = FileVersionInfo.GetVersionInfo(selfAssembly.Location);
            returnObject.Add("Name", selfAssembly.GetName().ToString());
            returnObject.Add("FullName", selfAssembly.FullName);
            returnObject.Add("FileVersion", (version.FileVersion ?? "Unversioned"));
            returnObject.Add("ImageRuntimeVersion", selfAssembly.ImageRuntimeVersion);
            returnObject.Add("Location", (selfAssembly.Location ?? "Unknown"));
            returnObject.Add("EntryPoint", (selfAssembly.EntryPoint?.ToString() ?? "Unknown"));
            returnObject.Add("DefinedTypes", selfAssembly.DefinedTypes.ToString());
            returnObject.Add("IsFullyTrusted", selfAssembly.IsFullyTrusted.ToString());

            selfAssembly = null;
            return returnObject;
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public string _CallerName([CallerMemberName] string caller = null) {
            return caller;
        }
        public static string CallerName([CallerMemberName] string caller = null) {
            return caller;
        }

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        public static void ShowLogo() {
            (string fullFontName, string shortFontName) = (String.Empty, String.Empty);
            bool hasChars = false;

            while (!hasChars) {
                (fullFontName, shortFontName) = Utilities.GetRandomFigletFont();
                hasChars = Utilities.FigletFontHasCharacters(fullFontName);
            }
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"{fullFontName}"))
            using (new StreamReader(stream)) {
                ;
                FigletFont font;
                FigletText logoText;
                try {
                    font = FigletFont.Load(stream);
                    logoText = new FigletText(font, "Revenant Logger")
                        .Centered()
                        .Color(Color.Red);
                    if ((null == logoText) || (font == FigletFont.Default)) {
                        AnsiConsole.MarkupLine("[bright yellow]Revenant Logger[/]");
                        return;
                    }
                    AnsiConsole.Write(logoText);
                    AnsiConsole.Write(new Rule($"[yellow]Figlet font: {shortFontName}[/]\n\n").Justify(Justify.Right).RuleStyle("red"));
                }
                catch {
                    AnsiConsole.MarkupLine("[bright yellow]Revenant Logger[/]");
                    return;
                }
            }
        }

        public static (string, string) GetRandomFigletFont() {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string figletExtRegex = ".+(?<FigletExt>\\.flf$)";
            string[]? resources = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => Regex.IsMatch(x, figletExtRegex, RegexOptions.IgnoreCase)).ToArray();
            ;
            if ((null == resources) || (resources.Length == 0)) {
                return (null, null);
            }
            else {
                Random random = new();
                int randomIndex = random.Next(0, (resources.Length - 1));
                string fullFontName = resources[randomIndex];
                string shortFontName = fullFontName.Replace($"{assemblyName}.Assets.figlet_fonts.", "")
                        .Replace(".flf", "");
                return (fullFontName, shortFontName);
            }
        }

        public static bool FigletFontHasCharacters(string fontName) {
            if ((String.IsNullOrWhiteSpace(fontName)) || (null == fontName)) {
                return false;
            }

            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"{fontName}"))
            using (new StreamReader(stream)) {
                ;
                FigletFont font;
                try {
                    font = FigletFont.Load(stream);
                }
                catch (InvalidOperationException) {
                    return false;
                }

                if (font.Count == 0) {
                    return false;
                }
                else {
                    return true;
                }
            }
        }

        public static void ListFigletFont() {

            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var resource in resources) {
                if ((null == resource) || (resource.Length == 0)) {
                    continue;
                }
                Console.WriteLine(resource);
            }
        }
    }
}
