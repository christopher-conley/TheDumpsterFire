using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PSPhlebotomist.Common;
using PSPhlebotomist.Core;
using PSPhlebotomist.Helpers;
using PSPhlebotomist.Utils;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Text;

namespace PSPhlebotomist.PSCmdlets
{
    /// <summary>
    /// Provides a base class for PowerShell cmdlets that support injection operations and logging functionality.
    /// </summary>
    /// <remarks>This class serves as a foundation for cmdlets that require dependency injection and logging
    /// capabilities. It manages logger instances and provides access to related cmdlet components. Derived cmdlets can
    /// utilize the provided logging infrastructure and extend injection-related functionality as needed.</remarks>
    public class InoculatorBasePSCmdlet : PSCmdlet
    {
        protected internal ILogger<InoculatorBasePSCmdlet>? _logger;
        protected internal ILogger<DIContainer>? DILogger = SyringeSingletons.CmdletDIContainer?.Logger;
        protected internal InsertNeedleCmdlet? BaseActiveInsertNeedleCmdlet;

        protected internal ILogger<InoculatorBasePSCmdlet>? BaseLogger
        {
            get => _logger;
            set => _logger = value;
        }
        public InoculatorBasePSCmdlet(ILogger<InoculatorBasePSCmdlet> logger)
        {
            _logger = logger;
        }

        public InoculatorBasePSCmdlet()
        {
            _logger ??= SyringeSingletons.CmdletDIContainer?.GenericHost?.Services.GetRequiredService<ILogger<InoculatorBasePSCmdlet>>();
        }

        /// <summary>
        /// Initializes the module when the assembly is loaded.
        /// </summary>
        /// <remarks>This method is automatically invoked by the runtime before any code in the containing
        /// assembly is executed. It should not be called directly.</remarks>
        [ModuleInitializer]
        public static void InitMod()
        {
            ;
            _ = Utils.Functions.FormatObject("Do I get touched now?");
        }

        /// <summary>
        /// Provides functionality for configuring and performing DLL injection into a target process, including
        /// validation of DLL files, process targeting, process validation, and ultimately the injection(s) themselves.
        /// </summary>
        /// <remarks>The Syringe class encapsulates the state and operations required to inject one or
        /// more DLLs into a process, supporting both process name and process ID targeting. It includes methods for
        /// validating DLL paths, preparing and resetting injection parameters, and executing the injection. Instances
        /// of Syringe should be disposed after use to release resources, and so you don't accidentally poke yourself.
        /// This class is intended for internal use within injection workflows and is not thread-safe.</remarks>
        protected internal class Syringe : IDisposable
        {
            private bool disposedValue;

            /// <summary>
            /// Gets or sets the collection of file paths to the DLLs to be injected.
            /// </summary>
            public string[] DllPaths { get; internal set; } = [];

            /// <summary>
            /// Gets the type of injection to be used for the current instance (either "pname" for process name or "pid" for process ID).
            /// </summary>
            public string InjectType { get; internal set; } = String.Empty;

            /// <summary>
            /// Gets the name of the target process for injection when using process name targeting.
            /// </summary>
            public string ProcessName { get; internal set; } = String.Empty;

            /// <summary>
            /// Gets the native process identifier associated with the target process.
            /// </summary>
            public nuint ProcessId { get; internal set; } = (nuint)Functions.NumGetFloor(Functions.GetArchUnsignedIntType());

            /// <summary>
            /// Gets a value indicating whether the Cmdlet should wait for the target process to launch if it is not already running.
            /// This value is only relevant when using process name targeting.
            /// </summary>
            public bool ShouldWait { get; internal set; } = false;

            /// <summary>
            /// Gets the timeout value, in platform-dependent units, used when waiting for the target process to launch.
            /// </summary>
            /// <remarks>The timeout is determined based on the architecture's unsigned integer type
            /// and may vary between platforms, but the default is a real big fuckin' number.</remarks>
            public nuint Timeout { get; internal set; } = (nuint)Functions.NumGetCeiling(Functions.GetArchUnsignedIntType());

