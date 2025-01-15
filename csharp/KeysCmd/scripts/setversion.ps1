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

function DoCleanup {
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

    if ($FromEngineEvent) {
        Write-Information -MessageData "Called by a PowerShell engine event" -InformationAction Continue

        if ($TerminatingError) {
            $ErrorActionPreference = "Continue"
            Write-Error -Message "Terminating error(s) detected.`n"
        }

        if ($TerminatingError -and $CleanupInfo.Errors.Count -gt 0) {
            Write-Error -Message "Error details: "
            foreach ($ErrorMessage in ($CleanupInfo.Errors)) {
                Write-Error -Message "$ErrorMessage"
            }
        }
        $ErrorActionPreference = $OriginalErrorActionPreference
    }

    if (($null -ne $SubscribedEngineEvents) -and ($SubscribedEngineEvents.Count -gt 0)) {
        Write-Information -MessageData "Unsubscribing from $($SubscribedEngineEvents.Count) engine event(s)." -InformationAction Continue
        foreach ($EventJob in $SubscribedEngineEvents) {
            Write-Information -MessageData "Unsubscribing from engine event source: $($EventJob.SourceIdentifier)" -InformationAction Continue
            Unregister-Event -SourceIdentifier "$($EventJob.SourceIdentifier)"
        }
    }

    if ($HostIsTranscribing) {
        Write-Information -MessageData "Stopping transcript on logfile: $($CleanupInfo.LogFile)" -InformationAction Continue
        Stop-Transcript -Confirm:$false | Out-Null
    }

    if ($null -ne $CleanupInfo.Mutex) {
        Write-Information -MessageData "Disposing of file mutex" -InformationAction Continue
        $CleanupInfo.Mutex.WaitOne()
        $CleanupInfo.Mutex.Close()
        $CleanupInfo.Mutex.Dispose()
        Write-Information -MessageData "Disposed of file mutex" -InformationAction Continue
    }

    if ($CleanupInfo.HasErrors) {
        [System.Environment]::Exit(1)
    }
    else {
        [System.Environment]::Exit(0)
    }
}

