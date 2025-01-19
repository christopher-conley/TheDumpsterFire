#!/usr/bin/pwsh

[CmdletBinding(DefaultParameterSetName = 'Default')]
param (
    [Parameter(Mandatory = $true, ParameterSetName = 'AssemblyVersion')]
    [switch]
    $AssemblyVersion,

    [Parameter(Mandatory = $true, ParameterSetName = 'MSIProductVersion')]
    [switch]
    $MSIProductVersion
)

$OriginalErrorActionPreference = $ErrorActionPreference

## If this script is ran as a pre-build event from Visual Studio, it exits prematurely
## immediately after the "begin" block and never proceeds to the "process" block
## for some reason. So this is just one big ugly glob of script that does everything

function DoCleanup
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [hashtable]
        $CleanupInfo,

        [Parameter(Mandatory = $false)]
        [array]
        $SubscribedEngineEvents,

        [Parameter(Mandatory = $false)]
        [switch]
        $FromEngineEvent,

        [Parameter(Mandatory = $false)]
        [switch]
        $TerminatingError
    )

    if ($FromEngineEvent)
    {
        Write-Information -MessageData "Called by a PowerShell engine event" -InformationAction 'Continue'

        if ($TerminatingError)
        {
            $ErrorActionPreference = "Continue"
            Write-Error -Message "Terminating error(s) detected.`n"
        }

        if ($TerminatingError -and $CleanupInfo.Errors.Count -gt 0)
        {
            Write-Error -Message "Error details: "
            foreach ($ErrorMessage in ($CleanupInfo.Errors))
            {
                Write-Error -Message "$ErrorMessage"
            }
        }
        $ErrorActionPreference = $OriginalErrorActionPreference
    }

    if (($null -ne $SubscribedEngineEvents) -and ($SubscribedEngineEvents.Count -gt 0))
    {
        Write-Information -MessageData "Unsubscribing from $($SubscribedEngineEvents.Count) engine event(s)." -InformationAction 'Continue'
        foreach ($EventJob in $SubscribedEngineEvents)
        {
            Write-Information -MessageData "Unsubscribing from engine event source: $($EventJob.SourceIdentifier)" -InformationAction 'Continue'
            Unregister-Event -SourceIdentifier "$($EventJob.SourceIdentifier)"
        }
    }

    if ($HostIsTranscribing)
    {
        Write-Information -MessageData "Stopping transcript on logfile: $((Resolve-Path -Path $CleanupInfo.LogFile).Path)" -InformationAction 'Continue'
        Stop-Transcript
    }

    if ($null -ne $CleanupInfo.Mutex)
    {
        Write-Information -MessageData "Disposing of file mutex" -InformationAction 'Continue'
        $CleanupInfo.Mutex.WaitOne()
        $CleanupInfo.Mutex.Close()
        $CleanupInfo.Mutex.Dispose()
        Write-Information -MessageData "Disposed of file mutex" -InformationAction 'Continue'
    }

    Write-Information -InformationAction 'Continue' -MessageData "Exiting from cleanup..."

    if ($CleanupInfo.HasErrors)
    {
        [System.Environment]::Exit(1)
    }
    else
    {
        [System.Environment]::Exit(0)
    }
}

# if ($PSCmdlet.ParameterSetName -eq 'Default')
# {
#     Write-Error -Message "The script was not called correctly, neither -AssemblyVersion nor -MSIProductVersion was specified."
#     [System.Environment]::Exit(1)
# }

[array] $SubscribedEvents = @()
[string] $ParamSetName = $PSCmdlet.ParameterSetName.ToString()
[string] $MutexName = "${ParamSetName}Mutex"
[string] $LogDir = "..\scripts\logs"
[string] $LogFilename = "setversion_${ParamSetName}_$((Get-Date).ToString("yyyy-MM-ddTHH.mm.ss.fff")).log"
[string] $LogFile = Join-Path -Path "$LogDir" -ChildPath "$LogFilename"

## This really shouldn't be necessary, but Visual Studio sometimes calls the
## prebuild script multiple times in rapid succession
[System.Threading.Mutex] $FileMutex = [System.Threading.Mutex]::new($false, $MutexName)

[hashtable] $ExitInfo = @{
    HasErrors = $false
    Errors    = @()
    LogFile   = "$Logfile"
    Mutex     = $FileMutex
}

$NormalExitActionInfo = {
    DoCleanup -CleanupInfo (Get-Variable ExitInfo).Value -SubscribedEngineEvents (Get-Variable SubscribedEvents).Value -FromEngineEvent
}

