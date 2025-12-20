using Microsoft.Extensions.Logging;
using PSPhlebotomist.Common;
using PSPhlebotomist.PSCmdlets;
using PSPhlebotomist.Utils;
using Spectre.Console;
using System.Text.RegularExpressions;
using static PSPhlebotomist.Common.Statics.ANSI;

namespace PSPhlebotomist.Helpers
{
    /// <summary>
    /// Provides interactive, console-based collection of user input required for process injection operations,
    /// including selection of injection type, target process, DLL/PE image path, and optional wait/timeout settings.
    /// This class is only invoked when no parameters are provided to the injection cmdlet, or only the -A/-Admin
    /// parameter is provided.
    /// </summary>
    /// <remarks>The TriageNurse class is used only in scenarios where user-driven, step-by-step input is needed
    /// to configure and initiate a process injection, as is the case when no parameters are provided to the Cmdlet
    /// at runtime. It guides the user through a series of prompts, validates responses (mostly), and assembles
    /// an IntakeForm class containing all necessary parameters for injection. Logging is performed throughout the interaction
    /// to assist with troubleshooting if necessary.</remarks>
    internal class TriageNurse : InsertNeedleCmdlet
    {
        new private readonly ILogger<TriageNurse>? _logger;
        private static readonly string injectTypeRegexString = Statics.StaticStrings.InjectTypeRegexString;
        //private Func<dynamic, bool> _modeSelectionFunction =
        private Func<object[], bool> _modeSelectionFunction =
            new((params object[] input) =>
            {
                Match match = Regex.Match(
                    input: (string)input[0],
                    pattern: injectTypeRegexString,
                    options: RegexOptions.IgnoreCase |
                             RegexOptions.CultureInvariant |
                             RegexOptions.Multiline
                    );
                return match.Success;
            });

        private Func<object[], bool> _targetSelectionFunction =
            //private Func<InteractiveModeResult, dynamic, bool> _targetSelectionFunction =
            //new((settings, input) => {
            new((params object[] input) =>
            {

                if (input is null)
                {
                    return false;
                }

                IntakeForm settings = (IntakeForm)input[1];
                dynamic inputObject;

                if (input[0].GetType() == typeof(string))
                {
                    inputObject = (string)input[0];

                    if (settings?.InjectType == "pname")
                    {
                        return
                            (inputObject is string &&
                            (String.IsNullOrWhiteSpace(inputObject) == false) &&  // Yeah, I know this looks stupid, but it's WAY easier to read than just
                            inputObject.ToString().Length >= 1);                  // using an ! operator, especially inside of a twice-nested statement
                                                                                  // nested in a twice-nested conditional block nested in a nested lambda
                                                                                  // function nested in a definition. So before you say anything, consider
                                                                                  // that and then hop off my nuts, please and thank you
                    }
                }

                if (IntPtr.Size == 8)
                {
                    try
                    {
                        inputObject = (Int64)input[0];
                    }
                    catch
                    {
                        inputObject = (UInt64)input[0];
                    }

                }
                else
                {
                    try
                    {
                        inputObject = (Int32)input[0];
                    }
                    catch
                    {
                        inputObject = (UInt32)input[0];
                    }
                }
                return Functions.IsNumericType(inputObject) && inputObject >= 0;
            });

        public TriageNurse(ILogger<TriageNurse> logger)
        {
            ;
            _logger = logger;
            ;
        }