if ($PSCmdlet.ParameterSetName -eq 'Default') {
    Write-Error -Message "The script was not called correctly, neither -AssemblyVersion nor -MSIProductVersion was specified."
    [System.Environment]::Exit(1)
}

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
    Errors = @()
    LogFile = "$Logfile"
    Mutex = $FileMutex
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
    if ($Event.SourceEventArgs -is [System.Management.Automation.ErrorRecord]) {
        $ExitInfo.Errors += $ErrorEvent.SourceEventArgs
        if ($null -ne $ErrorEvent.SourceEventArgs.InvocationInfo) {
            Write-Error "Terminating Error detected in script `"$($ErrorEvent.InvocationInfo.ScriptName)`" at line: $($ErrorEvent.InvocationInfo.ScriptLineNumber)"
            Write-Error "Message data: $($ErrorEvent.MessageData)"
        }
    }
    else {
        $ExitInfo.Errors += $ErrorEvent.MessageData
    }

    $ErrorActionPreference = $OriginalErrorActionPreference
    DoCleanup -CleanupInfo $ErrorExitInfo -SubscribedEngineEvents $UnsubscribeEvents -FromEngineEvent -TerminatingError
}

Register-EngineEvent -SourceIdentifier "PowerShell.Exiting" -SupportEvent -Action (Get-Variable NormalExitActionInfo).Value
$SubscribedEvents += Get-EventSubscriber


if (!(Test-Path "$LogDir")) {
    New-Item -Path "$LogDir" -ItemType Directory
}

Start-Transcript -Path "$LogFile" -Append
[bool] $HostIsTranscribing = $true

Write-Host "Setting the AssemblyVersion and MSIProductVersion attributes in the KeysCmd.csproj and KeysCmdInstaller.wixproj project files."
Write-Host "PsparameterSetName: $($PSCmdlet.ParameterSetName)"
[string] $ApplicationVersion = (Get-Date).ToString("yyyy.MM.dd.HHmm")
[string] $MSIProductVersion = $ApplicationVersion.Substring(2)
[string] $CurrenAssemblyVersion = [string]::Empty
[string] $CurrentMSIProductVersion = [string]::Empty

Write-Host "ApplicationVersion: $ApplicationVersion"
Write-Host "MSIProductVersion: $MSIProductVersion"

try
{
    Write-Information -MessageData "Acquiring file mutex for the script." -InformationAction Continue
    $FileMutex.WaitOne()
    Write-Information -MessageData "Acquired mutex" -InformationAction Continue
    if ($PSCmdlet.ParameterSetName -eq 'AssemblyVersion') {
        $CurrenAssemblyVersion = [System.Text.RegularExpressions.Regex]::Match((Get-Content ".\KeysCmd.csproj"), '<AssemblyVersion>(\d\d\d\d\.\d\d\.\d\d\.\d\d\d\d)<\/AssemblyVersion>').Groups[1].Value
        $CurrentMSIProductVersion = [System.Text.RegularExpressions.Regex]::Match((Get-Content "..\KeysCmdInstaller\KeysCmdInstaller.wixproj"), '<MSIProductVersion>(\d\d\.\d\d\.\d\d\.\d\d\d\d)<\/MSIProductVersion>').Groups[1].Value
    }
    elseif ($PSCmdlet.ParameterSetName -eq 'MSIProductVersion') {
        $CurrenAssemblyVersion = [System.Text.RegularExpressions.Regex]::Match((Get-Content "..\KeysCmd\KeysCmd.csproj"), '<AssemblyVersion>(\d\d\d\d\.\d\d\.\d\d\.\d\d\d\d)<\/AssemblyVersion>').Groups[1].Value
        $CurrentMSIProductVersion = [System.Text.RegularExpressions.Regex]::Match((Get-Content ".\KeysCmdInstaller.wixproj"), '<MSIProductVersion>(\d\d\.\d\d\.\d\d\.\d\d\d\d)<\/MSIProductVersion>').Groups[1].Value
    }
    else {
        New-Event -SourceIdentifier "InternalTerminatingError" -MessageData "The script was not called correctly, neither -AssemblyVersion nor -MSIProductVersion was specified, and reaching this line of code should have been impossible."
    }

    Write-Host "Current AssemblyVersion: $CurrenAssemblyVersion"
    Write-Host "Current MSIProductVersion: $CurrentMSIProductVersion"
}
catch
{
    $VersionParseError = $_
    Write-Error -Message "Parsing the AssemblyVersion and MSIProductVersion attributes failed. The error was: $($VersionParseError.Exception)"
    Write-Error -Message "Ensure that the `"AssemblyVersion`" and `"MSIProductVersion`" attributes are present in the KeysCmd.csproj and KeysCmdInstaller.wixproj project files."
    Write-Error -Message "The AssemblyVersion attribute should be in the format: <AssemblyVersion>$ApplicationVersion</AssemblyVersion>."
    Write-Error -Message "The MSIProductVersion attribute should be in the format: <MSIProductVersion>$MSIProductVersion</MSIProductVersion>."
    New-Event -SourceIdentifier "InternalTerminatingError" -EventArguments $VersionParseError -MessageData "Parsing AssemblyVersion and MSIProductVersion attributes failed. The error was: $($VersionParseError.Exception)"
}
finally {
    Write-Information -MessageData "Releasing file mutex" -InformationAction Continue
    $FileMutex.ReleaseMutex()
}

[bool] $HasVersionParity = $CurrenAssemblyVersion.Substring(2) -eq $CurrentMSIProductVersion
Write-Host "HasVersionParity: $HasVersionParity"

if ($PSCmdlet.ParameterSetName -eq 'AssemblyVersion')
{

    try
    {
        Write-Information -MessageData "Acquiring file mutex for the script." -InformationAction Continue
        $FileMutex.WaitOne()
        Write-Information -MessageData "Acquired mutex" -InformationAction Continue
        $ApplicationVersion | Set-Content ".\VERSION" -Force -Confirm:$false -Encoding utf8
        Write-Host "Setting AssemblyVersion to $ApplicationVersion in the KeysCmd.csproj project file."
        Copy-Item -Path ".\KeysCmd.csproj" -Destination ".\KeysCmd.csproj.bak" -Force -Confirm:$false
        Write-Host "Current proj file: "
        Write-Host "$(Get-Content ".\KeysCmd.csproj")"
        Write-Host "`n`n"
            (Get-Content ".\KeysCmd.csproj") |
        ForEach-Object { 
            $_ -replace '^\W+<AssemblyVersion>\d\d\d\d\.\d\d\.\d\d\.\d\d\d\d<\/AssemblyVersion>$', "    <AssemblyVersion>$ApplicationVersion</AssemblyVersion>"
        } | Set-Content -Path ".\KeysCmd.csproj" -Force -Confirm:$false -Encoding utf8
        
        Write-Host "New proj file: "
        Write-Host "$(Get-Content ".\KeysCmd.csproj")"
        Remove-Item -Path ".\KeysCmd.csproj.bak" -Force -Confirm:$false

        exit 0
        
    }
    catch
    {
        $VersionError = $_
        Write-Error -Message "Setting the AssemblyVersion attribute failed. The error was: $($VersionError.Exception)"
        Write-Warning -Message "Reverting the KeysCmd.csproj file to its original state."
        Copy-Item -Path ".\KeysCmd.csproj.bak" -Destination ".\KeysCmd.csproj" -Force -Confirm:$false
        New-Event -SourceIdentifier "InternalTerminatingError" -EventArguments $VersionParseError -MessageData "Setting the AssemblyVersion attribute failed. The error was: $($VersionError.Exception)"
    }
    finally {
        Write-Information -MessageData "Releasing file mutex" -InformationAction Continue
        $FileMutex.ReleaseMutex()
    }
}

elseif ($PSCmdlet.ParameterSetName -eq "MSIProductVersion")
{
    if (!$HasVersionParity)
    {
        try
        {
            Write-Information -MessageData "Acquiring file mutex for the script." -InformationAction Continue
            $FileMutex.WaitOne()
            Write-Information -MessageData "Acquired mutex" -InformationAction Continue
            $MSIProductVersion | Set-Content ".\VERSION" -Force -Confirm:$false -Encoding utf8
            Copy-Item -Path ".\KeysCmdInstaller.wixproj" -Destination ".\KeysCmdInstaller.wixproj.bak" -Force -Confirm:$false
                (Get-Content ".\KeysCmdInstaller.wixproj") |
            ForEach-Object { 
                $_ -replace '^\W+<MSIProductVersion>\d\d\.\d\d\.\d\d\.\d\d\d\d<\/MSIProductVersion>$', "    <MSIProductVersion>$MSIProductVersion</MSIProductVersion>"
            } | Set-Content -Path ".\KeysCmdInstaller.wixproj" -Force -Confirm:$false -Encoding utf8
            
            Remove-Item -Path ".\KeysCmdInstaller.wixproj.bak" -Force -Confirm:$false

            exit 0
            
        }
        catch
        {
            $VersionError = $_
            Write-Error -Message "Setting the MSIProductVersion attribute failed. The error was: $($VersionError.Exception)"
            Write-Warning -Message "Reverting the KeysCmd.csproj file to its original state."
            Copy-Item -Path ".\KeysCmdInstaller.wixproj.bak" -Destination ".\KeysCmdInstaller.wixproj" -Force -Confirm:$false
            New-Event -SourceIdentifier "InternalTerminatingError" -EventArguments $VersionParseError -MessageData "Setting the MSIProductVersion attribute failed. The error was: $($VersionError.Exception)"
        }
        finally {
            Write-Information -MessageData "Releasing file mutex" -InformationAction Continue
            $FileMutex.ReleaseMutex()
        }
    }
}

Stop-Transcript
$HostIsTranscribing = $false
DoCleanup -CleanupInfo $ExitInfo -SubscribedEngineEvents $SubscribedEvents

