using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpShell.Configuration;
using sharpLogger = SharpShell.Diagnostics.Logging;

namespace Chainwax.ShellExtension.Helpers {
    public static class ShellExSymlinkHelper {

        private static string failureCLIHelper = "C:\\Users\\tool\\source\\repos\\Chainwax\\Chainwax.CLI\\bin\\Debug\\net8.0\\Chainwax.CLI.exe";
        public static void CreateSymlink(Dictionary<string, string> Items, int ItemCount, string Destination) {

            string fileName = Process.GetCurrentProcess().MainModule.FileName;

            foreach (string inputItem in Items.Keys) {

                var itemType = Items[inputItem];
                sharpLogger.Log($"Item: {inputItem}");
                sharpLogger.Log($"Type: {itemType}");

                string itemName = Path.GetFileName(inputItem);
                string symlinkName = Path.Combine(Destination, itemName);

                if (itemType == "file") {
                    sharpLogger.Log($"Creating symlink for file {itemName} at {Destination}");
                }
                else if (itemType == "directory") {
                    sharpLogger.Log($"Creating symlink for directory {itemName} at {Destination}");
                }
                else {
                    sharpLogger.Log($"Unknown item type {itemName} encountered, exiting...");
                    return;
                }
                try {
                    Process proc = new();
                    proc.StartInfo.FileName = failureCLIHelper;
                    proc.StartInfo.Arguments = $"add symlink --target \"{inputItem}\" --link-name \"{symlinkName}\"";
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
                    proc.Start();
                    proc.WaitForExit();
                    if (proc.ExitCode == 0) {
                        sharpLogger.Log($"Successfully created symlink for {itemName} at {symlinkName}");
                    }
                    else {
                        sharpLogger.Log($"Error creating symlink, admin helper process exited with failure code: {proc.ExitCode}");
                    }
                    proc.Dispose();
                }
                catch (Exception e) {
                    sharpLogger.Log($"Error creating symlink: {e.Message}");
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
