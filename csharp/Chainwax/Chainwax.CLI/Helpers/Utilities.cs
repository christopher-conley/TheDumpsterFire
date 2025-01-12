using Spectre.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;

namespace Chainwax.CLI.Helpers {

    public abstract class StaticLoggerBase {
        protected static ILogger Logger
        {
            get; private set;
        }
        public static void InitializeLogger(ILoggerFactory factory) {
            Logger = factory.CreateLogger(typeof(StaticLoggerBase));
        }
    }
    internal class Utilities {

        private readonly ILogger<Utilities> _logger;

        public ILogger<Utilities> Logger
        {
            get => _logger;
        }

        public Utilities(ILogger<Utilities> logger) {
            _logger = logger;
        }

        public static ILogger? CreateLogger(Type type) {

            var loggerFactory = LoggerFactory.Create(builder => {
                builder.ClearProviders();
                builder.AddSpectreConsole(config => {
                    config.AddTemplateRenderers()
                    .WriteInForeground()
                    .ConfigureProfiles(profiles => {
                        profiles.PreserveMarkupInFormatStrings = true;
                        profiles.AddTypeStyle<SuccessMessage>("[green1]");
                        profiles.AddTypeStyle<FailMessage>("[red1]");
                    });
                });
            });

            var builtLogger = loggerFactory.CreateLogger(type);
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
            returnObject.Add("CodeBase", (selfAssembly.CodeBase ?? "Unknown"));
            returnObject.Add("Location", (selfAssembly.Location ?? "Unknown"));
            returnObject.Add("EntryPoint", (selfAssembly.EntryPoint.ToString() ?? "Unknown"));
            returnObject.Add("DefinedTypes", selfAssembly.DefinedTypes.ToString());
            returnObject.Add("IsFullyTrusted", selfAssembly.IsFullyTrusted.ToString());

            selfAssembly = null;
            return returnObject;
        }

        public static string GetStringHash(object? inputString, string? hashName, bool? prompt) {
            string[] validHashAlgorithms = [
                "MD5",
                "SHA",
                "SHA1",
                "SHA256",
                "SHA-256",
                "SHA384",
                "SHA-384",
                "SHA512",
                "SHA-512",
                "System.Security.Cryptography.SHA1",
                "System.Security.Cryptography.HashAlgorithm",
                "System.Security.Cryptography.MD5",
                "System.Security.Cryptography.SHA256",
                "System.Security.Cryptography.SHA384",
                "System.Security.Cryptography.SHA512"
                ];

            string hashAlgorithm = ((string?)hashName) ?? "SHA256";

            if ((hashAlgorithm.Equals("SHA256")) && ((string?)hashName != hashAlgorithm)) {
                StringBuilder errorString = new StringBuilder($"The provided hash \"{((string?)hashName)}\" is not a valid hash algorithm.");
                errorString.AppendLine("Valid hash algorithms are:");

                foreach (string algo in validHashAlgorithms) {
                    errorString.AppendLine(algo);
                }

                //App.stdErr?.WriteLine
            }

            return String.Empty;
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
                    logoText = new FigletText(font, "Chainwax CLI")
                        .Centered()
                        .Color(Color.Red);
                    if ((null == logoText) || (font == FigletFont.Default)) {
                        AnsiConsole.MarkupLine("[bright yellow]Chainwax CLI[/]");
                        return;
                    }
                    AnsiConsole.Write(logoText);
                    AnsiConsole.Write(new Rule($"[yellow]Figlet font: {shortFontName}[/]\n\n").Justify(Justify.Right).RuleStyle("red"));
                }
                catch {
                    AnsiConsole.MarkupLine("[bright yellow]Chainwax CLI[/]");
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
                        .Replace(".flf", "", StringComparison.OrdinalIgnoreCase);
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
