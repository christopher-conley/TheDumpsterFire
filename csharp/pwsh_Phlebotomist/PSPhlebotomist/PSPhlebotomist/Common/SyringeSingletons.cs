using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PSPhlebotomist.Core;
using PSPhlebotomist.Core.Helpers;
using PSPhlebotomist.Helpers;
using System.Runtime.CompilerServices;

// The only scenario in which these fields are null
// is before the module initializer runs. If they're null
// after that, there are bigger problems, like the fact
// that this Cmdlet can't run at all
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8602

namespace PSPhlebotomist.Common
{
    /// <summary>
    /// Provides shared singleton instances of core services and helpers used throughout the Cmdlet.
    /// </summary>
    /// <remarks>This class centralizes access to commonly used singleton objects, such as dependency
    /// injection containers, logging infrastructure, and helper utilities. All members are static and intended for
    /// internal use only. The class is initialized automatically at module load time.</remarks>
    internal static class SyringeSingletons
    {
        /// <summary>
        /// Provides a globally accessible dependency injection container for use by cmdlets.
        /// </summary>
        /// <remarks>This static field is intended to be used by cmdlets that require dependency
        /// resolution. It should be initialized before any cmdlet attempts to resolve dependencies.
        /// </remarks>
        public static DIContainer CmdletDIContainer;

        /// <summary>
        /// Provides a shared instance of an <see cref="ILoggerFactory"/> for use across the Cmdlet.
        /// </summary>
        /// <remarks>Use this factory to create logger instances that share configuration and resources.
        /// Sharing a single <see cref="ILoggerFactory"/> instance is recommended to ensure consistent logging behavior
        /// and to avoid unnecessary resource usage.</remarks>
        public static ILoggerFactory SharedLoggerFactory;

        /// <summary>
        /// Provides a globally accessible instance of the Injector class to perform DLL/PE image injection.
        /// </summary>
        /// <remarks>Use this field to access a common Injector instance across different parts of the
        /// Cmdlet. Modifying this field affects all consumers that rely on the shared Injector.</remarks>
        public static Injector SharedInjector;

        /// <summary>
        /// Provides a globally accessible instance of the NativeMethods class, which contains methods for interop with unmanaged code.
        /// </summary>
        /// <remarks>This field exposes a set of native methods that can be used throughout the
        /// Cmdlet, such as LoadLibrary, GetModuleHandle, etc.</remarks>
        public static NativeMethods SharedNativeMethods;

        /// <summary>
        /// Provides a shared instance of the ProcessHelper for use across the Cmdlet.
        /// </summary>
        /// <remarks>Use this field to access common process-related functionality without creating
        /// multiple instances of ProcessHelper.</remarks>
        public static ProcessHelper SharedProcessHelper;

        /// <summary>
        /// Provides a shared instance of the ValidationHelper for use across the Cmdlet.
        /// </summary>
        /// <remarks>Use this static field to access a common ValidationHelper instance when multiple
        /// components require consistent validation logic. This can help reduce resource usage and ensure uniform
        /// validation behavior.</remarks>
        public static ValidationHelper SharedValidationHelper;

        /// <summary>
        /// Represents the shared instance of the triage nurse named Jane.
        /// </summary>
        /// <remarks>This is an instance of the interactive mode helper class which is triggered to run when
        /// the Cmdlet is called with no parameters, or only the -A/-Admin parameter with no further parameters.</remarks>
        public static TriageNurse NurseJane;

        /// <summary>
        /// Represents the root Serilog logger instance from which all oher loggers should derive.
        /// </summary>
        /// <remarks>This logger is configured within the DI container at Cmdlet startup and is used
        /// as the base logger for all logging operations. Modifying this instance affects logging behavior throughout the
        /// Cmdlet.</remarks>
        public static Serilog.ILogger _rootLogger;

        /// <summary>
        /// Gets the root Serilog logger instance from which all oher loggers should derive.
        /// </summary>
        /// <remarks>Use this property to access the global logger for logging messages throughout the
        /// Cmdlet. This logger is configured within the DI container at Cmdlet startup and should be used for
        /// general, contextless logging needs. This property is thread-safe.</remarks>
        public static Serilog.ILogger RootLoggerInstance
        {
            get => _rootLogger!;
        }

        /// <summary>
        /// Gets a new instance of the <see cref="InjectionResult"/> class, which records the outcome of an injection operation.
        /// </summary>
        /// <remarks>Each access returns a new <see cref="InjectionResult"/> instance. I'm forgetful and might
        /// reuse an object, so this makes it easy to instantiate a new one. I actually forgot that I made this until
        /// I was documenting it just now, so I'm gonna go back and actually use it throughout the codebase.</remarks>
        public static InjectionResult SharedInjectionResult
        {
            get => new InjectionResult();
        }

        /// <summary>
        /// Initializes shared service instances and dependencies required by the module at Cmdlet startup.
        /// </summary>
        /// <remarks>This method is automatically invoked during module initialization due to the <see
        /// cref="ModuleInitializerAttribute"/>. It sets up core services such as logging, dependency injection, and
        /// helper utilities, making them available for use throughout the module and during bootstrapping. This method
        /// should not be called directly.</remarks>
        [ModuleInitializer]
        public static void Initialize()
        {
            CmdletDIContainer ??= new DIContainer();
            SharedLoggerFactory = CmdletDIContainer.GenericHost.Services.GetRequiredService<ILoggerFactory>();
            SharedInjector = CmdletDIContainer.GenericHost.Services.GetRequiredService<Injector>();
            SharedNativeMethods = CmdletDIContainer.GenericHost.Services.GetRequiredService<NativeMethods>();
            SharedProcessHelper = CmdletDIContainer.GenericHost.Services.GetRequiredService<ProcessHelper>();
            SharedValidationHelper = CmdletDIContainer.GenericHost.Services.GetRequiredService<ValidationHelper>();
            NurseJane = CmdletDIContainer.GenericHost.Services.GetRequiredService<TriageNurse>();
        }

    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS8602
