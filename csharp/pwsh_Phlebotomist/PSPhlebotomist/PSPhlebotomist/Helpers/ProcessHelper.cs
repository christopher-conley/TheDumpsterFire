using Microsoft.Extensions.Logging;
using System.Diagnostics;
using PSPhlebotomist.Common;
using System.Threading;
using System.Text;

namespace PSPhlebotomist.Core.Helpers
{
    /// <summary>
    /// Helper methods for process discovery and monitoring.
    /// </summary>
    public class ProcessHelper
    {
        private readonly ILogger<ProcessHelper> _logger;

        public ProcessHelper(ILogger<ProcessHelper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets process ID by name (case-insensitive). Extension is optional.
        /// </summary>
        public int GetProcessIdByName(string processName)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(processName);
            var processes = Process.GetProcessesByName(nameWithoutExtension);
            var process = processes.FirstOrDefault();

            if (process != null)
            {
                _logger.LogDebug("Found process '{ProcessName}' with PID {ProcessId}", processName, process.Id);
                return process.Id;
            }

            //_logger.LogWarning("Process '{ProcessName}' not found", processName);
            return 0;
        }

        /// <summary>
        /// Waits for a process to launch, with optional timeout.
        /// </summary>
        public async Task<int> AwaitProcessAsync(string processName, int timeoutSeconds = -1)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(processName);
            var startTime = DateTime.Now;
            var timeout = timeoutSeconds > 0 ? TimeSpan.FromSeconds(timeoutSeconds) : Timeout.InfiniteTimeSpan;

            var reminderShown30Sec = false;
            var reminderShown60Sec = false;

            _logger.LogInformation("Waiting for process '{ProcessName}' to launch...", processName);

            while (true)
            {
                var pid = GetProcessIdByName(nameWithoutExtension);
                if (pid > 0)
                {
                    _logger.LogInformation("Process '{ProcessName}' launched with PID {ProcessId}", processName, pid);
                    return pid;
                }

                var elapsed = DateTime.Now - startTime;
                if (timeout != Timeout.InfiniteTimeSpan && elapsed >= timeout)
                {
                    _logger.LogError("Timeout hit waiting for process '{ProcessName}' after {Seconds} seconds", processName, timeoutSeconds);
                    return 0;
                }

                if (!reminderShown30Sec && elapsed.TotalSeconds >= 30)
                {
                    _logger.LogInformation("Still waiting for process '{ProcessName}' after 30 seconds...", processName);
                    reminderShown30Sec = true;
                }

                if (!reminderShown60Sec && elapsed.TotalSeconds >= 60)
                {
                    var timeLeft = timeout != Timeout.InfiniteTimeSpan ? (timeout - elapsed).TotalSeconds : -1;

                    _logger.LogWarning("Still waiting for process '{ProcessName}' after 60 seconds...", processName);

                    if (timeLeft > 0)
                    {
                        _logger.LogWarning("Will wait another {SecondsLeft:F0} seconds before timing out", timeLeft);
                    }

                    reminderShown60Sec = true;
                }

                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Checks if the current process is running as administrator.
        /// </summary>
        public static bool IsRunningAsAdmin()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }


        //public static async Task<ProcReturnInfo> StartProcess(
        //    string path = "",
        //    string arguments = "",
        //    bool useShellExecute = false,
        //    bool redirectStandardOutput = true,
        //    bool redirectStandardError = true,
        //    bool redirectStandardInput = false,
        //    bool createNoWindow = true,
        //    bool async = true
        //    )
        //{
        //    return StartProcess(path, arguments, useShellExecute, redirectStandardOutput, redirectStandardError, redirectStandardInput, createNoWindow);
        //}

