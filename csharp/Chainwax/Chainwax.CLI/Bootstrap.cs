using Chainwax;
using Chainwax.CLI.ChainwaxSpectre;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Chainwax.CLI.Helpers;
using Chainwax.CLI.Interfaces;
using Chainwax.CLI.ChainwaxSpectre.Commands;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using Spectre.Console;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;
using System.Runtime.CompilerServices;
using Chainwax.CLI.ChainwaxSpectre.Meta;
using System.IO.Pipelines;
using Spectre.Console.Advanced;
using Spectre.Console.Rendering;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Chainwax.CLI {

    public class Bootstrap {

        protected internal static ChainwaxBase _cbase;

        public static ChainwaxBase CBase {
            get => _cbase;
            private set {
                _cbase = value;
            }
        }

        public static ILogger<BareLogger> Bootstraplogger;

        static int Main(string[] args) {

            AnsiConsole.Record();
            AnsiConsole.WriteLine();

            var sharedLoggerFactory = LoggerFactory.Create(builder =>
            {
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

            CBase = new ChainwaxBase {
                ConsoleHWND = ChainwaxBase.GetConsoleWindow()
            };

            CBase.GenericHost = BuildAppHost(sharedLoggerFactory);
            ILogger? preHostLogger = Utilities.CreateLogger(typeof(Bootstrap));
            //var preHostLogger = sharedLoggerFactory.CreateLogger<Bootstrap>();

            Utilities.ShowLogo();
            preHostLogger?.LogInformation("Starting application setup...");
            ;

            //////Bootstrap.ShowWindow(ConsoleHWND, (int)ConsoleState.HIDE);

            string helpExampleDir = Environment.GetEnvironmentVariable("USERPROFILE") ?? "C:\\Users\\username";

            var registrar = new TypeRegistrar(CBase.GenericHost);
            CBase.AppTypeRegistrar = registrar;

            ////var blah = CBase.GenericHost.Build();
            ////var serv = blah.Services;
            ////Bootstraplogger = serv.GetRequiredService<ILogger<BareLogger>>();
            ////Bootstraplogger.LogInformation("{success}: [blue]Bootstrapping[/] complete", SuccessMessage.Value)
            ;

            var spectreApp = new CommandApp(registrar);
            spectreApp.Configure(c =>
            c.SetApplicationName(((Environment.ProcessPath) == null) ? "Chainwax" : Environment.ProcessPath)
            .UseAssemblyInformationalVersion()
            .AddBranch<AddLinkSettings>(AppInfo.BranchAdd.BranchName, add => {
                add.SetDescription(AppInfo.BranchAdd.Description);
                add.AddCommand<AddSymLinkCommand>(AppInfo.BranchAdd.Commands.Symlink.CommandName)
                .WithDescription(AppInfo.BranchAdd.Commands.Symlink.Description)
                .WithAlias(AppInfo.BranchAdd.Commands.Symlink.Aliases[0])
                .WithAlias(AppInfo.BranchAdd.Commands.Symlink.Aliases[1])
                .WithExample(AppInfo.BranchAdd.Commands.Symlink.Examples[0]
                    .Split(" ")
                    .Select(piece => $"{piece}")
                    .ToArray())
                .WithExample(AppInfo.BranchAdd.Commands.Symlink.Examples[1]
                    .Split(" ")
                    .Select(piece => $"{piece}")
                    .ToArray());
                add.AddCommand<AddHardLinkCommand>(AppInfo.BranchAdd.Commands.Hardlink.CommandName)
                .WithDescription(AppInfo.BranchAdd.Commands.Hardlink.Description)
                .WithAlias(AppInfo.BranchAdd.Commands.Hardlink.Aliases[0]);
            })
            .WithAlias(AppInfo.BranchAdd.Aliases[0])
            .WithAlias(AppInfo.BranchAdd.Aliases[1]));

            spectreApp.Configure(c => c.AddBranch<TestLogSettings>("test", add => {
                add.SetDescription("Testing stuff.");
                add.AddCommand<TestLogCommand>("log");
            }));

            CBase.SpectreCommandApp = spectreApp;

            preHostLogger?.LogInformation("Configured CLI app, preparing to run...");

            int spectreExitCode = spectreApp.Run(args);
            string? recording = AnsiConsole.ExportHtml();
            string? txtRecording = AnsiConsole.ExportText();
            if ((recording != null)) {
                File.WriteAllText("C:\\Users\\tool\\test_recording.html", recording);
                File.WriteAllText("C:\\Users\\tool\\test_recording.txt", recording);
            }
            return spectreExitCode;

        }


        internal static IHostBuilder BuildAppHost(ILoggerFactory? sharedFactory) {

            string basePath = Directory.GetCurrentDirectory();
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c => {
                    c.SetBasePath(basePath);
                })
                .ConfigureHostOptions(options => {
                    options.ShutdownTimeout = System.TimeSpan.FromSeconds(15);
                })
                .ConfigureServices((context, services) => {
                    services.AddSingleton<ILoggerFactory>(sharedFactory);
                    services.AddSingleton<ICommandInterceptor, BaseInterceptor>();
                })
                .ConfigureLogging(builder => builder.ClearProviders());

            return hostBuilder;
        }

    }
}
