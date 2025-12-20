using System.Runtime.CompilerServices;

namespace PSPhlebotomist.Core
{
    /// <summary>
    /// Tracks the results of DLL injection operations, including success/failure counts and per-DLL status.
    /// Provides methods for recording injection outcomes and generating grammatically correct output strings.
    /// </summary>
    public class InjectionResult
    {
        /// <summary>Total number of DLLs attempted for injection.</summary>
        public int TotalDlls { get; set; }

        /// <summary>Number of successful injections.</summary>
        public int TotalSuccess { get; private set; }

        /// <summary>Number of failed injections.</summary>
        public int TotalFailures { get; private set; }

        /// <summary>Map of DLL paths to their injection status (true = success, false = failure).</summary>
        public Dictionary<string, bool> InjectionStatus { get; } = new();

        /// <summary>
        /// Records a successful DLL injection.
        /// </summary>
        /// <param name="dllPath">Path to the successfully injected DLL.</param>
        public InjectionResult AddSuccess(string dllPath)
        {
            TotalSuccess++;
            InjectionStatus[dllPath] = true;
            return this;
        }

        /// <summary>
        /// Records a failed DLL injection.
        /// </summary>
        /// <param name="dllPath">Path to the DLL that failed to inject.</param>
        public InjectionResult AddFailure(string dllPath)
        {
            TotalFailures++;
            InjectionStatus[dllPath] = false;
            return this;
        }

        /// <summary>
        /// Gets grammatically correct plural form for payloads.
        /// </summary>
        public string PayloadPlural => TotalSuccess == 1 ? "payload" : "payloads";

        /// <summary>
        /// Gets grammatically correct plural form for failures.
        /// </summary>
        public string FailurePlural => TotalFailures == 1 ? "failure" : "failures";

        /// <summary>
        /// Gets exit code based on failures (0 = all succeeded).
        /// </summary>
        public int ExitCode => TotalFailures;



        public InjectionResult()
        {

        }

        /// <summary>
        /// Initializes the module when the assembly is loaded, performing any required setup before other code
        /// executes.
        /// </summary>
        /// <remarks>This method is automatically invoked by the runtime due to the <see
        /// cref="System.Runtime.CompilerServices.ModuleInitializerAttribute"/>. It should not be called directly.</remarks>
        [ModuleInitializer]
        public static void ModInit()
        {
            new InjectionResult();
        }
    }
}