        /// <summary>
        /// Guides the user through an interactive process to collect all required information to perform
        /// a DLL/PE image injection.
        /// </summary>
        /// <remarks>This method prompts the user for the injection type (by process name or process ID),
        /// the target process, the DLL path, and whether to wait for the process to start if it is not running. If an
        /// error occurs during any step, the method handles the error and returns the current state of the <see
        /// cref="IntakeForm"/>. The method is intended for interactive, console-based scenarios and is not suitable for
        /// automated or non-interactive use.</remarks>
        /// <returns>An <see cref="IntakeForm"/> object containing the user's selections and input for the injection process. The
        /// returned object reflects the user's choices, even if the process is cancelled or an error occurs.</returns>
        public IntakeForm INeedHelp()
        {
            #region Inject type selection
            IntakeForm returnObject = new();

            _logger?.LogInformation("No parameters provided, starting interactive mode...");

            SyringePromptSettings userPromptSettings = new SyringePromptSettings()
                .Question($"Do you want to inject based on {FGColor("g", "Process Name")} or {FGColor("y", "Process ID")}?\n")
                .DefaultChoice("name")
                .UserResponseText($"(Choose [green]name[/] or [yellow]id[/]): ")
                .ResponseType("".GetType())
                .SetVerificationFunction(_modeSelectionFunction);

            try
            {
                returnObject = GetInputLoop<string>(inputObject: returnObject, settings: userPromptSettings);
            }
            catch (Exception ex)
            {
                HandleInteractiveError(ex, ref returnObject, "An error was thrown during interactive mode injection type collection:\n\n {exceptionMessage}\n");
                return returnObject;
            }

            if (returnObject.ShouldInject)
            {
                switch (returnObject.UserResponse?.ToLowerInvariant().Trim())
                {
                    case "name":
                    case "n":
                    case "process name":
                    case "processname":
                    case "pname":
                    case "p name":
                        returnObject.InjectType = "pname";
                        userPromptSettings
                            .UserResponseText("Enter the [green]process name[/] (e.g., notepad.exe): ")
                            .DefaultChoice("notepad.exe");
                        break;
                    case "pid":
                    case "p":
                    case "id":
                    case "i":
                    case "process id":
                    case "processid":
                        returnObject.InjectType = "id";
                        userPromptSettings
                            .UserResponseText("Enter the [green]process id[/] (e.g., 29597): ")
                            .DefaultChoice(0);
                        break;
                    default:
                        returnObject.InjectType = "pname";
                        userPromptSettings
                            .UserResponseText("Enter the [green]process name[/] (e.g., notepad.exe): ")
                            .DefaultChoice("notepad.exe");
                        break;
                }

                returnObject.UserResponse = null;
            }

            #endregion
            #region Target process selection

            userPromptSettings
                .Question("Please enter the target process information.\n")
                .ResponseType(returnObject.InjectType == "pname" ? "".GetType() : Functions.GetArchUnsignedIntType())
                .SetVerificationFunction(_targetSelectionFunction);

            try
            {
                if (returnObject.InjectType == "pname")
                {
                    returnObject = GetInputLoop<string>(inputObject: returnObject, settings: userPromptSettings);

                    // I know, but we're in a try block, calm yourself
                    returnObject.ProcessName = returnObject.UserResponse ?? String.Empty;
                }
                else
                {
                    returnObject = IntPtr.Size >= 8 ?
                        GetInputLoop<UInt64>(inputObject: returnObject, settings: userPromptSettings) :
                        GetInputLoop<UInt32>(inputObject: returnObject, settings: userPromptSettings);

                    returnObject.ProcessId = (nuint)returnObject.UserResponse;
                }
            }
            catch (Exception ex)
            {
                HandleInteractiveError(ex, ref returnObject, "An error was thrown during interactive mode target process collection:\n\n {exceptionMessage}\n");
                return returnObject;
            }

            #endregion
            #region DLL path selection

            userPromptSettings.Question("Please enter the full path to the DLL you wish to inject:\n")
                .UserResponseText("DLL Path: ")
                .DefaultChoice("C:\\Path\\To\\Your\\DLL.dll")
                .ResponseType("".GetType())
                .SetVerificationFunction(new Func<object[], bool>((params object[] input) =>
                {

                    // only do basic checking on this right now
                    string pathInput = (string)input[0];
                    if (String.IsNullOrWhiteSpace(pathInput) || pathInput.Length < 3)
                    {
                        return false;
                    }
                    return true;
                }));

            try
            {
                returnObject = GetInputLoop<string>(inputObject: returnObject, settings: userPromptSettings);
            }
            catch (Exception ex)
            {
                HandleInteractiveError(ex, ref returnObject, "An error was thrown during interactive mode DLL path collection:\n\n {exceptionMessage}\n");
                return returnObject;
            }

            returnObject.DllPaths = new string[] { (string)returnObject.UserResponse! };

            #endregion
            #region Process wait and timeout selection

            if (returnObject.InjectType == "pname")
            {
                _logger?.LogInformation($"If the target process {FGColor("g", $"{returnObject.ProcessName}")} is not currently running, would you like to {FGColor("y", "wait")} for it to launch and then inject at the first opportunity?\n");
                returnObject.ShouldWait = AnsiConsole.Confirm("Wait for process to launch? ");
            }
            else
            {
                returnObject.ShouldWait = false;
                returnObject.Timeout = (nuint)Functions.NumGetCeiling(Functions.GetArchUnsignedIntType());
            }


            if (returnObject.ShouldWait)
            {
                userPromptSettings
                    .Question($"How long ({FGColor("g", "in seconds")}) would you like to {FGColor("y", "wait")} for the target process to launch before giving up?\n")
                    .UserResponseText("Timeout ([yellow]in seconds[/]): ")
                    .DefaultChoice(Functions.NumGetCeiling(Functions.GetArchSignedIntType()))
                    .ResponseType(Functions.GetArchUnsignedIntType())
                    .SetVerificationFunction(_targetSelectionFunction);

                try
                {
                    returnObject = IntPtr.Size >= 8 ?
                        GetInputLoop<UInt64>(inputObject: returnObject, settings: userPromptSettings) :
                        GetInputLoop<UInt32>(inputObject: returnObject, settings: userPromptSettings);

                    returnObject.Timeout = (nuint)returnObject.UserResponse;

                }
                catch (Exception ex)
                {
                    HandleInteractiveError(ex, ref returnObject, "An error was thrown during interactive mode target process collection:\n\n {exceptionMessage}\n");
                    return returnObject;
                }

                #endregion
            }

            return returnObject;
        }

