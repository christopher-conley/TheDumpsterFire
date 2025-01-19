using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Common;
using RosettaTools.Pwsh.Text.RevenantLogger.Helpers;
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

namespace RosettaTools.Pwsh.Text.RevenantLogger.Helpers {
    internal class Utilities : RevenantLoggerPSCmdlet {
        private readonly ILogger<Utilities> _logger;

        public ILogger<Utilities> Logger
        {
            get => _logger;
        }

        public Utilities(ILogger<Utilities> logger) {
            _logger = logger;
        }

        public static ILoggerFactory? NewLoggerFactory(Configuration LoggerConfig) {
            //string modulePath = Path.GetDirectoryName(typeof(Utilities).Assembly.Location);
            //string ObjectPool = Path.Combine(modulePath, "Microsoft.Extensions.ObjectPool.dll");
            //string SysTextJson = Path.Combine(modulePath, "System.Text.Json.dll");
            //string SysDiagnosticSource = Path.Combine(modulePath, "System.Diagnostics.DiagnosticSource.dll");

            //System.Runtime.Loader.AssemblyLoadContext ObjPool = AssemblyLoadContext.Default;
            //ObjPool.LoadFromAssemblyPath(ObjectPool);
            //ObjPool.LoadFromAssemblyPath(SysTextJson);
            //ObjPool.LoadFromAssemblyPath(SysDiagnosticSource);
            //Assembly.LoadFrom(ObjectPool);
            //Assembly.LoadFrom(SysTextJson);
            //Assembly.LoadFrom(SysDiagnosticSource);


            return LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddSpectreConsole(config => {
                    config.AddTemplateRenderers()
                    .WriteInForeground()
                    .ConfigureProfiles(profiles => {
                        profiles.PreserveMarkupInFormatStrings = true;
                        profiles.AddTypeStyle<SuccessMessage>("[green1]");
                        profiles.AddTypeStyle<WarnMessage>("[yellow1]");
                        profiles.AddTypeStyle<FailMessage>("[red1]");
                    });
                });
                if (LoggerConfig.LoggingEnabled) {
                    builder.AddProvider(new FileLogProvider());
                }
            });
        }
        public static ILogger? NewLogger(Type type, Configuration LoggerConfig) {

            var loggerFactory = NewLoggerFactory(LoggerConfig);

            var builtLogger = loggerFactory.CreateLogger(type);
            return builtLogger;
        }

        public static ILogger? NewLogger(Type type, ILoggerFactory? factory) {
            var builtLogger = factory?.CreateLogger(type);
            return builtLogger;
        }

        public static Hashtable GetApplicationVersionInfo() {
            Hashtable returnObject = new();
            Assembly? selfAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo? version = FileVersionInfo.GetVersionInfo(selfAssembly.Location);
            returnObject.Add("Name", selfAssembly.GetName().ToString());
            returnObject.Add("FullName", selfAssembly.FullName);
            returnObject.Add("FileVersion", (version.FileVersion ?? "Unversioned"));
            returnObject.Add("ImageRuntimeVersion", selfAssembly.ImageRuntimeVersion);
            returnObject.Add("Location", (selfAssembly.Location ?? "Unknown"));
            returnObject.Add("EntryPoint", (selfAssembly.EntryPoint.ToString() ?? "Unknown"));
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
                    logoText = new FigletText(font, "RosettaTools Pwsh RevenantLogger")
                        .Centered()
                        .Color(Color.Red);
                    if ((null == logoText) || (font == FigletFont.Default)) {
                        AnsiConsole.MarkupLine("[bright yellow]RosettaTools Pwsh RevenantLogger[/]");
                        return;
                    }
                    AnsiConsole.Write(logoText);
                    AnsiConsole.Write(new Rule($"[yellow]Figlet font: {shortFontName}[/]\n\n").Justify(Justify.Right).RuleStyle("red"));
                }
                catch {
                    AnsiConsole.MarkupLine("[bright yellow]CRosettaTools Pwsh RevenantLogger[/]");
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