        /// <summary>
        /// Starts a process with the specified executable path and arguments, and returns information about its
        /// execution, including exit code and any captured output or errors. This is mainly used by the Cmdlet
        /// to relaunch itself with elevated permissions when the -A/-Admin switch is used.
        /// </summary>
        /// <remarks>If <paramref name="redirectStandardOutput"/> or <paramref
        /// name="redirectStandardError"/> is <see langword="true"/>, the corresponding output or error streams are
        /// captured and included in the returned <see cref="ProcReturnInfo"/>. If the process fails to start, the
        /// returned object contains exception details. This method blocks until the process exits.</remarks>
        /// <param name="path">The full path to the executable file to start. Cannot be null, empty, or whitespace.</param>
        /// <param name="arguments">The command-line arguments to pass to the process. If empty, no arguments are supplied.</param>
        /// <param name="useShellExecute">Indicates whether to use the operating system shell to start the process. Set to <see langword="true"/> to
        /// use the shell; otherwise, <see langword="false"/>. This is set to <see langword="false"/> when using sudo, <see langword="true"/> otherwise.</param>
        /// <param name="redirectStandardOutput">Indicates whether the standard output stream of the process should be redirected and captured.</param>
        /// <param name="redirectStandardError">Indicates whether the standard error stream of the process should be redirected and captured.</param>
        /// <param name="redirectStandardInput">Indicates whether the standard input stream of the process should be redirected.</param>
        /// <param name="createNoWindow">Indicates whether to start the process without creating a new window.</param>
        /// <param name="verb">The verb to use when starting the process (for example, "runas" to start with elevated permissions). If
        /// empty, no verb is specified.</param>
        /// <returns>A <see cref="ProcReturnInfo"/> object containing the process exit code, any captured standard output and
        /// error, and exception information if the process failed to start.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is null, empty, or consists only of white space.</exception>
        public static ProcReturnInfo StartProcess(
            string path = "",
            string arguments = "",
            bool useShellExecute = false,
            bool redirectStandardOutput = true,
            bool redirectStandardError = true,
            bool redirectStandardInput = false,
            bool createNoWindow = true,
            string verb = ""
            )
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            Process proc = new();
            proc.StartInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments,
                UseShellExecute = useShellExecute,
                RedirectStandardOutput = redirectStandardOutput,
                RedirectStandardError = redirectStandardError,
                RedirectStandardInput = redirectStandardInput,
                CreateNoWindow = createNoWindow
            };


            if (string.IsNullOrEmpty(verb) == false)
            {
                proc.StartInfo.Verb = verb;
            }

            ProcReturnInfo returnObject = new ProcReturnInfo
            {
                Path = path,
                Arguments = arguments
            };

            try
            {
                proc.Start();
                proc.WaitForExit();
                returnObject.ExitCode = proc.ExitCode;

                if (redirectStandardOutput)
                {
                    returnObject.StandardOutput = proc.StandardOutput.ReadToEnd();
                }

                if (redirectStandardError)
                {
                    returnObject.StandardError = proc.StandardError.ReadToEnd();
                }
            }

            catch (Exception ex)
            {
                returnObject.Exception = ex;
                return returnObject;
            }

            finally
            {
                proc.Dispose();
            }


