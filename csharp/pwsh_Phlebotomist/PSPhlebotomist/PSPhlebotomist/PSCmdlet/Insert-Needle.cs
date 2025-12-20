using Microsoft.Extensions.Logging;
using PSPhlebotomist.Common;
using PSPhlebotomist.Core;
using PSPhlebotomist.Helpers;
using Spectre.Console;
using System.Management.Automation;
using System.Reflection;
using static PSPhlebotomist.Common.Statics;
using static PSPhlebotomist.Common.Statics.Functions;

namespace PSPhlebotomist.PSCmdlets
{

    /// <summary>
    /// Provides a PowerShell cmdlet for injecting one or more DLLs into a target process, supporting both process ID
    /// and process name selection, optional elevation, and interactive or automated operation.
    /// </summary>
    /// <remarks>This cmdlet can be invoked using several aliases, including 'New-Syringe', 'New-Injection',
    /// 'New-Patient', and 'Insert-Needle', because at one point I decided that I'm all-in on the bit.
    /// It supports both automated and interactive modes: if no parameters are provided, or only the -A/-Admin parameter
    /// is provided, the cmdlet will prompt for input interactively. The cmdlet will attempt to elevate privileges and
    /// relaunch with the original commandline args if the 'Admin' switch is specified and it is not already running
    /// within an Administrator security context. If already running within an Administrator security context, the
    /// -A/-Admin switch is ignored and the normal injection workflow and logic continues. This Cmdlet prefers to use an
    /// implmentation of sudo to elevate privileges if available, otherwise it will use the standard Windows process
    /// launch and UAC prompt/runas flow.
    /// 
    /// <br></br><br></br>Output is a PatientDiagnosis object representing the result of the injection attempt(s). For best
    /// results, ensure that the target process is accessible and that DLL paths are valid. Thread safety is not guaranteed; concurrent
    /// invocations on the same process should probably be avoided, although multiple DLL injections <i>within the same Cmdlet call</i>
    /// are a native and supported feature. Use the 'Wait' switch to block and wait for the process to
    /// launch (only valid when using Process Name, not PID), and 'Timeout' to specify the maximum process launch wait
    /// duration in seconds.</remarks>
    [Alias("New-Syringe", "New-Injection", "New-Patient", "Insert-Needle")]
    [Cmdlet(VerbsCommon.New, "Needle", DefaultParameterSetName = "default")]
    [OutputType(typeof(PatientDiagnosis))]

    public class InsertNeedleCmdlet : InoculatorBasePSCmdlet
    {
        private readonly ILogger<InsertNeedleCmdlet>? _cmdletLogger = GetRequiredService<ILogger<InsertNeedleCmdlet>>();
        private bool NoAppointment = false;
        private bool NeedsExternalConsult = false;
        public PatientDiagnosis? Diagnosis;

        private string[] _inject = { };
        private nint _pid;
        private string _name = string.Empty;
        private SwitchParameter _wait;
        private nint _timeout;
        private SwitchParameter _admin;

        protected internal IntakeForm patientStates = new IntakeForm();
        protected internal ILogger<InsertNeedleCmdlet>? CmdletLogger { get => _cmdletLogger; }

        /// <summary>
        /// Gets or sets an array of file paths to DLL files that will be injected into the target process.
        /// </summary>
        /// <remarks>This parameter accepts multiple DLL paths and can be provided via the pipeline or by property name.
        /// If no DLLs are specified, the cmdlet will enter interactive mode to prompt for input. Paths should be valid
        /// and accessible to the current user context.</remarks>
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Inject
        {
            get
            {
                _inject ??= new string[] { };
                return _inject;
            }
            protected internal set => _inject = value;
        }

