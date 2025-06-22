function Write-Syslog
{
    [CmdletBinding(DefaultParameterSetName = 'Default')]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [AllowNull()]
        [array]
        $Messages,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [ValidateScript({
                $StringCheck = $_
                return ($StringCheck -is [string] -and (-not ([string]::IsNullOrEmpty($StringCheck) -or [string]::IsNullOrWhiteSpace($StringCheck))))
            }, ErrorMessage = 'Tag must be a non-empty string')]
        [string]
        $Tag,

        [Parameter(Mandatory = $false, ValueFromPipeline = $false, ValueFromPipelineByPropertyName = $true)]
        [ValidateScript({
                $StringCheck = $_
                return ($StringCheck -is [string] -and (-not ([string]::IsNullOrEmpty($StringCheck) -or [string]::IsNullOrWhiteSpace($StringCheck))))
            }, ErrorMessage = 'Logger level must be a non-empty string')]
        [string]
        $Level = [string]::Empty,

        [Parameter(Mandatory = $false, ValueFromPipeline = $false, ValueFromPipelineByPropertyName = $true)]
        [ValidateSet("output",
            "stdout",
            "stderr",
            "error",
            "critical",
            "verbose",
            "warning",
            "info",
            "information",
            "host",
            "debug", IgnoreCase = $true)]
        [string]
        $PwshLevel = 'stdout',

        [Parameter(Mandatory = $false, ValueFromPipeline = $false, ValueFromPipelineByPropertyName = $true)]
        [ValidateScript({
                $StringCheck = $_
                return ($StringCheck -is [string] -and (-not ([string]::IsNullOrEmpty($StringCheck) -or [string]::IsNullOrWhiteSpace($StringCheck))))
            }, ErrorMessage = 'Timestamp format must be a non-empty string')]
        [string]
        $TimestampFormat = 'o',

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [switch]
        $Testing,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [switch]
        $IsError,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [switch]
        $LocalEcho,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [array]
        $ExtraArgs = @()

    )

    function Timestamp
    {
        [CmdletBinding()]
        [OutputType([string])]
        param (
        )
        return "[$(([DateTime]::now).ToString($TimestampFormat).Trim())]"
    }

    [string] $LocalOut = ''

    if (
        ($null -eq $Messages) -or
        ($Messages -isnot [string] -and $Messages -isnot [array]) -or
        ($Messages -is [array] -and $Messages.Count -le 0) -or
        ($Messages -is [string] -and [string]::IsNullOrEmpty($Messages)) -or
        ($Messages -is [string] -and [string]::IsNullOrWhiteSpace($Messages)) -or
        ($Messages -is [string] -and $Messages.Length -le 0)
    )
    {
        Write-Debug -Message "$(Timestamp) Received a null, empty, or whitespace message, or an invalid `$Messages object, returning."
        return
    }

    [hashtable] $OutputStreams = @{
        output      = "Microsoft.PowerShell.Utility\Write-Output"
        stdout      = "Microsoft.PowerShell.Utility\Write-Host"
        host        = "Microsoft.PowerShell.Utility\Write-Host"
        stderr      = "Microsoft.PowerShell.Utility\Write-Error"
        error       = "Microsoft.PowerShell.Utility\Write-Error"
        critical    = "Microsoft.PowerShell.Utility\Write-Error"
        verbose     = "Microsoft.PowerShell.Utility\Write-Verbose"
        warning     = "Microsoft.PowerShell.Utility\Write-Warning"
        info        = "Microsoft.PowerShell.Utility\Write-Information"
        information = "Microsoft.PowerShell.Utility\Write-Information"
        debug       = "Microsoft.PowerShell.Utility\Write-Debug"
    }

    [string] $LoggerPath = $((which logger 2>&1).Trim())

    if ($LoggerPath -is [System.Management.Automation.ErrorRecord])
    {
        Write-Warning -Message "$(Timestamp) 'logger' command not found, messages will not go to syslog."
        $LoggerPath = $null
    }

    [string] $LoggerCmd = "$LoggerPath"
    [string] $LoggerArgs = "-e "
    [string] $PwshLoggerCmd = ($PwshLevel -in $OutputStreams.Keys) ? $OutputStreams[$PwshLevel] : $OutputStreams['stdout']

    if ($Testing)
    {
        $LoggerArgs += '--no-act '
    }

    if ($IsError)
    {
        $LoggerArgs += '-s '
        $PwshLoggerCmd = $OutputStreams['stderr']
    }

    if ($Tag -is [string] -and $Tag.Length -gt 0)
    {
        $LoggerArgs += "-t $Tag "
        $LocalOut += "[$Tag] "
    }

    if ($Level -is [string] -and $Level.Length -gt 0)
    {
        $LoggerArgs += "-p $Level "
    }

    if ($ExtraArgs -is [array] -and $ExtraArgs.Count -gt 0)
    {
        foreach ($ExtraArg in $ExtraArgs)
        {
            if ($ExtraArg -is [string] -and (-not ([string]::IsNullOrEmpty($ExtraArg) -or [string]::IsNullOrWhiteSpace($ExtraArgs))))
            {
                $LoggerArgs += "$ExtraArg "
            }
            else
            {
                Write-Warning -Message "$(Timestamp) Received an invalid extra argument, expected a non-empty string, got: $($ExtraArg.GetType().FullName)"
            }
        }
    }

    foreach ($Message in $Messages)
    {
        if ($Message -is [string])
        {
            $Message = $Message.Trim()
        }
        elseif ($Message -is [array])
        {
            $Message = $Message -join ' '
        }
        else
        {
            Write-Warning -Message "$(Timestamp) Received an invalid message type, expected string or array, got: $($Message.GetType().FullName)"
            continue
        }

        if ($LoggerPath)
        {
            Start-Process -FilePath "$LoggerCmd" -ArgumentList "$LoggerArgs $Message"
        }

        if ($LocalEcho)
        {
            $Message = "${LocalOut}${Message}"
            Invoke-Expression -Command "$($PwshLoggerCmd) '$(Timestamp) $Message'"
        }
    }
}