        /// <summary>
        /// Prompts the user for input in an interactive loop, validating the response according to the specified
        /// settings and returning an updated IntakeForm class object upon successful input.
        /// </summary>
        /// <remarks>If the type parameter T does not match the expected response type specified in the
        /// settings, a warning is logged but the method continues. The method allows up to 10 consecutive failed input
        /// attempts before terminating with an exception. The user is given the option to exit the interactive mode
        /// after several failed attempts.</remarks>
        /// <typeparam name="T">The expected Type of the user's response. Must match the response type specified in the settings.
        /// This is to prevent signed/unsigned overflow/underflow shenanigans and the like.</typeparam>
        /// <param name="inputObject">The IntakeForm object to be updated with the user's validated response.</param>
        /// <param name="settings">The settings that define the prompt question, expected response type, default choice, and validation logic
        /// for the user input.</param>
        /// <returns>An updated IntakeForm object containing the user's validated response. The ShouldInject property is set to
        /// true if input is successfully obtained.</returns>
        /// <exception cref="Exception">Thrown if the user fails to provide valid input after 10 consecutive attempts, or if the user chooses to
        /// exit the interactive mode after multiple failed attempts.</exception>
        private IntakeForm GetInputLoop<T>(IntakeForm inputObject, SyringePromptSettings settings)
        {
            int promptFailures = 0;

            string promptQuestion = settings.Question();
            T defaultChoice = (T)settings.DefaultChoice();
            Type ExpectedType = settings.ResponseType();
            string responseText = settings.UserResponseText();

            bool goodPromptResponse = false;
            //T userResponse;
            IntakeForm returnObject = new();

            if (typeof(T) != ExpectedType)
            {
                _logger?.LogWarning("Received type {inputType} as input, but expect {respType} type as a response", typeof(T).ToString(), ExpectedType.ToString());
            }

            while (!goodPromptResponse)
            {
                if (promptFailures >= 10)
                {
                    _logger?.LogCritical("Either you're drunk, or I'm drunk, and neither of us are going to do anything productive in this situation.");
                    _logger?.LogCritical("Let's both come back when we're sober.\n");
                    _logger?.LogInformation("Exiting interactive mode after 10 consecutive failed input attempts.");
                    throw new Exception("One of us is drunk.");
                }

                if (promptFailures == 3 || promptFailures == 5)
                {

                    //_logger?.LogWarning("There have been: {{_red_}}{firstPromptFailures}{{_}} consecutive failed attempts to gather necessary information.\n", promptFailures);
                    _logger?.LogWarning($"There have been: {FGColor("sigred", $"{promptFailures}")} consecutive failed attempts to gather necessary information.\n");
                    bool shouldContinue = AnsiConsole.Confirm("Would you want to continue?", false);

                    if (!shouldContinue)
                    {
                        _logger?.LogError($"User opted to exit interactive mode after {FGColor("sigred", $"{promptFailures}")} failed attempts.");
                        throw new Exception("User chose to exit interactive mode.");
                    }
                }

                _logger?.LogInformation(promptQuestion);


                dynamic? cleanResponse;
                if (Functions.IsNumericType(defaultChoice))
                {
                    string tempResponse = String.Empty;

                    if (inputObject.InjectType == String.Empty || inputObject.InjectType == "pname")
                    {
                        tempResponse = AnsiConsole.Ask<string>(prompt: responseText, defaultValue: "18446744073709551615");
                    }
                    else
                    {
                        tempResponse = AnsiConsole.Ask<string>(prompt: responseText, defaultValue: "0");
                    }

                    Type targetType = settings.ResponseType();

                    if (Functions.NumIsSignedType(targetType))
                    {
                        if (!Int64.TryParse(tempResponse, out Int64 parsedValue))
                        {
                            promptFailures++;
                            _logger?.LogError($"Invalid numeric input: {tempResponse}");
                            continue;
                        }

                        dynamic floor = Functions.NumGetFloor(targetType);
                        dynamic ceiling = Functions.NumGetCeiling(targetType);

                        if (parsedValue < floor || parsedValue > ceiling)
                        {
                            promptFailures++;
                            _logger?.LogError($"Value {parsedValue} out of range: [{floor}, {ceiling}]");
                            continue;
                        }

                        cleanResponse = parsedValue;
                    }

                    else  // Unsigned type
                    {
                        if (!UInt64.TryParse(tempResponse, out UInt64 parsedValue))
                        {
                            promptFailures++;
                            _logger?.LogError($"Invalid numeric input: {tempResponse}");
                            continue;
                        }

                        dynamic ceiling = Functions.NumGetCeiling(targetType);

                        if (parsedValue > ceiling)
                        {
                            promptFailures++;
                            _logger?.LogError($"Value {parsedValue} exceeds maximum: {ceiling}");
                            continue;
                        }

                        cleanResponse = parsedValue;
                    }
                }

                else
                {
                    cleanResponse = AnsiConsole.Ask<T>(prompt: responseText, defaultValue: defaultChoice);
                }

                if (null == cleanResponse)
                {
                    promptFailures++;
                    _logger?.LogError("You have made an invalid selection.");
                    continue;
                }

                //bool verifiedGood = settings.VerificationFunction(userResponse);
                bool verifiedGood = settings.VerificationFunction().Invoke([cleanResponse, inputObject]);

                if (verifiedGood)
                {
                    goodPromptResponse = true;
                    returnObject = inputObject;
                    returnObject.ShouldInject = true;
                    returnObject.UserResponse = cleanResponse;
                }
                else
                {
                    promptFailures++;
                    _logger?.LogError("You have made an invalid selection.");
                }


                //userInjectType = AnsiConsole.Ask<string>("(Enter [green]name[/] or [yellow]id[/]):", "name");
            }



            return returnObject;
        }