            return returnObject;
        }

        /// <summary>
        /// Represents the result of executing an external process, including its exit code, output streams, and any
        /// exception encountered.
        /// </summary>
        /// <remarks>Use this class to access details about a process execution, such as the command path,
        /// arguments, standard output, standard error, and whether the process completed successfully. The properties
        /// provide convenient checks for output presence and error conditions. This type is typically used to capture
        /// and inspect the outcome of process invocations in automation or scripting scenarios.</remarks>
        public class ProcReturnInfo
        {
            public string Path = string.Empty;
            public string Arguments = string.Empty;
            public int ExitCode = -1;
            public string StandardOutput = string.Empty;
            public string StandardError = string.Empty;
            public Exception? Exception = null;
            public bool Success
            {
                get
                {
                    return ExitCode == 0 && Exception == null;
                }
            }

            public bool HasSTDOut
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(StandardOutput);
                }
            }

            public bool HasSTDErr
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(StandardError);
                }
            }

            public bool HasException
            {
                get
                {
                    return Exception != null;
                }
            }
        }

        /// <summary>
        /// Relaunches the Cmdlet with elevated (administrator) privileges, passing along the original commandline
        /// arguments. This method supports and prefers various sudo implementations if available, otherwise it will
        /// use the standard Windows ShellExecute &amp; UAC prompt. The relaunch is performed via a temporary PowerShell script
        /// which reimports this module and executes the original command, then deletes itself.
        /// </summary>
        /// <remarks>
        /// This method creates a temporary PowerShell script using the original provided commandline args, and
        /// attempts to relaunch the PowerShell host as an administrator, and then run the temporary script. The relaunch behavior may vary
        /// depending on the availability and/or type of sudo implementation, if available. The temporary script file is deleted after
        /// the relaunch attempt. This <i>may</i> set off false positives with particularly sensitive/paranoid virus scanners since it's
        /// running a script from $env:TEMP, and the dumber heuristics engines which operate primarily or entirely off of just heuristics
        /// instead of heuristics + behavioral or actual intent will absolutely flip their shit about that.
        /// </remarks>
        /// <param name="patientInfo">A dictionary containing info required to relaunch process, like the original host application,
        /// current directory, and the original commandline arguments.</param>
        /// <exception cref="ArgumentNullException">Thrown if the current process path cannot be determined.</exception>
        public static void RelaunchAsAdmin(Dictionary<string, dynamic> patientInfo)
        {
            string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            string argsAsString = String.Empty;

            foreach (string arg in args)
            {
                argsAsString += " " + arg;
            }

            try
            {
                var exeName = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(exeName))
                {
                    throw new ArgumentNullException("Cannot get current process path");
                }

                StringBuilder launchArgsBuilder = new StringBuilder();
                StringBuilder scriptFile = new StringBuilder();

                string tempScript = System.IO.Path.GetTempPath() + "temp_elevate.ps1";
                scriptFile.Append($"Import-Module \"{patientInfo["OfficeLocation"]}\" \n{patientInfo["ConsultInfo"]}");
                System.IO.File.WriteAllText(tempScript, scriptFile.ToString());
                string launchPath = String.Empty;
                string verb = String.Empty;
                bool useShellExecute = false;

                if (Statics.SudoAvailable)
                {
                    Dictionary<string, dynamic> sudoInfo = Statics.GetSudoInfo();
                    ;
                    ;

                    launchPath = sudoInfo["Path"];

                    if (sudoInfo["Flavor"] == "microsoft")
                    {
                        launchArgsBuilder.Append("--preserve-env --inline ");
                    }
                    else if (sudoInfo["Flavor"] == "gsudo")
                    {
                        launchArgsBuilder.Append("--keepShell --keepWindow --copyev --direct ");
                    }
                    else
                    {
                        // Don't know what this sudo implementation is, spray & pray
                        launchArgsBuilder.Append(" ");
                    }

                    launchArgsBuilder
                        .Append("--chdir ")
                        .Append(patientInfo["CurrentLocation"] + " ")
                        .Append($"\"{exeName}\" ");
                }

                else
                {
                    launchPath = $"\"{exeName}\"";
                    verb = "runas";
                    useShellExecute = true;
                }

                launchArgsBuilder
                        .Append(argsAsString + $" -File {tempScript}");

                ProcReturnInfo returnInfo;

                returnInfo = StartProcess(
                    path: launchPath,
                    arguments: launchArgsBuilder.ToString(),
                    useShellExecute: useShellExecute,
                    redirectStandardInput: false,
                    redirectStandardError: false,
                    redirectStandardOutput: false,
                    createNoWindow: false,
                    verb: verb
                );



                Thread.Sleep(5000);

                System.IO.File.Delete(tempScript);

            }
            catch (Exception ex)
            {
                throw;
            }

            return;
        }
    }
}
