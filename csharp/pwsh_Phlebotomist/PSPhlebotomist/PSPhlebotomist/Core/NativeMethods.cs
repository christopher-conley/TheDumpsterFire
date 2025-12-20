using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PSPhlebotomist.Core
{
    /// <summary>
    /// P/Invoke declarations for Windows API functions used in DLL injection.
    /// All methods use Unicode (W) variants for full path support.
    /// </summary>
    internal class NativeMethods
    {
        // Process access rights

        /// <summary>
        /// Specifies the access right to create a thread in a process. Used with process access control APIs.
        /// </summary>
        /// <remarks>This constant is typically used when calling native Windows API functions, such as
        /// OpenProcess, to request permission to create threads in the target process. The value corresponds to the
        /// PROCESS_CREATE_THREAD right defined by the Windows API.</remarks>
        public const uint PROCESS_CREATE_THREAD = 0x0002;

        /// <summary>
        /// Specifies the access right required to query information about a process object on Windows systems.
        /// </summary>
        /// <remarks>This constant is typically used with native Windows API calls, such as OpenProcess,
        /// to request permission to retrieve information about a process. It does not grant permission to read or
        /// modify the process's memory.</remarks>
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;

        /// <summary>
        /// Specifies the access right required to perform memory operations in a process, such as reading or writing to
        /// its address space.
        /// </summary>
        /// <remarks>This constant is typically used with Windows API functions that require process
        /// access rights, such as OpenProcess. It enables operations like VirtualAllocEx and WriteProcessMemory on the
        /// target process.</remarks>
        public const uint PROCESS_VM_OPERATION = 0x0008;

        /// <summary>
        /// Specifies the access right required to write to the memory of a process when calling native Windows APIs.
        /// </summary>
        /// <remarks>This constant is typically used with functions such as OpenProcess to request
        /// permission to modify the memory of another process. It corresponds to the PROCESS_VM_WRITE access flag
        /// defined in the Windows API.</remarks>
        public const uint PROCESS_VM_WRITE = 0x0020;

        /// <summary>
        /// Represents the access right required to read the memory of a process using Windows API functions.
        /// </summary>
        /// <remarks>Use this constant when calling native methods such as OpenProcess to specify that the
        /// process handle should allow reading from the process's memory. This value corresponds to the PROCESS_VM_READ
        /// access flag defined by the Windows operating system.</remarks>
        public const uint PROCESS_VM_READ = 0x0010;

        /// <summary>
        /// Specifies a combination of the access rights above, which are required to perform all possible operations on a process.
        /// </summary>
        /// <remarks>This constant combines multiple process access flags, including creating threads,
        /// querying information, and reading or writing to the process's memory. It is typically used when calling
        /// native Windows API functions that require process access rights, such as OpenProcess.</remarks>
        public const uint PROCESS_ALL_ACCESS =
                          PROCESS_CREATE_THREAD |
                          PROCESS_QUERY_INFORMATION |
                          PROCESS_VM_OPERATION |
                          PROCESS_VM_WRITE |
                          PROCESS_VM_READ;

        // Memory allocation types

        /// <summary>
        /// Specifies the memory allocation type indicating that committed pages are to be allocated in memory.
        /// </summary>
        /// <remarks>Use this constant with memory management functions to request that physical storage
        /// is allocated for the specified memory pages. This value is commonly used with Windows API calls such as
        /// VirtualAlloc.</remarks>
        public const uint MEM_COMMIT = 0x1000;

        /// <summary>
        /// Specifies the memory allocation type constant used to reserve a region of address space without allocating
        /// physical storage in the Windows API.
        /// </summary>
        /// <remarks>Use this constant with memory management functions such as VirtualAlloc to indicate
        /// that the specified memory region should be reserved but not committed. This value corresponds to the
        /// MEM_RESERVE flag defined in the Windows API.</remarks>
        public const uint MEM_RESERVE = 0x2000;

        /// <summary>
        /// Specifies the memory release option for use with memory management functions in Windows API calls.
        /// </summary>
        /// <remarks>Use this constant with functions such as VirtualFree to indicate that the specified
        /// memory region should be released. This value corresponds to the MEM_RELEASE flag defined in the Windows
        /// API.</remarks>
        public const uint MEM_RELEASE = 0x8000;

        // Memory protection constants

        /// <summary>
        /// Specifies a memory protection constant that enables read and write access to a memory region.
        /// </summary>
        /// <remarks>Use this constant when calling memory management functions that require protection
        /// flags, such as those in interop scenarios with native Windows APIs. The value corresponds to the
        /// PAGE_READWRITE flag defined in the Windows API.</remarks>
        public const uint PAGE_READWRITE = 0x04;

        // Wait constants

        /// <summary>
        /// Represents an infinite timeout value for wait operations.
        /// </summary>
        /// <remarks>Use this constant to specify that a wait operation should not time out. This value is
        /// commonly used with methods that accept a timeout parameter, indicating that the method should wait
        /// indefinitely until the operation completes.</remarks>
        public const uint INFINITE = 0xFFFFFFFF;

        /// <summary>
        /// Opens an existing process and returns a handle that can be used to interact with the process according to
        /// the specified access rights.
        /// </summary>
        /// <remarks>If the function fails, call <see cref="Marshal.GetLastWin32Error"/> to retrieve the
        /// error code. The caller is responsible for closing the returned handle using the appropriate API (such as
        /// CloseHandle) when it is no longer needed.</remarks>
        /// <param name="dwDesiredAccess">A bitmask specifying the access rights requested for the process. This determines the permitted operations
        /// on the returned handle. Refer to the Windows API documentation for valid access flags.</param>
        /// <param name="bInheritHandle">Indicates whether the returned handle is inheritable by child processes. Specify <see langword="true"/> to
        /// make the handle inheritable; otherwise, <see langword="false"/>.</param>
        /// <param name="dwProcessId">The identifier of the process to open. This must be the process ID of an existing process.</param>
        /// <returns>An <see cref="IntPtr"/> representing a handle to the opened process. If the function fails, the return value
        /// is <see cref="IntPtr.Zero"/>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId);

        /// <summary>
        /// Reserves, commits, or changes the protection of a region of memory within the virtual address space of a
        /// specified process.
        /// </summary>
        /// <remarks>This method is a P/Invoke signature for the Windows API function VirtualAllocEx. Use
        /// Marshal.GetLastWin32Error to retrieve extended error information if the function fails. Allocated memory
        /// must be released using VirtualFreeEx. This method is not thread-safe and should be used with caution in
        /// multi-threaded scenarios.</remarks>
        /// <param name="hProcess">A handle to the process in which to allocate memory. The handle must have the PROCESS_VM_OPERATION access
        /// right.</param>
        /// <param name="lpAddress">The desired starting address of the region to allocate. If zero, the system determines the address.</param>
        /// <param name="dwSize">The size of the memory region to allocate, in bytes. Must be greater than zero.</param>
        /// <param name="flAllocationType">The type of memory allocation to perform. This parameter can be a combination of allocation type flags such
        /// as MEM_COMMIT or MEM_RESERVE.</param>
        /// <param name="flProtect">The memory protection for the region of pages to be allocated. Specify one of the memory protection
        /// constants, such as PAGE_READWRITE.</param>
        /// <returns>If successful, returns a pointer to the base address of the allocated region of memory. If the function
        /// fails, returns IntPtr.Zero.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        /// <summary>
        /// Writes data to the memory of a specified process.
        /// </summary>
        /// <remarks>If the method returns false, call GetLastError to obtain extended error information.
        /// This method is typically used in advanced scenarios such as debugging or inter-process communication, and
        /// requires appropriate permissions. Writing to another process's memory can cause instability or security
        /// risks if not used carefully.</remarks>
        /// <param name="hProcess">A handle to the process whose memory is to be modified. The handle must have write access to the process
        /// memory.</param>
        /// <param name="lpBaseAddress">A pointer to the base address in the specified process where the data will be written.</param>
        /// <param name="lpBuffer">An array of bytes containing the data to be written to the process memory. Cannot be null.</param>
        /// <param name="nSize">The number of bytes to write from the buffer to the specified process memory.</param>
        /// <param name="lpNumberOfBytesWritten">When the method returns, contains the actual number of bytes written to the process memory.</param>
        /// <returns>true if the operation succeeds; otherwise, false.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out int lpNumberOfBytesWritten);

        /// <summary>
        /// Creates a thread in the virtual address space of a specified process. This is the magic method that
        /// allows for DLL/PE image injection and remote code execution within another process.
        /// </summary>
        /// <remarks>This method is a P/Invoke signature for the Windows API CreateRemoteThread function.
        /// The returned thread handle must be closed using CloseHandle when it is no longer needed.</remarks>
        /// <param name="hProcess">A handle to the process in which the thread is to be created. The handle must have the
        /// PROCESS_CREATE_THREAD, PROCESS_QUERY_INFORMATION, PROCESS_VM_OPERATION, PROCESS_VM_WRITE, and
        /// PROCESS_VM_READ access rights.</param>
        /// <param name="lpThreadAttributes">A pointer to a SECURITY_ATTRIBUTES structure that determines whether the returned handle can be inherited by
        /// child processes. If this parameter is IntPtr.Zero, the handle cannot be inherited.</param>
        /// <param name="dwStackSize">The initial size of the stack, in bytes. If this parameter is zero, the default stack size for the
        /// executable is used.</param>
        /// <param name="lpStartAddress">A pointer to the application-defined function to be executed by the thread. This address must be valid in
        /// the context of the target process.</param>
        /// <param name="lpParameter">A pointer to a variable to be passed to the thread function. This value is supplied as the single argument
        /// to the thread function.</param>
        /// <param name="dwCreationFlags">Flags that control the creation of the thread. For example, use 0 to run the thread immediately, or
        /// CREATE_SUSPENDED to create the thread in a suspended state.</param>
        /// <param name="lpThreadId">When the function returns, contains the thread identifier of the newly created thread.</param>
        /// <returns>If the function succeeds, returns a handle to the newly created thread. If the function fails, returns
        /// IntPtr.Zero.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out IntPtr lpThreadId);

        /// <summary>
        /// Retrieves a handle to the specified module loaded into the address space of the calling process.
        /// </summary>
        /// <remarks>The returned handle can be used in subsequent calls to functions that require a
        /// module handle. If the specified module is not loaded in the calling process, the function returns <see
        /// cref="IntPtr.Zero"/> and the error code can be retrieved using <see cref="Marshal.GetLastWin32Error"/>. This
        /// function does not increment the module's reference count.</remarks>
        /// <param name="lpModuleName">The name of the module to retrieve the handle for. If this parameter is null, the function returns a handle
        /// to the file used to create the calling process.</param>
        /// <returns>An <see cref="IntPtr"/> representing the handle to the specified module if the function succeeds; otherwise,
        /// <see cref="IntPtr.Zero"/>.</returns>

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Retrieves the address of an exported function or variable from the specified DLL/PE image.
        /// </summary>
        /// <remarks>If the function or variable cannot be found, the method returns IntPtr.Zero. To
        /// obtain extended error information, call the GetLastError function. The returned address can be used with
        /// delegates or marshaling to invoke the function from managed code. This method is typically used in 
        /// scenarios involving interoperability with native code, like shoving our DLL into its address space.</remarks>
        /// <param name="hModule">A handle to the DLL module that contains the function or variable. This handle must have been obtained by
        /// calling the LoadLibrary function.</param>
        /// <param name="lpProcName">The name of the function or variable to retrieve, as a null-terminated ANSI string. Alternatively, this
        /// parameter can be a function ordinal value cast to a string.</param>
        /// <returns>An IntPtr that contains the address of the specified function or variable if found; otherwise, IntPtr.Zero.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        /// <summary>
        /// Waits until the specified object is in the signaled state or the timeout interval elapses.
        /// </summary>
        /// <remarks>This method is used to wait for synchronization objects, like events, mutexes, or semaphores.
        /// The calling thread will be blocked until the object is signaled or the specified time-out period expires.
        /// If the function fails, call GetLastError to obtain extended error information.</remarks>
        /// <param name="hHandle">A handle to the object to be waited on. This handle must have been obtained from a function that returns a
        /// waitable object.</param>
        /// <param name="dwMilliseconds">The time-out interval, in milliseconds. Specify <see langword="0xFFFFFFFF"/> to wait indefinitely.</param>
        /// <returns>A value indicating the event that caused the function to return. Returns <see langword="0"/> if the object
        /// was signaled, <see langword="0x102"/> if the time-out interval elapsed, or other values for error
        /// conditions.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        /// <summary>
        /// Closes an open object handle, such as a file, process, thread, or other system resource, and releases the
        /// associated system resources.
        /// </summary>
        /// <remarks>If the function fails, call GetLastError to retrieve the error code. After a handle
        /// is closed, it is no longer valid and should not be used in subsequent API calls. Closing a handle that has
        /// already been closed or is invalid may result in undefined behavior.</remarks>
        /// <param name="hObject">A handle to an open object. This handle must have been obtained from a previous call to a Windows API
        /// function that returns a handle.</param>
        /// <returns>true if the function succeeds; otherwise, false.</returns>

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// Releases, decommits, or deletes a region of memory within the virtual address space of a specified process.
        /// </summary>
        /// <remarks>This method is a P/Invoke wrapper for the Windows API VirtualFreeEx function.
        /// If the method returns false, call Marshal.GetLastWin32Error() to obtain the error code. The behavior of the
        /// function depends on the value of dwFreeType.</remarks>
        /// <param name="hProcess">A handle to the process whose memory is to be freed. The handle must have the PROCESS_VM_OPERATION access
        /// right.</param>
        /// <param name="lpAddress">A pointer to the starting address of the region of memory to be freed. This address must be within the
        /// virtual address space of the specified process.</param>
        /// <param name="dwSize">The size, in bytes, of the region to be freed. If dwFreeType includes MEM_RELEASE, dwSize must be 0;
        /// otherwise, it specifies the size of the region to decommit.</param>
        /// <param name="dwFreeType">The type of free operation. This parameter can be MEM_DECOMMIT or MEM_RELEASE, and determines how the memory
        /// is freed.</param>
        /// <returns>true if the operation succeeds; otherwise, false.</returns>

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint dwFreeType);

        // I was gonna discern between x86 and x64 processes during injection to make sure
        // that the architecture of the PE image being injected matched the architecture of
        // the process, but then I got lazy and never implemented it. Leaving this here
        // for future reference if I ever get around to it
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);
    }
}