        /// <summary>
        /// Handles an interactive error by logging the exception, displaying it to the user, and updating the return
        /// object with error details.
        /// </summary>
        /// <remarks>This method resets the return object and populates its message with formatted
        /// exception details. The exception is both logged and written to the console for user visibility.</remarks>
        /// <param name="ex">The exception that occurred which will be logged and displayed.</param>
        /// <param name="returnObject">A reference to the IntakeForm object that will be updated with error information.</param>
        /// <param name="errorMessage">An optional error message to include in the log entry. If not specified, an empty string is used.</param>
        /// <param name="loglevel">The log level to use when recording the error. If not specified, defaults to LogLevel.Error.</param>
        private void HandleInteractiveError(Exception ex, ref IntakeForm returnObject, string? errorMessage = "", LogLevel? loglevel = LogLevel.Error)
        {
            if (loglevel != LogLevel.Error)
            {
                _logger?.LogCritical(errorMessage, ex);
            }
            else
            {
                _logger?.LogError(errorMessage, ex);
            }

            AnsiConsole.WriteException(ex, ExceptionFormats.ShowLinks);

            returnObject = new IntakeForm();
            returnObject.Message = Functions.FormatObject(ex);
        }
    }


    /// <summary>
    /// Represents the configuration settings for a user prompt, including the question text, default choice, response
    /// type, and input verification logic.
    /// </summary>
    /// <remarks>This class defines the parameters and validation behavior for interactive prompts, such
    /// as those presented to the user when prompting for necessary values to perform the DLL injection.
    /// The settings allow customization of the prompt's question, the expected response Type, the
    /// default value, and a verification function to validate user input. This class is immutable
    /// after configuration; use the provided methods to fluently set each option before
    /// presenting the prompt.</remarks>
    public sealed class SyringePromptSettings
    {
        private string _question = String.Empty;
        private dynamic? _defaultChoice;
        private string _userResponseText = String.Empty;
        private Type _responseType = typeof(object);