            public Syringe()
            {

            }

            // This is actually a validation function to ensure that the
            // images to inject actually exist and are valid PE files. So unlike
            // the functions below it, this one actually does something useful

            /// <summary>
            /// Validates that all DLL paths specified in the IntakeForm class object exist and reference valid PE files before
            /// proceeding with an injection attempt.
            /// </summary>
            /// <remarks>This method performs validation on the provided DLL paths to ensure they are
            /// suitable for injection. It does not perform the injection itself, but must be called prior to any
            /// injection operations to proactively prevent runtime errors.</remarks>
            /// <param name="tieOff">An IntakeForm object containing the collection of DLL paths to validate. Each path should reference an
            /// existing file intended for injection.</param>
            /// <returns>The current <see cref="Syringe"/> instance if all DLL paths are valid and reference valid PE files.</returns>
            /// <exception cref="ArgumentException">Thrown if one or more DLL paths in <paramref name="tieOff"/> are invalid or do not reference valid PE
            /// files.</exception>
            protected internal Syringe FindVein(IntakeForm tieOff)
            {
                bool shouldhrow = false;
                Dictionary<string, bool> dllResults = [];

                foreach (string dllPath in tieOff.DllPaths)
                {
                    bool goodPath = SyringeSingletons.SharedValidationHelper.IsValidDll(dllPath) || false;
                    bool goodPE = SyringeSingletons.SharedValidationHelper.IsValidPEFile(dllPath) || false;
                    dllResults.Add(dllPath, goodPath && goodPE);

                    if (!dllResults[dllPath])
                    {
                        shouldhrow = true;
                    }
                }

                if (shouldhrow)
                {
                    StringBuilder errMsg = new StringBuilder();
                    errMsg.AppendLine("One or more of the specified DLL paths are invalid or do not point to valid PE files: ");
                    foreach (KeyValuePair<string, bool> kvp in dllResults)
                    {
                        if (!kvp.Value)
                        {
                            errMsg.AppendLine($" - {kvp.Key}");
                        }
                    }
                    throw new ArgumentException(errMsg.ToString());
                }

                return this;
            }

            /// <summary>
            /// Attempts to inject one or more DLLs into a target process as specified by the provided validated IntakeForm object, and
            /// returns a "diagnosis" of the injection attempt(s).
            /// </summary>
            /// <remarks>The injection may be performed by process name or process ID, depending on
            /// the IntakeForm configuration. The method blocks until the target process is available if -W/-Wait is specified when the
            /// Cmdlet is called or chosen in interactive mode. The return object provides detailed feedback for each DLL injection attempt.</remarks>
            /// <param name="inoculation">An IntakeForm object containing details about the target process, injection type, DLL paths, and related
            /// options. Must specify valid process information (process name or ID) and at least one DLL path.</param>
            /// <returns>A PatientDiagnosis object containing the results of the injection attempt, including success status,
            /// counts of attempted and successful injections, and detailed status information for each DLL.</returns>
            /// <exception cref="ArgumentException">Thrown if the target process cannot be found based on the information provided in the IntakeForm object.</exception>
            internal PatientDiagnosis Inject(IntakeForm inoculation)
            {
                List<string> dllList = [];

                foreach (string dllPath in inoculation.DllPaths)
                {
                    dllList.Add(dllPath);
                }

                int pid = 0;

                if (inoculation.InjectType == "pname")
                {
                    if (inoculation.ShouldWait)
                    {
                        pid = SyringeSingletons.SharedProcessHelper.AwaitProcessAsync(inoculation.ProcessName, (int)inoculation.Timeout).GetAwaiter().GetResult();
                    }
                    else
                    {
                        pid = SyringeSingletons.SharedProcessHelper.GetProcessIdByName(inoculation.ProcessName);
                    }

                    inoculation.ProcessId = (nuint)pid;
                }
                else
                {
                    pid = (int)inoculation.ProcessId;
                }

                if (pid == 0)
                {
                    throw new ArgumentException("Target process not found.");
                }

                Injector hypo = SyringeSingletons.SharedInjector;
                InjectionResult result = hypo.Venipuncture((int)inoculation.ProcessId, dllList);

                PatientDiagnosis diagnosis = new()
                {
                    InoculationSuccessful = result.TotalFailures == 0,
                    InoculumsAttempted = result.TotalDlls,
                    InoculumsSucceeded = result.TotalSuccess,
                    InoculumsFailed = result.TotalFailures,
                    InoculumsDetail = result.InjectionStatus,
                    PayloadPlural = result.PayloadPlural,
                    FailurePlural = result.FailurePlural

                };

                return diagnosis;
            }

