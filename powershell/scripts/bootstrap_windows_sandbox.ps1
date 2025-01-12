function Send-Toast
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $false)]
        [string] $AppID,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [string] $Title,

        [Parameter(Mandatory = $false, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
        [string] $MessageContent,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [string] $ActionButtonLabel,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [string] $ActionButtonActivity,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [string] $Duration,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [switch] $NullActivity = $false
    )

    if ([string]::IsNullOrWhiteSpace($AppID))
    {
        # Must use the Known Folder GUID for this, an absolute path will not work
        # https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid

        $AppID = "{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}\WindowsPowerShell\v1.0\powershell.exe"
    }

    if ($NullActivity)
    {
        $ActionButtonActivity = $null
    }

    [string] $MessageTemplate = @'
"<toast duration=`"PLACEHOLDERDURATION`">
    <visual>
        <binding template=`"ToastGeneric`">
            <text>PLACEHOLDERTITLE</text>
            <text>PLACEHOLDERMESSAGECONTENT</text>
        </binding>
    </visual>
    <actions>
        <action activationType=`"protocol`" arguments=`"PLACEHOLDERACTIONBUTTONACTIVITY`" content=`"PLACEHOLDERACTIONBUTTONLABEL`" />
    </actions>
</toast>"
'@

    [string] $ToastCmd = @"

function CanCastToString
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = `$true, ValueFromPipeline = `$true, ValueFromPipelineByPropertyName = `$true)]
        [AllowNull()]
        [object]
        `$PossibleString,

        [Parameter(Mandatory = `$false, ValueFromPipelineByPropertyName = `$true)]
        [switch]
        `$AllowNull = `$false
    )

    if ((`$AllowNull -eq `$false) -and (`$null -eq `$PossibleString))
    {
        return `$false
    }

    try
    {
        `$TempEA = `$ErrorActionPreference
        `$ErrorActionPreference = 'Stop'
        `$PossibleString.ToString() | Out-Null
        `$ErrorActionPreference = `$TempEA
        return `$true
    }
    catch
    {
        return `$false
    }
}

function Show-ToastNotification
{
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory = `$false)]
        [AllowNull()]
        [ValidateScript( { CanCastToString -PossibleString `$_ -AllowNull } )]
        [string] `$AppID,

        [Parameter(Mandatory = `$false, ValueFromPipelineByPropertyName = `$true)]
        [AllowNull()]
        [ValidateScript( { CanCastToString -PossibleString `$_ -AllowNull } )]
        [string] `$Title,

        [Parameter(Mandatory = `$false, ValueFromPipeline = `$true, ValueFromPipelineByPropertyName = `$true)]
        [AllowNull()]
        [ValidateScript( { CanCastToString -PossibleString `$_ -AllowNull } )]
        [string] `$MessageContent,

        [Parameter(Mandatory = `$false, ValueFromPipelineByPropertyName = `$true)]
        [AllowNull()]
        [ValidateScript( { CanCastToString -PossibleString `$_ -AllowNull } )]
        [string] `$ActionButtonLabel,

        [Parameter(Mandatory = `$false, ValueFromPipelineByPropertyName = `$true)]
        [AllowNull()]
        [ValidateScript( { CanCastToString -PossibleString `$_ -AllowNull } )]
        [string] `$ActionButtonActivity,

        [Parameter(Mandatory = `$false, ValueFromPipelineByPropertyName = `$true)]
        [AllowNull()]
        [ValidateScript( { CanCastToString -PossibleString `$_ -AllowNull } )]
        #[ValidateSet("reminder", "short", "long", "alarm")]
        [string] `$Duration = "reminder",

        [Parameter(Mandatory = `$false, ValueFromPipelineByPropertyName = `$true)]
        [AllowNull()]
        [string] `$XMLTemplate

    )

    begin
    {
        [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
        [Windows.UI.Notifications.ToastNotification, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
        [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null
    }

    process
    {
        # Write-Host "AppID is `$AppID"
        # Write-Host "Title is `$Title"
        # Write-Host "MessageContent is `$MessageContent"
        # Write-Host "ActionButtonLabel is `$ActionButtonLabel"
        # Write-Host "ActionButtonActivity is `$ActionButtonActivity"
        # Write-Host "Duration is `$Duration"
    
        [Windows.Data.Xml.Dom.XmlDocument] `$NotificationObject = New-Object Windows.Data.Xml.Dom.XmlDocument
        `$NotificationObject.LoadXml(`$XMLTemplate)
        
        [Windows.UI.Notifications.ToastNotification] `$ToastNotification = New-Object Windows.UI.Notifications.ToastNotification -ArgumentList `$NotificationObject
    }

    end
    {
        [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier(`$AppID).Show(`$ToastNotification)
    }
}


$(if ([string]::IsNullOrWhiteSpace("$Title")) {
    $Title = "Unspecified Title"
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERTITLE", $Title)
}
else {
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERTITLE", $Title)
}

if ([string]::IsNullOrWhiteSpace("$Duration")) {
    $Duration = "reminder"
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERDURATION", $Duration)
}
else {
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERDURATION", $Duration)
}

if ([string]::IsNullOrWhiteSpace("$MessageContent")) {
    $MessageContent = "Unspecified message"
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERMESSAGECONTENT", $MessageContent)
}
else {
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERMESSAGECONTENT", $MessageContent)
}

if ([string]::IsNullOrWhiteSpace("$ActionButtonLabel")) {
    $ActionButtonLabel = "OK"
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERACTIONBUTTONLABEL", $ActionButtonLabel)
}
else {
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERACTIONBUTTONLABEL", $ActionButtonLabel)
}

if (([string]::IsNullOrWhiteSpace("$ActionButtonActivity")) -and ($NullActivity -eq $false)) {
    $ActionButtonActivity = "https://www.kagi.com"
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERACTIONBUTTONACTIVITY", $ActionButtonActivity)
}
else {
    $MessageTemplate = $MessageTemplate.Replace("PLACEHOLDERACTIONBUTTONACTIVITY", $ActionButtonActivity)
})

    Show-ToastNotification -AppID "$AppID" -Title "$Title" -MessageContent "$MessageContent" -ActionButtonLabel "$ActionButtonLabel" -ActionButtonActivity "$ActionButtonActivity" -Duration "$Duration" -XMLTemplate $MessageTemplate    

"@

    [byte[]] $ToastCmdByteArray = [System.Text.Encoding]::Unicode.GetBytes($ToastCmd)
    [string] $Base64ToastCmd = [Convert]::ToBase64String($ToastCmdByteArray)

    Start-Process -FilePath "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -ArgumentList "-EncodedCommand `"$Base64ToastCmd`"" -NoNewWindow -Wait

}

$DebugPreference = "Continue"
$VerbosePreference = "Continue"
$InformationPreference = "Continue"

[string] $CROShareRoot = "C:\Users\WDAGUtilityAccount\Desktop\shares\cshare_ro"
[string] $CRWShareRoot = "C:\Users\WDAGUtilityAccount\Desktop\shares\cshare_rw"

[string] $EROShareRoot = "C:\Users\WDAGUtilityAccount\Desktop\shares\eshare_ro"
[string] $ERWShareRoot = "C:\Users\WDAGUtilityAccount\Desktop\shares\eshare_rw"
[bool] $PWSHInstallFinished = $false

[System.Text.RegularExpressions.RegexOptions] $RegexIgnoreCase = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
[System.IO.FileSystemWatcher] $PWSHInstallWatcher = New-Object -TypeName System.IO.FileSystemWatcher -ArgumentList "$CRWShareRoot", "pwsh_install.txt"
$PWSHInstallWatcher.EnableRaisingEvents = $true

[hashtable] $PWSHInstallFinishedOptions = @{
    WatchName = "PowerShell"
    EventType = "pwshFinishCheck"
    Path = "$CRWShareRoot\pwsh_install.txt"
    Pattern = '=== Verbose logging stopped:'
    EventWatcher = (Get-Variable PWSHInstallWatcher)
    FinishedVar = (Get-Variable PWSHInstallFinished)
}

$PWSHWatcherAction = {
    InstallIsFinished @PWSHInstallFinishedOptions
}


# Register the event handler
Register-ObjectEvent -InputObject $PWSHInstallWatcher -EventName "Changed" -SourceIdentifier "pwshFinishCheck" -Action $PWSHWatcherAction

function InstallIsFinished {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $WatchName,

        [Parameter(Mandatory = $true)]
        [string]
        $EventType,

        [Parameter(Mandatory = $true)]
        [string]
        $Path,

        [Parameter(Mandatory = $true)]
        [string]
        $Pattern,

        [Parameter(Mandatory = $true)]
        [psvariable]
        $EventWatcher,

        [Parameter(Mandatory = $true)]
        [psvariable]
        $FinishedVar
    )

    try {
        if ([Regex]::Match([System.IO.File]::ReadAllText($Path), $Pattern, $RegexIgnoreCase).Success) {
            $FinishedVar.Value = $true
            Unregister-Event -SourceIdentifier $EventType
            $EventWatcher.Value.EnableRaisingEvents = $false

            Write-Host "$WatchName install finished at $((Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fff")), event unregistered"

            Send-Toast -Title "Done" -MessageContent "$WatchName installed successfully" -NullActivity -Duration reminder
            return
        }
    }

    catch [System.Management.Automation.MethodInvocationException] {
        # File in use, do nothing
        return
    }
    catch {
        # so no infinite loop
        Write-Host "fun infinite loop"
        $FinishedVar.Value = $true
        return
    }

}

Remove-Item -Path "$CRWShareRoot\bootstrap.txt" -Force -Confirm:$false -ErrorAction SilentlyContinue | Out-Null
Start-Transcript -Path "$CRWShareRoot\bootstrap.txt" -Force -Append

Write-Host "Bootstrapping, running as user: $($env:USERNAME)"
Write-Host "Starting pwsh installer with args: /i `"$CROShareRoot\PowerShell.msi`" /qn /norestart /l*v `"$CRWShareRoot\pwsh_install.txt`""

Start-Sleep -Seconds 5
Start-Process -Verbose -FilePath "C:\Windows\System32\msiexec.exe" -ArgumentList "/i `"$CROShareRoot\PowerShell.msi`" /qn /norestart /l*v `"$CRWShareRoot\pwsh_install.txt`""

do {
;
} until ($PWSHInstallFinished)


# New-Variable -Name msgBox -Option AllScope, Constant -Value $( New-Object psobject | Add-Member -MemberType ScriptMethod -Name Show -Value {
#         param([string]$Message)
  
#         Add-Type -AssemblyName System.Windows.Forms
  
#         [System.Windows.Forms.MessageBox]::Show($Message)
  
# } -PassThru )

# $msgBox.Show("Bootstrapping finished successfully")

Write-Host "Sending toast notification"
Send-Toast -Title "Done" -MessageContent "Bootstrapping finished successfully" -NullActivity -Duration reminder
Write-Host "Toast notification sent"

Stop-Transcript