Register-EngineEvent -SourceIdentifier "InternalTerminatingError" -Action {
    $ErrorActionPreference = "Continue"
    $ErrorEvent = $Event.SourceEventArgs
    $ErrorExitInfo = (Get-Variable ExitInfo).Value
    $UnsubscribeEvents = (Get-Variable SubscribedEvents).Value
    $ExitInfo.HasErrors = $true
    if ($Event.SourceEventArgs -is [System.Management.Automation.ErrorRecord])
    {
        $ExitInfo.Errors += $ErrorEvent.SourceEventArgs
        if ($null -ne $ErrorEvent.SourceEventArgs.InvocationInfo)
        {
            Write-Error "Terminating Error detected in script `"$($ErrorEvent.InvocationInfo.ScriptName)`" at line: $($ErrorEvent.InvocationInfo.ScriptLineNumber)"
            Write-Error "Message data: $($ErrorEvent.MessageData)"
        }
    }
    else
    {
        $ExitInfo.Errors += $ErrorEvent.MessageData
    }

    $ErrorActionPreference = $OriginalErrorActionPreference
    DoCleanup -CleanupInfo $ErrorExitInfo -SubscribedEngineEvents $UnsubscribeEvents -FromEngineEvent -TerminatingError
}

Register-EngineEvent -SourceIdentifier "PowerShell.Exiting" -SupportEvent -Action (Get-Variable NormalExitActionInfo).Value
$SubscribedEvents += Get-EventSubscriber


if (!(Test-Path "$LogDir"))
{
    New-Item -Path "$LogDir" -ItemType Directory
}

Start-Transcript -Path "$LogFile" -Append
[bool] $HostIsTranscribing = $true

Write-Information -InformationAction 'Continue' -MessageData "PsparameterSetName: $($PSCmdlet.ParameterSetName)"
[string] $ApplicationVersion = (Get-Date).ToString("yyyy.MM.dd.HHmm")
[string] $MSIProductVersion = $ApplicationVersion.Substring(2)

Write-Information -InformationAction 'Continue' -MessageData "ApplicationVersion: $ApplicationVersion"
Write-Information -InformationAction 'Continue' -MessageData "MSIProductVersion: $MSIProductVersion"

try {
    Write-Information -InformationAction 'Continue' -MessageData "Acquiring file mutex"
    $FileMutex.WaitOne()
    Write-Information -InformationAction 'Continue' -MessageData "Acquired mutex"

    Write-Information -InformationAction 'Continue' -MessageData "Making backup of VERSION file"
    Copy-Item -Path "..\VERSION" -Destination "..\VERSION.bak" -Force -Confirm:$false

    Write-Information -InformationAction 'Continue' -MessageData "Setting content of VERSION file in the main solution directory to: $ApplicationVersion."
    Set-Content -Path "..\VERSION" -Value $ApplicationVersion -NoNewline -Force -Confirm:$false -Encoding utf8

    Write-Information -InformationAction 'Continue' -MessageData "Removing backup of VERSION file"
    Remove-Item -Path "..\VERSION.bak" -Force -Confirm:$false
}
catch {
    $VersionError = $_
    [string] $VersionBackupFile = (Resolve-Path -Path (Join-Path -Path "$PSScriptRoot" -ChildPath "..\VERSION.bak")).Path
    [string] $LogfilePath = (Resolve-Path -Path (Join-Path -Path "$PSScriptRoot" -ChildPath "$LogFile")).Path
    Write-Error -Message "Setting version information failed. The error was: $($VersionError.Exception)"
    Write-Error -Message "$($VersionError.InvocationInfo.Line)"
    Write-Error -Message "$($VersionError.InvocationInfo.PositionMessage)"
    Write-Error -Message "$($VersionError.Exception.StackTrace)`n"

    Write-Warning -Message "Reverting VERSION file to its original state."
    Copy-Item -Path "..\VERSION.bak" -Destination "..\VERSION" -Force -Confirm:$false
    Write-Warning -Message "Backup of VERSION file has intentionally been left remaining on the filesystem at: $VersionBackupFile"
    Write-Warning -Message "Please inspect Build Output and the log file at `"$LogfilePath`" to determine the new and exciting method Visual Studio has discovered to fuck things up."
    New-Event -SourceIdentifier "InternalTerminatingError" -EventArguments $VersionParseError -MessageData "Setting version information failed. The error was: $($VersionError.Exception)"
}
finally {
    Write-Information -InformationAction 'Continue' -MessageData "Releasing file mutex"
    $FileMutex.ReleaseMutex()
    Write-Information -InformationAction 'Continue' -MessageData "Exiting..."
    DoCleanup -CleanupInfo $ExitInfo -SubscribedEngineEvents $SubscribedEvents
}

