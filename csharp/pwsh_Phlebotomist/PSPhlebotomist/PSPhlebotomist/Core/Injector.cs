using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PSPhlebotomist.Core
{
    /// <summary>
    /// Class containing methods for performing DLL/PE Image injection into a remote process.
    /// </summary>
    public class Injector
    {
        private readonly ILogger<Injector>? _logger;

        public Injector(ILogger<Injector> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Injector()
        {

        }

        /// <summary>
        /// Performs DLL/PE Image injection via LoadLibrary & CreateRemoteThread.
        /// Gets a handle to the target process and calls InjectSingleImage, the method
        /// which actually does the work of injecting the image, for each PE image path provided
        /// </summary>
        public InjectionResult Venipuncture(int pid, List<string> dllPaths)
        {
            var result = new InjectionResult { TotalDlls = dllPaths.Count };

            var hProcess = NativeMethods.OpenProcess(
                NativeMethods.PROCESS_ALL_ACCESS,
                false,
                pid);

            if (hProcess == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                _logger?.LogError("Failed to open process! Error code: {ErrorCode}", error);

                foreach (var dllPath in dllPaths)
                {
                    result.AddFailure(dllPath);
                }

                return result;
            }

            try
            {
                foreach (var dllPath in dllPaths)
                {
                    if (!InjectSingleImage(hProcess, dllPath, result))
                    {
                        _logger?.LogWarning("Failed to inject: {DllPath}", dllPath);
                    }
                }
            }
            finally
            {
                NativeMethods.CloseHandle(hProcess);
            }

            return result;
        }

        /// <summary>
        /// Attempts to inject a single PE image into the specified process via remote thread creation.
        /// </summary>
        /// <remarks>This method allocates memory in the target process, writes the DLL path, and creates
        /// a remote thread to load the DLL. This method ensures that any memory allocated during the injection attempt
        /// is released back to the OS, regardless of success or failure. The caller is responsible for ensuring that the
        /// process handle remains valid for the duration of the operation.
        /// </remarks>
        /// <param name="hProcess">A handle to the target process into which the DLL will be injected. The handle must have appropriate access
        /// rights for memory allocation and thread creation.</param>
        /// <param name="dllPath">The full path to the PE image to inject. The path must be valid and accessible from the calling process.</param>
        /// <param name="result">An object that records the outcome of the injection attempt, including success or failure details for the
        /// injected image.</param>
        /// <returns>true if the image was successfully injected into the target process; otherwise, false.</returns>
        private bool InjectSingleImage(IntPtr hProcess, string dllPath, InjectionResult result)
        {
            IntPtr allocMem = IntPtr.Zero;

            try
            {
                // Convert path to bytes (Unicode)
                byte[] pathBytes = Encoding.Unicode.GetBytes(dllPath + "\0");
                uint size = (uint)pathBytes.Length;

                // Allocate memory in target process
                allocMem = NativeMethods.VirtualAllocEx(
                    hProcess,
                    IntPtr.Zero,
                    size,
                    NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE,
                    NativeMethods.PAGE_READWRITE);

                if (allocMem == IntPtr.Zero)
                {
                    _logger?.LogError("Failed to allocate memory in target process!");
                    result.AddFailure(dllPath);
                    return false;
                }

                // Write DLL path to allocated memory
                if (!NativeMethods.WriteProcessMemory(
                    hProcess,
                    allocMem,
                    pathBytes,
                    size,
                    out _))
                {
                    _logger?.LogError("Failed to write process memory!");
                    result.AddFailure(dllPath);
                    return false;
                }

                // Get LoadLibraryW address from kernel32.dll
                var hKernel32 = NativeMethods.GetModuleHandle("kernel32.dll");
                if (hKernel32 == IntPtr.Zero)
                {
                    _logger?.LogError("Failed to get handle for kernel32.dll!");
                    result.AddFailure(dllPath);
                    return false;
                }

                var loadLibraryAddr = NativeMethods.GetProcAddress(hKernel32, "LoadLibraryW");
                if (loadLibraryAddr == IntPtr.Zero)
                {
                    _logger?.LogError("Failed to get LoadLibraryW address!");
                    result.AddFailure(dllPath);
                    return false;
                }

                // Create remote thread to call LoadLibraryW
                var hThread = NativeMethods.CreateRemoteThread(
                    hProcess,
                    IntPtr.Zero,
                    0,
                    loadLibraryAddr,
                    allocMem,
                    0,
                    out _);

                if (hThread == IntPtr.Zero)
                {
                    _logger?.LogError("Failed to create remote thread!");
                    result.AddFailure(dllPath);
                    return false;
                }

                // Wait for thread to complete
                NativeMethods.WaitForSingleObject(hThread, NativeMethods.INFINITE);
                NativeMethods.CloseHandle(hThread);

                result.AddSuccess(dllPath);
                _logger?.LogInformation("Successfully injected: {DllPath}", dllPath);
                return true;
            }
            finally
            {
                // Always clean up allocated memory
                if (allocMem != IntPtr.Zero)
                {
                    NativeMethods.VirtualFreeEx(hProcess, allocMem, 0, NativeMethods.MEM_RELEASE);
                }
            }
        }

        /// <summary>
        /// Initializes the module when the assembly is loaded.
        /// </summary>
        /// <remarks>This method is automatically invoked by the runtime before any code in the assembly
        /// is executed. It should be used to perform one-time setup or initialization required for the module. Do not
        /// call this method directly.</remarks>
        [ModuleInitializer]
        public static void ModInit()
        {
            //new Injector();
            return;
        }
    }
}