            // Yes, this function is 100% useless and is essentially just a
            // shitty default constructor that's zeroing out class properties that
            // already have a value, but it's fun to lean into the
            // whole syringe/injection metaphor

            /// <summary>
            /// Resets the Syringe instance to its default state by clearing all configuration properties.
            /// </summary>
            /// <remarks>This only exists for the long chain-method bit in the main Cmdlet. It's nothing more
            /// than a default constructor that isn't actually a constructor.</remarks>
            /// <returns>The current Syringe instance with all properties set to their default values.</returns>
            protected internal Syringe Sanitize()
            {
                DllPaths = [];
                InjectType = String.Empty;
                ProcessName = String.Empty;
                ProcessId = (nuint)Functions.NumGetFloor(Functions.GetArchUnsignedIntType());
                ShouldWait = false;
                Timeout = (nuint)Functions.NumGetCeiling(Functions.GetArchUnsignedIntType());

                return this;
            }

            // This one is useless too since it's also just a
            // shitty-constructor-that-actually-isn't-a-constructor, but
            // at least it has parameters

            /// <summary>
            /// Configures the syringe instance with the specified DLLs, injection type, target process information, and
            /// injection options.
            /// </summary>
            /// <param name="dllPaths">An array of file paths to the DLLs to be injected into the target process. Each path must refer to a
            /// valid DLL file.</param>
            /// <param name="injectType">The type of injection to perform. This value determines the injection method used.</param>
            /// <param name="processName">The name of the target process into which the DLLs will be injected.</param>
            /// <param name="processId">The Process ID of the target process.</param>
            /// <param name="shouldWait">Indicates whether the Cmdlet should wait for the target process to launch and then attempt injection
            /// at the first opportunity. Specify <see langword="true"/> to wait; otherwise, <see langword="false"/>. A value of <see langword="false"/>
            /// will result in an immediate injection attempt which will fail if the target process is not currently running.</param>
            /// <param name="timeout">The maximum time, in seconds, to wait for the target process to launch. Only valid when specified in conjunction with
            /// the -W/-Wait parameter and is ignored otherwise. Must be greater than zero if <paramref name="shouldWait"/> is
            /// <see langword="true"/>.</param>
            /// <returns>The current syringe instance configured with the specified parameters.</returns>
            protected internal Syringe Prepare(
                string[] dllPaths,
                string injectType,
                string processName,
                nuint processId,
                bool shouldWait,
                nuint timeout
                )
            {
                DllPaths = dllPaths;
                InjectType = injectType;
                ProcessName = processName;
                ProcessId = processId;
                ShouldWait = ShouldWait;
                Timeout = timeout;

                return this;
            }

            // This is just a dumb little wrapper function so that I can
            // call it after the functions that ACTUALLY DO ANY FUCKING WORK
            // IN THIS GODDAMNED CMDLET, and have it look like:
            //
            // Dispose(biohazard: true, container: "Sharps")
            //
            // I guess I'm just all-in on the bit at this point
            protected internal void Dispose(bool biohazard, string container)
            {
                Dispose(biohazard);
            }
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects)
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                    // TODO: set large fields to null
                    disposedValue = true;
                }
            }

            // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
            // ~Syringe()
            // {
            //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

    }
}
