using Chainwax;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Spectre.Console;
using Chainwax.CLI.Interfaces;
using Spectre.Console.Advanced;

namespace Chainwax.CLI.Helpers {
    public static class SymlinkHelper {

        private static ILogger? _logger = Utilities.CreateLogger(typeof(SymlinkHelper));

        public static ILogger? Logger
        {
            get => _logger;
            private set {
                _logger = value;
            }
        }
        public static int Result { get; set; }

        public static void CreateSymlink(Dictionary<string, string> Items, int ItemCount, string Destination) {

            foreach (string inputItem in Items.Keys) {

                var itemType = Items[inputItem];
                Logger.LogInformation("Item: {inputItem}", inputItem);
                Logger.LogInformation("Type: {itemType}", itemType);

                if (itemType == "file") {
                    string itemName = Path.GetFileName(inputItem);
                    string symlinkName = Path.Combine(Destination, itemName);
                    Logger.LogInformation("Creating symlink for file {itemName} at {Destination}", itemName, Destination);
                    try {
                        File.CreateSymbolicLink(Destination, inputItem);
                        Result = 0;
                        Logger.LogInformation("{success}: Created symlink for file [green]{itemName}[/] at: [blue]{Destination}[/]", SuccessMessage.Value, itemName, Destination);
                    }
                    catch (Exception e) {
                        Logger.LogError("{fail}: Error creating symlink: {e.Message}", FailMessage.Value, e.Message);
                        Result = 1;
                        continue;
                    }
                }
                else if (itemType == "directory") {
                    string dirName = Path.GetFileName(inputItem);
                    string symlinkName = Path.Combine(Destination, dirName);
                    Logger.LogInformation("Creating symlink for directory {dirName} at {Destination}", dirName, Destination);
                    try {
                        Directory.CreateSymbolicLink(Destination, inputItem);
                        Logger.LogInformation("{success}: Created[/] symlink for directory [green]{dirName}[/] at: [blue]{Destination}[/]", SuccessMessage.Value, dirName, Destination);
                        Result = 0;
                    }
                    catch (Exception e) {
                        Logger.LogError("{fail} Error creating symlink: {e.Message}", FailMessage.Value, e.Message);
                        Result = 1;
                        continue;
                    }
                }
                else {
                    Logger.LogError("{fail} Unknown", FailMessage.Value);
                    Result = 1;
                }
            }

            ;
            // if destinationDirectory is null but targetItems is filled, user just right-clicked
            // on the file, it was not a drag & drop operation
            ;
            ;
            ;
            ;
            ;
            ;
            ;
            ;
            ;
            ;

        }

    }
}