        /// <summary>
        /// Gets or sets the process ID (PID) of the target process to inject DLLs into.
        /// </summary>
        /// <remarks>This parameter is mandatory when using the 'pid' parameter set. It cannot be used simultaneously
        /// with the Name parameter. The process must be accessible and running for injection to succeed. This parameter
        /// can be referenced by the aliases 'ProcessId' or 'Id'.</remarks>
        [Alias("ProcessId", "Id")]
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "pid"
            )]
        public nint PID { get; protected internal set; } = Statics.INT_FLOOR;

        /// <summary>
        /// Gets or sets the name of the target process to inject DLLs into.
        /// </summary>
        /// <remarks>This parameter is used with the 'pname' parameter set. It cannot be used simultaneously with the
        /// PID parameter. When combined with the Wait switch, the cmdlet will wait for a process with this name to launch
        /// before attempting injection. This parameter can be referenced by the aliases 'ProcessName' or 'PName'.</remarks>
        [Alias("ProcessName", "PName")]
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "pname"
            )]
        public string Name
        {
            get
            {
                _name ??= string.Empty;
                return _name;
            }
            protected internal set => _name = value;
        }

        /// <summary>
        /// Gets or sets a switch indicating whether the cmdlet should wait for the target process to launch before
        /// attempting injection.
        /// </summary>
        /// <remarks>This switch is only valid when using the Name parameter to specify a process by name. When enabled,
        /// the cmdlet will block and monitor for a process with the specified name to start, up to the timeout duration.
        /// This is useful for injecting into processes that are about to be launched.</remarks>
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Wait { get => _wait; protected internal set => _wait = value; }

        /// <summary>
        /// Gets or sets the maximum time in seconds to wait for a process to launch when using the Wait switch.
        /// </summary>
        /// <remarks>This parameter only applies when the Wait switch is enabled and a process name is specified. If the
        /// target process does not launch within the specified timeout period, the cmdlet will terminate with an error.
        /// The default value is the maximum unsigned integer value for the current architecture.</remarks>
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true)]

        public nuint Timeout { get; protected internal set; } = Statics.UINT_CEILING;

        /// <summary>
        /// Gets or sets a switch indicating whether the cmdlet should attempt to elevate privileges and relaunch as
        /// administrator.
        /// </summary>
        /// <remarks>When this switch is enabled and the cmdlet is not already running with administrator privileges, it
        /// will attempt to relaunch itself in an elevated context, preserving the original command-line arguments. The
        /// cmdlet prefers using a sudo implementation if available, otherwise it falls back to the standard UAC prompt.
        /// This parameter can be referenced by the aliases 'AsAdmin', 'Administrator', 'AsAdministrator', or 'Root'.</remarks>
        [Alias("AsAdmin", "Administrator", "AsAdministrator", "Root")]
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Admin { get => _admin; protected internal set => _admin = value; }

        /// <summary>
        /// Performs initialization tasks at the beginning of pipeline execution, including parameter validation and
        /// interactive mode detection.
        /// </summary>
        /// <remarks>This method is called once when the pipeline starts executing. It initializes logging, displays the
        /// cmdlet banner, determines whether interactive mode is required, and checks for administrator elevation requests.
        /// If insufficient parameters are provided, the cmdlet will flag for interactive mode prompting.</remarks>
        protected override void BeginProcessing()
        {
            AnsiConsole.Write(
                new FigletText("PSPhlebotomist")
                .Centered()
                .Color(Color.FromHex("CD2E3A")));
            AnsiConsole.WriteLine();

            base.BeginProcessing();
            BaseLogger ??= GetRequiredService<ILogger<InoculatorBasePSCmdlet>>();

            // This looks dumb, and that's because it is dumb, because Powershell's threading model can be dumb
            // If you're running on a different thread than the thread the Cmdlet that was called is on, even
            // if it's the same Cmdlet, you can't call the Write* methods, like WriteInformation, WriteVerbose, etc.,
            // or it'll throw a terminating error. So we have to do something dumb like this to get around that limitation.
            BaseActiveInsertNeedleCmdlet ??= this;

            var cmdletDefaults = Defaults.Cmdlets.GetDefaults<InsertNeedleCmdlet>();

            if (
                (PID == cmdletDefaults[nameof(PID)] && Name == cmdletDefaults[nameof(Name)]) ||
                (ParameterSetName != "pid" && ParameterSetName != "pname") ||
                Inject.Length == 0
                )
            {
                NoAppointment = true;
            }

            else
            {
                patientStates = new IntakeForm
                {
                    DllPaths = Inject,
                    ProcessId = (nuint)PID,
                    ProcessName = Name,
                    ShouldWait = Wait.IsPresent,
                    Timeout = Timeout,
                    InjectType = ParameterSetName,
                    ShouldInject = true,
                };

                patientStates.InjectType = ParameterSetName == "pname" ? "pname" : "pid";
            }

            NeedsExternalConsult = Admin.IsPresent;

        }

        /// <summary>
        /// Processes each input record from the pipeline, handling privilege elevation, interactive mode, and DLL
        /// injection operations.
        /// </summary>
        /// <remarks>This method is called for each input object received from the pipeline. If no input is received,
        /// the method is not called. It handles administrator elevation attempts, interactive mode prompting via the
        /// TriageNurse helper, and orchestrates the actual DLL injection workflow using the Syringe class. All
        /// exceptions are logged and re-thrown as terminating errors.</remarks>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            ;

            if (NeedsExternalConsult && !Statics.ConsultAuthorized)
            {
                Dictionary<string, dynamic> patientInfo = new Dictionary<string, dynamic>
                {
                    { "OfficeLocation", Assembly.GetExecutingAssembly().Location },
                    { "CurrentLocation", SessionState.Path.CurrentLocation.Path },
                    { "ConsultInfo", MyInvocation.Line }
                };

                try
                {
                    Utils.Functions.StartExternalConsult(patientInfo);
                }
                catch (Exception ex)
                {
                    CmdletLogger?.LogError("An error occurred while attempting to relaunch as admin: {Message}", ex);
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShowLinks);
                    throw;
                }

                WriteObject("");
                return;
            }


            if (NoAppointment)
            {
                TriageNurse nurseJane = SyringeSingletons.NurseJane;

                try
                {
                    patientStates = nurseJane.INeedHelp();
                }
                catch (Exception ex)
                {
                    CmdletLogger?.LogError("An error occurred during interactive mode: {Message}", ex);
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShowLinks);
                    throw;
                }
            }

            if (patientStates.ShouldInject)
            {
                // Very important to procure a new one!
                // Don't reuse sharps!
                Syringe Sharp = new Syringe();

                try
                {
                    Diagnosis = Sharp
                    .Sanitize()
                    .Prepare(
                        patientStates.DllPaths,
                        patientStates.InjectType,
                        patientStates.ProcessName,
                        patientStates.ProcessId,
                        patientStates.ShouldWait,
                        (nuint)patientStates.Timeout)

                    .FindVein(tieOff: patientStates)
                    .Inject(inoculation: patientStates);
                }

                catch (Exception ex)
                {
                    CmdletLogger?.LogError("An error occurred during injection: {Message}", ex);
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShowLinks);
                    throw;
                }

                finally
                {
                    Sharp.Dispose(
                        biohazard: true,
                        container: "Sharps"
                        );
                }
            }
            else
            {
                CmdletLogger?.LogInformation("User opted to cancel injection via interactive mode.");
                return;
            }
        }

        /// <summary>
        /// Performs cleanup tasks at the end of pipeline execution and outputs the final diagnosis result.
        /// </summary>
        /// <remarks>This method is called once at the end of pipeline execution. If no input was received during
        /// pipeline execution, this method is not called. It writes the PatientDiagnosis object to the output stream,
        /// containing the results of the injection attempt(s).</remarks>
        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteObject(Diagnosis);
        }

        /// <summary>
        /// Handles cleanup when the cmdlet is stopped prematurely by the user or system.
        /// </summary>
        /// <remarks>This method is called when the cmdlet is interrupted via Ctrl+C or when Stop-Pipeline is invoked.
        /// It ensures proper cleanup of resources and termination of ongoing operations.</remarks>
        protected override void StopProcessing()
        {
            base.StopProcessing();
        }
    }
}