        private Func<object[], bool>? _verificationFunction = (params object[] input) => { return true; };
        //private Func<dynamic, dynamic?, bool>? _verificationFunction = (input, input2) => { return true; };

        //public Func<dynamic, bool> VerificationFunction { get; internal set; } = (input) => { return true; };
        public SyringePromptSettings()
        {

        }

        public SyringePromptSettings(
            string question,
            dynamic? defaultChoice,
            string userResponseText,
            Type responseType,
            Func<object[], bool> verificationFunction
            )
        {
            this.Question(question)
            .UserResponseText(userResponseText)
            .ResponseType(responseType)
            .SetVerificationFunction(verificationFunction)
            .DefaultChoice(defaultChoice);
        }


        public string Question()
        {
            return _question;
        }

        public SyringePromptSettings Question(string? question)
        {
            question ??= String.Empty;
            _question = question;
            return this;
        }

        public dynamic DefaultChoice()
        {
            return _defaultChoice ?? String.Empty;
        }

        public SyringePromptSettings DefaultChoice(dynamic? defaultChoice)
        {
            defaultChoice ??= String.Empty;
            _defaultChoice = defaultChoice;
            return this;
        }

        public string UserResponseText()
        {
            return _userResponseText;
        }

        public SyringePromptSettings UserResponseText(string? userResponseText)
        {
            userResponseText ??= String.Empty;
            _userResponseText = userResponseText;
            return this;
        }

        public Type ResponseType()
        {
            return _responseType;
        }

        public SyringePromptSettings ResponseType(Type? responseType)
        {
            responseType ??= typeof(object);
            _responseType = responseType;
            return this;
        }

        public SyringePromptSettings SetVerificationFunction(Func<object[], bool> verificationFunction)
        {
            _verificationFunction = verificationFunction;
            return this;
        }
        public Func<object[], bool> VerificationFunction()
        {
            return _verificationFunction!;
        }
    }


    /// <summary>
    /// Represents the data collected from an interactive mode run, including injection options, process
    /// information, and user responses.
    /// </summary>
    /// <remarks>This class is used to encapsulate user input and configuration details required for
    /// the injection workflow. All properties are set internally and are not intended to be modified directly by
    /// external callers.
    /// 
    /// <br></br><br></br>Instances of this class are created and populated by the main Cmdlet in conjunction with
    /// the helper TriageNurse data collection interactive-mode class. The populated values are further validated
    /// by the main Cmdlet and subsequently-called methods, and are ultimately used as the critcial key data
    /// points during an injection attempt. </remarks>
    public sealed class IntakeForm
    {
        public bool ShouldInject { get; internal set; } = false;
        public string Message { get; internal set; } = "User exited interactive mode.";
        public string[] DllPaths { get; internal set; } = [];

        public string InjectType { get; internal set; } = String.Empty;

        public string ProcessName { get; internal set; } = String.Empty;

        public nuint ProcessId { get; internal set; } = (nuint)Functions.NumGetFloor(Functions.GetArchUnsignedIntType());

        public bool ShouldWait { get; internal set; } = false;
        public nuint Timeout { get; internal set; } = (nuint)Functions.NumGetCeiling(Functions.GetArchUnsignedIntType());
        public dynamic? UserResponse;

        public IntakeForm()
        {

        }

        public IntakeForm(
            bool shouldInject,
            string message,
            string[] dllPaths,
            string injectType,
            string processName,
            nuint processId,
            bool shouldWait,
            nuint timeout,
            dynamic? userResponse = null
            )
        {
            ShouldInject = shouldInject;
            Message = message;
            DllPaths = dllPaths;
            InjectType = injectType;
            ProcessName = processName;
            ProcessId = processId;
            ShouldWait = shouldWait;
            Timeout = timeout;
            UserResponse = userResponse;
        }

    }
}
