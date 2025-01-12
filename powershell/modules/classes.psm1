#region TimeDefinitions class

class TimeDefinitions
{
    [array] $Days
    [array] $Daily
    [array] $Everyday
    [array] $Workweek
    [array] $Weekdays
    [array] $Weekends
    [array] $Months
    [ordered] $Bools
    [datetime] $Today
    [datetime] $Yesterday
    [datetime] $Tomorrow
    [datetime] $LastWeek
    [datetime] $NextWeek
    [datetime] $LastMonth
    [datetime] $NextMonth
    [datetime] $LastYear
    [datetime] $NextYear
    hidden [ordered] $TruthTableWeekday
    hidden [ordered] $TruthTableWeekend
    hidden [enum] $DaysEnum
    hidden [enum] $MonthsEnum


    TimeDefinitions()
    {
        $this.Days = [Days]::GetNames([Days])
        $this.Months = [Months]::GetNames([Months])
        $this.DaysEnum = [Days]::new()
        $this.MonthsEnum = [Months]::new()
        $this.Daily = $this.Days
        $this.Everyday = $this.Days
        $this.Bools = [ordered]@{}
        $this.Today = [datetime]::Today
        $this.Yesterday = $this.Today.AddDays(-1)
        $this.Tomorrow = $this.Today.AddDays(1)
        $this.LastWeek = $this.Today.AddDays(-7)
        $this.NextWeek = $this.Today.AddDays(7)
        $this.LastMonth = $this.Today.AddDays(-30.4375)
        $this.NextMonth = $this.Today.AddDays(30.4375)
        $this.LastYear = $this.Today.AddDays(365.25)
        $this.NextYear = $this.Today.AddDays(-365.25)
        $this.TruthTableWeekday = [ordered]@{"IsWorkweekDay" = $true; "IsWeekday" = $true; "IsWeekend" = $false }
        $this.TruthTableWeekend = [ordered]@{"IsWorkweekDay" = $false; "IsWeekday" = $false; "IsWeekend" = $true }

        foreach ($Day in $this.Days)
        {
            if (($Day -eq "Saturday") -or ($Day -eq "Sunday"))
            {
                $this.Weekends += $Day
                $this.Bools.Add("$Day", $this.TruthTableWeekend)
            }
            else
            {
                $this.Weekdays += $Day
                $this.Workweek += $Day
                $this.Bools.Add("$Day", $this.TruthTableWeekday)
            }
        }
    }

    hidden [bool] _IsDay([string] $Day)
    {
        return ($Day -in $this.Days)
    }

    hidden [bool] _IsDay([datetime] $Day)
    {
        return ($Day.DayOfWeek -in $this.Days)
    }

    hidden [bool] _IsWeekday([string] $Day)
    {
        if ($Day -notin $this.Days)
        {
            return $false
        }

        if ($Day -notin $this.Weekdays)
        {
            return $false
        }

        return $true
    }

    hidden [bool] _IsWeekday([datetime] $Day)
    {
        if ($Day.DayOfWeek -notin $this.Days)
        {
            return $false
        }

        if ($Day.DayOfWeek -notin $this.Weekdays)
        {
            return $false
        }

        return $true
    }

    hidden [bool] _IsWeekend([string] $Day)
    {
        if ($Day -notin $this.Days)
        {
            return $false
        }

        if ($Day -notin $this.Weekends)
        {
            return $false
        }

        return $true
    }

    hidden [bool] _IsWeekend([datetime] $Day)
    {
        if ($Day.DayOfWeek -notin $this.Days)
        {
            return $false
        }

        if ($Day.DayOfWeek -notin $this.Weekends)
        {
            return $false
        }

        return $true
    }

    static [bool] IsDay([string] $Day)
    {
        $TimeDef = [TimeDefinitions]::new()
        return ($Day -in $TimeDef.Days)
    }

    static [bool] IsDay([datetime] $Day)
    {
        $TimeDef = [TimeDefinitions]::new()
        return ($Day.DayOfWeek -in $TimeDef.Days)
    }

    static [bool] IsWeekday([string] $Day)
    {
        $TimeDef = [TimeDefinitions]::new()
        return $TimeDef._IsWeekday($Day)
    }

    static [bool] IsWeekday([datetime] $Day)
    {
        $TimeDef = [TimeDefinitions]::new()
        return $TimeDef._IsWeekday($Day)
    }

    static [bool] IsWeekend([string] $Day)
    {
        $TimeDef = [TimeDefinitions]::new()
        return $TimeDef._IsWeekend($Day)
    }

    static [bool] IsWeekend([datetime] $Day)
    {
        $TimeDef = [TimeDefinitions]::new()
        return $TimeDef._IsWeekend($Day)
    }

    static [TimeDefinitions] GetTimeDefinitions()
    {
        return [TimeDefinitions]::new()
    }

    static [ordered] GetTimeDefinitions([bool] $AsHashTable)
    {
        $TimeDefs = [TimeDefinitions]::new()
        $ReturnObject = [ordered]@{}

        foreach ($Property in $TimeDefs.psobject.Properties)
        {
            $ReturnObject.Add($Property.Name, $Property.Value)
        }
        return $ReturnObject
    }

}
#endregion

#region HelperFunctions class
class HelperFunctions
{
    [hashtable] ConvertFromArgsList([string] $ArgsList)
    {
        [hashtable] $ReturnHash = @{}

        if ([HelperFunctions]::TestIsNullOrEmpty($ArgsList))
        {
            return $ReturnHash
        }

        [array] $ArgsArray = ($ArgsList.Substring(1).Substring(0, ($ArgsList.Length - 2)).Split(',').Trim())

        foreach ($Entry in $ArgsArray)
        {
            ([string] $Key, [string] $Value) = $Entry.Split('=').Trim()

            if ($ReturnHash.ContainsKey($Key))
            {
                Write-Debug -Message "Key $Key already exists in the hashtable. Keeping original value: $($ReturnHash[$Key])"
                continue
            }
            else
            {
                $ReturnHash.Add($Key, $Value)
            }
        }

        return $ReturnHash
    }

    [bool] TestValidFilename([object] $Filename)
    {
        if ($this._TestIsNullOrEmpty($Filename))
        {
            return $false
        }

        $IndexOfInvalidChar = $FileName.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars())
    
        # IndexOfAny() returns the value -1 to indicate no such character was found
        return $IndexOfInvalidChar -eq -1
    }

    [bool] _TestIsNullOrEmpty([object] $TestObject)
    {
        return [HelperFunctions]::TestIsNullOrEmpty($TestObject)
    }

    static [bool] TestIsNullOrEmpty([object] $TestObject)
    {
        #$MethodName = (Get-PSCallStack).FunctionName[0]

        if ($null -eq $TestObject)
        {
            return $true
        }

        try
        {
            $ObjectType = $TestObject.GetType().Name
        }
        catch
        {
            return $true
        }

        switch ($ObjectType)
        {
            'String'
            {
                return ([String]::IsNullOrEmpty($TestObject)) -or $false
                break
            }

            'DateTime'
            {
                try
                {
                    New-TimeSpan -Start $TestObject -End (Get-Date) -ErrorAction Stop | Out-Null
                    return $false
                }
                catch
                {
                    return $true
                }
                break
            }

            'PSCustomObject'
            {
                return (@($TestObject.psobject.Properties).Count -eq 0) -or ($null -eq $TestObject)
                break
            }

            'Hashtable'
            {
                return ($TestObject.Count -eq 0) -or ($null -eq $TestObject)
                break
            }

            Default
            {
                Write-Array -Messages @(
                    "Object of type $ObjectType does not have a specific switch case",
                    "Testing with most likely attributes"
                ) -MessageType Debug
                return ($TestObject.Count -eq 0) -or ($TestObject.Length -eq 0) -or ($null -eq $TestObject)
                break
            }
        }

        ## We should never get here

        Write-Error "Object of type $ObjectType is not supported"
        throw New-Object System.NotSupportedException
    }

    [string] GetMaskedReadHost([object] $Prompt)
    {

        if ($null -eq $Prompt)
        {
            $TempString = Read-Host -AsSecureString -Prompt "Enter your response (will be masked)"
            $String = (New-Object PSCredential 0, $TempString).GetNetworkCredential().Password
            Remove-Variable TempString
            return $String
        }

        else
        {
            $TempString = Read-Host -AsSecureString -Prompt "$Prompt"
            $String = (New-Object PSCredential 0, $TempString).GetNetworkCredential().Password
            Remove-Variable TempString
            return $String
        }
    }

    [string] GetStringHash([object] $InputString, [string] $HashName, [bool] $Prompt)
    {
        $ValidAlgorithms = @(
            "MD5",
            "SHA",
            "SHA1",
            "SHA256",
            "SHA-256",
            "SHA384",
            "SHA-384",
            "SHA512",
            "SHA-512",
            "System.Security.Cryptography.SHA1",
            "System.Security.Cryptography.HashAlgorithm",
            "System.Security.Cryptography.MD5",
            "System.Security.Cryptography.SHA256",
            "System.Security.Cryptography.SHA384",
            "System.Security.Cryptography.SHA512"
        )
        if ($HashName -notin $ValidAlgorithms)
        {
            Write-Error -Message "The algorithm `"$HashName`" is not a valid hash algorithm. Please provide a valid hash algorithm from the following list:"
            Write-Error -Message $ValidAlgorithms
        }
        [string] $String = $InputString

        if ($this._TestIsNullOrEmpty($InputString))
        {
            Write-Warning -Message "No string provided to hash, providing hash of an empty string"
        }

        if ($Prompt)
        {
            $String = $this.GetMaskedReadHost("Enter the string to hash")
        }

        $Bytes = [System.Text.Encoding]::UTF8.GetBytes($String)
        $HashAlgorithm = [System.Security.Cryptography.HashAlgorithm]::Create("$HashName")
        $StringBuilder = New-Object System.Text.StringBuilder
    
        $HashAlgorithm.ComputeHash($Bytes) |
        ForEach-Object {
            $null = $StringBuilder.Append($_.ToString("x2"))
        }

        return $StringBuilder.ToString()

    }

    [void] static WriteArray([array] $Messages)
    {
        [HelperFunctions]::WriteArray($Messages, [MessageChannelTypes]::Output, $false)
    }

    [void] static WriteArray([array] $Messages, [bool] $Force)
    {
        [HelperFunctions]::WriteArray($Messages, [MessageChannelTypes]::Output, $Force)
    }

    [void] static WriteArray([array] $Messages, [MessageChannelTypes] $MessageType)
    {
        [HelperFunctions]::WriteArray($Messages, $MessageType, $false)
    }


    [void] static WriteArray([array] $Messages, [MessageChannelTypes] $MessageType, [bool] $Force)
    {

        $OriginalPreferences = @{
            Information = @{
                VarName  = 'InformationPreference'
                VarValue = $InformationPreference
            }
            Verbose     = @{
                VarName  = 'VerbosePreference'
                VarValue = $VerbosePreference
            }
            Debug       = @{
                VarName  = 'DebugPreference'
                VarValue = $DebugPreference
            }
            Error       = @{
                VarName  = 'ErrorPreference'
                VarValue = $ErrorActionPreference
            }
            Warning     = @{
                VarName  = 'WarningPreference'
                VarValue = $WarningPreference
            }
        }

        switch ($MessageType)
        {
            ([MessageChannelTypes]::Information)
            {
                foreach ($Message in $Messages)
                {
                    if ($Message -is [array])
                    {
                        [HelperFunctions]::WriteArray($Message, $MessageType, $Force)
                        continue
                    }
                    if ($Force)
                    {
                        $InformationPreference = 'Continue'
                        Write-Information -MessageData $Message -InformationAction Continue
                    }
                    else
                    {
                        Write-Information -MessageData $Message
                    }
                }
                break
            }

            ([MessageChannelTypes]::Output)
            {
                foreach ($Message in $Messages)
                {
                    Write-Output -InputObject $Message
                }
                break
            }

            ([MessageChannelTypes]::Verbose)
            {
                foreach ($Message in $Messages)
                {
                    if ($Force)
                    {
                        $VerbosePreference = 'Continue'
                        Write-Verbose -Message $Message -Verbose
                    }
                    else
                    {
                        Write-Verbose -Message $Message
                    }
                }
                break
            }

            ([MessageChannelTypes]::Debug)
            {
                foreach ($Message in $Messages)
                {
                    if ($Force)
                    {
                        $DebugPreference = 'Continue'
                        Write-Debug -Message $Message -Debug
                    }
                    else
                    {
                        Write-Debug -Message $Message
                    }
                }
                break
            }

            ([MessageChannelTypes]::Warning)
            {
                foreach ($Message in $Messages)
                {
                    $WarningPreference = 'Continue'
                    Write-Warning -Message $Message
                }
                break
            }

            ([MessageChannelTypes]::Error)
            {
                foreach ($Message in $Messages)
                {
                    $ErrorActionPreference = 'Continue'
                    Write-Error -Message $Message
                }
                break
            }

            Default
            {
                foreach ($Message in $Messages)
                {
                    Write-Output -InputObject $Message
                }
                break
            }
        }
    
        if ($Force)
        {
            foreach ($Preference in $OriginalPreferences.Keys)
            {
                Set-Variable -Name "$($OriginalPreferences[$Preference].VarName)" -Value $($OriginalPreferences[$Preference].VarValue)
            }
        }
    }

    [array] _RemoveEmptyElements([array] $InputObject)
    {
        return [HelperFunctions]::RemoveEmptyElements($InputObject)
    }

    [bool] _IsValidEmailAddress([string] $Email)
    {
        return [HelperFunctions]::IsValidEmailAddress($Email)
    }

    [bool] static IsValidEmailAddress([string] $Email)
    {
        try
        {
            $TestEmail = [MailAddress]::new("$Email")
            # Just to get PSScriptAnalyzer to shut up
            $TestEmail.Dispose()
            return $true
        }
        catch
        {
            return $false
        }
    }

    [array] static RemoveEmptyElements([array] $InputArray)
    {
        $ReturnArray = @()
        foreach ($Object in $InputArray)
        {
            if (([System.String]::IsNullOrWhiteSpace($Object)) -or ([HelperFunctions]::TestIsNullOrEmpty($Object)))
            {
                continue
            }
            else
            {
                $ReturnArray += $Object
            }
        }

        return $ReturnArray
    }

}

#endregion

class Config
{
    [ScriptVars] $ScriptVars
    [PSCustomObject] $Config
    [hashtable] $ConfigAsHash
    [array] $ConfigAsHashKeys
    [array] $ConfigAsArray
    [string] $ConfigAsText

    Config()
    {
        $this.Init()
    }

    Config([System.IO.FileInfo] $FileObject)
    {
        $this.Config($FileObject.FullName)
    }

    Config([string] $ConfigPath)
    {
        $this.Init()
        $this.ScriptVars.ConfigFile = [HelperFunctions]::TestValidFilename($ConfigPath) ? $ConfigPath : { $null; throw "The file path `"$ConfigPath`" is invalid." }
    }

    ToString() {
        Write-Array -MessageType Debug -Messages "Test for overriding the ToString() method" -Force
    }

    [PSCustomObject] GetConfig() {
        return $this.Config
    }

    [hashtable] GetConfig([bool] $AsHashTable) {
        return $this.ConfigAsHash
    }

    [object] GetConfigValue([string] $Key) {
        return $this.Config.$Key
    }

    ReadConfig() {
        $this.ReadConfig($this.ScriptVars.ConfigFile)
    }

    ReadConfig([string] $ConfigPath)
    {
        if (!(Test-Path -Path $ConfigPath))
        {
            Write-Array -MessageType Error -Messages  @("The file `"$ConfigPath`" does not exist. Please provide a valid configuration file.")
            throw $ConfigPath
        }
        try {
            Get-Content -Path $ConfigPath -Tail 1 -ErrorAction Stop
        }
        catch {
            $ReadError = $_
            Write-Array -MessageType Error -Messages  @(
                "Error reading the config file at `"$ConfigPath`"",
                "This is likely due to an empty file or incorrect permissions.",
                "The error was: $($ReadError | Select-Object * | Out-String)"
                )
            throw $ConfigPath
        }
        try {
            $RawConfig = Get-Content -Path $ConfigPath -Raw -ErrorAction Stop
            $this.ConfigAsText = $RawConfig
            $this.Config = $RawConfig | ConvertFrom-Yaml -ErrorAction Stop
            $this.ConfigAsHash = [hashtable] $this.Config
            $this.ConfigAsHashKeys = [array] $this.ConfigAsHash.Keys
            $this.ConfigAsArray = [array] $RawConfig.Split("`n")
        }
        catch {
            $ParseError = $_
            Write-Array -MessageType Error -Messages  @(
                "Error parsing the config file at `"$ConfigPath`"",
                "This is likely due to a formatting error in the YAML file.",
                "The error was: $($ParseError | Select-Object * | Out-String)"
                )
            throw $ConfigPath
        }
    }

    ShowConfig() {
        if ([HelperFunctions]::TestIsNullOrEmpty($this.Config))
        {
            throw "No configuration has been loaded. Please load a configuration file with the .Read() method."
        }
        $YAMLConfig = ($this.Config | ConvertTo-Yaml).ToString()
        Write-Array -MessageType Information -Messages "$YAMLConfig"
    }

    [void] Init()
    {
        $this.ScriptVars = [ScriptVars]::new()
    }


}

#region ScriptVars class
class ScriptVars : Config
{
    [string] $Name
    [Object] $Callstack
    [Int32] $CallStackLength
    [string] $ScriptName
    [string] $ScriptBasename
    [string] $ScriptBasePath
    [string] $ScriptPath
    [string] $ConfgfilePath
    [string] $ConfigFilename
    [string] $ConfigFile
    [PSCustomObject] $Config
    [bool] $Logging
    [string] $LogPath
    [string] $LogFilename
    [string] $LogFile
    [string] $HostOS
    [hashtable] $Arguments
    [bool] $HostIsWindows
    [bool] $HostIsLinux
    [datetime] $StartTime
    [hashtable] $EnvVars
    [HelperFunctions] $HelperObject
    [version] $Version

    ScriptVars()
    {
        $this.Init()
    }

    Init()
    {
        $this.EnvVars = @{}
        $this.EnvVars.Add("AtStart", [System.Environment]::GetEnvironmentVariables())
        $this.EnvVars.Add("ScriptSet", @{})
        $this.StartTime = [datetime]::Now
        $this.HelperObject = [HelperFunctions]::new()
        $this.Name = $this.ToString()
        $this.HostIsWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
        $this.HostIsLinux = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)

        if ($this.HostIsWindows)
        {
            $this.HostOS = "Windows"
        }
        elseif ($this.HostIsLinux)
        {
            $this.HostOS = "Linux"
        }

        $this.CallStack = (Get-PSCallStack)
        $this.CallStackLength = ($this.Callstack | Measure-Object).Count
        if ($this.CallStackLength -ge 2)
        {
            $this.Callstack = $this.Callstack[$this.CallStackLength - 2]
            $this.ScriptPath = $this.Callstack.ScriptName
            try
            {
                $this.ScriptName = (Split-Path -Path "$($this.ScriptPath)" -Leaf).Split('.')[0]
                $this.ScriptBasename = Split-Path -Path "$($this.ScriptPath)" -Leaf
                $this.ScriptBasePath = (Split-Path -Path "$($this.ScriptPath)" -Parent)
            }
            catch
            {
                $this.ScriptName = "no_script"
                $this.ScriptBasename = "no_script"
                $this.ScriptBasePath = (Get-Location).ToString()
            }

        }
        elseif ($this.CallStackLength -eq 1)
        {
            $this.ScriptName = (Split-Path -Path "$($this.Callstack.ScriptName)" -Leaf).Split('.')[0]
            $this.ScriptBasename = "no_script"
            $this.ScriptPath = $this.ScriptBasePath = (Get-Location).ToString()
        }

        $this.Logging = $false
        $this.LogPath = Join-Path -Path "$($this.ScriptBasePath)" -ChildPath "logs"
        $this.LogFilename = "$($this.ScriptBasename)_$($this.StartTime.ToString('yyyy-MM-dd.HH.mm.ss.fff')).log"
        $this.LogFile = Join-Path -Path "$($this.LogPath)" -ChildPath "$($this.LogFilename)"
        $this.ConfgfilePath = Join-Path -Path "$($this.ScriptBasePath)" -ChildPath "config"
        $this.ConfigFilename = "$($this.ScriptName).config.yml"
        $this.ConfigFile = Join-Path -Path "$($this.ConfgfilePath)" -ChildPath "$($this.ConfigFilename)"
        $this.Arguments = $this.HelperObject.ConvertFromArgsList($this.Callstack.Arguments)
    }
}

#endregion

#region MailObject class
class MailObject
{
    [string] $From
    [array] $Recipients
    [string] $Subject
    [string] $Body
    [string] $SMTPServer
    [Int32] $Port
    [array] $Attachments
    [string] $Logfile
    [bool] $HTML
    hidden [string] $Name
    hidden [HelperFunctions] $HelperObject = [HelperFunctions]::new()
    hidden [string] $MethodName
    hidden [string] $Caller

    MailObject()
    {
        $this.Init()
    }

    MailObject([hashtable] $Settings)
    {
        $this.Init()
        try
        {
            $this.Port = [Int32]::Parse($Settings.Port)
        }
        catch
        {
            Write-Array -MessageType Error -Messages "Port `"$($Settings.Port)`" is invalid, falling back to port 587"
            $this.Port = 587
        }
        $this.MailObject(
            $Settings.From,
            $Settings.Recipients,
            $Settings.Subject,
            $Settings.Body,
            $Settings.SMTPServer,
            $this.Port,
            $Settings.Logfile,
            $Settings.Attachments,
            $Settings.HTML
        )
    }

    MailObject(
        [string] $From,
        [array] $Recipients,
        [string] $Subject,
        [string] $Body,
        [string] $SMTPServer,
        [Int32] $Port,
        [string] $Logfile,
        [array] $Attachments,
        [bool] $HTML
    )
    {
        $this.Init()
        $this.From = $From
        $this.Recipients = $this.AddMailRecipients($Recipients)
        $this.Subject = $Subject
        $this.Body = $Body
        $this.SMTPServer = $SMTPServer
        $this.Port = $Port
        $this.Logfile = $Logfile
        $this.Attachments = $Attachments
        $this.HTML = $HTML
    }

    [void] Init()
    {
        $this.Name = $this.ToString()
        $this.MethodName = (Get-PSCallStack).FunctionName[0]
        $this.Caller = "[$($this.Name)]::$($this.MethodName)"
        $this.HelperObject = [HelperFunctions]::new()

        $this.From = [System.String]::Empty
        $this.Recipients = @()
        $this.Subject = "No Subject Provided"
        $this.Body = [MailObject]::GetDefaultBody()
        $this.SMTPServer = [System.String]::Empty
        $this.Port = 587
        $this.Attachments = @()
        $this.Logfile = [System.String]::Empty
        $this.HTML = $true
    }

    [bool] IsValidEmailAddress([string] $Email)
    {
        return [HelperFunctions]::IsValidEmailAddress($Email)
    }

    [string] ValidateEmailAddress([string] $Recipient)
    {
        if ([System.String]::IsNullOrWhiteSpace($Recipient))
        {
            Write-Array -MessageType Error -Messages "Recipient address `"$Recipient`" is null, empty, or invalid; skipping."
            return [System.String]::Empty
        }
        elseif ($this.IsValidEmailAddress($Recipient))
        {
            Write-Array -MessageType Debug -Messages "Recipient address `"$Recipient`" is valid."
            return $Recipient
        }
        else
        {
            Write-Array -MessageType Error -Messages "Recipient address `"$Recipient`" is null, empty, or invalid; skipping."
            return $null
        }
    }

    [void] AddMailRecipient([string] $Recipient)
    {
        $this.AddMailRecipients($Recipient)
    }

    [void] AddMailRecipients([string] $Recipient)
    {
        $this.Recipients += $this.ValidateEmailAddress($Recipient)
        $this.Recipients = $this.HelperObject._RemoveEmptyElements($this.Recipients)
    }

    [void] AddMailRecipients([array] $Recipients)
    {
        foreach ($Recipient in $Recipients)
        {
            $this.Recipients += $this.ValidateEmailAddress($Recipient)
        }
        $this.Recipients = $this.HelperObject._RemoveEmptyElements($this.Recipients)
    }

    [void] AddMailRecipients([PSCustomObject] $Config, [PSCustomObject] $MailNode)
    {
        if (($null -ne $MailNode.recipients))
        {
            if ($Config.Params.Testing)
            {
                foreach ($Recipient in $MailNode.recipients.testrecipient)
                {
                    $this.Recipients += $this.ValidateEmailAddress($Recipient)
                }
            }
            else
            {
                foreach ($Recipient in $MailNode.recipients.normalrecipients)
                {
                    $this.Recipients += $this.ValidateEmailAddress($Recipient)
                }
            }
        }

        $this.Recipients = $this.HelperObject._RemoveEmptyElements($this.Recipients)
    }

    [void] Send()
    {
        if ($this.HelperObject._ObjectIsNullOrEmpty($this.From))
        {
            Write-Array -MessageType Error -Messages "From address cannot be empty. Please add a From address."
            throw $this.From
        }
        elseif ($this.HelperObject._ObjectIsNullOrEmpty($this.Recipients))
        {
            Write-Array -MessageType Error -Messages "Recipients list cannot be empty. Please add recipients."
            throw $this.Recipients
        }
        elseif ($this.HelperObject._ObjectIsNullOrEmpty($this.SMTPServer))
        {
            Write-Array -MessageType Error -Messages "SMTP server cannot be empty. Please set the SMTP server."
            throw $this.SMTPServer
        }
        elseif ($this.HelperObject._ObjectIsNullOrEmpty($this.Port))
        {
            Write-Array -MessageType Error -Messages "Port cannot be empty. Please set the SMTP port."
            throw $this.Port
        }
        elseif (([Int32] $this.Port -lt 1) -or ([Int32] $this.Port -gt 65535))
        {
            Write-Array -MessageType Error -Messages "Port number $($this.Port) is invalid. Port must be between 1 and 65535. Please set the SMTP port to a valid port."
            throw $this.Port
        }

        $this.Recipients = $this.HelperObject._RemoveEmptyElements($this.Recipients)
        foreach ($Recipient in $this.Recipients)
        {
            $SMTPMessage = [System.Net.Mail.MailMessage]::new($this.From, $Recipient, $this.Subject, $this.Body)
            $SMTPMessage.IsBodyHtml = $this.HTML
            if ($null -ne $this.Logfile -and $this.Logfile.Length -ne 0)
            {
                if ([System.IO.File]::Exists("$($this.Logfile)"))
                {
                    $Attachment = [System.Net.Mail.Attachment]::new("$($this.Logfile)")
                    $SMTPMessage.Attachments.Add($Attachment)
                }
                else
                {
                    Write-Array -MessageType Information -Messages "Logfile `"$($this.Logfile)`" does not exist, no logfile will be attached."
                }
            }

            if ($this.Attachments.Count -gt 0)
            {
                foreach ($Attachment in $this.Attachments)
                {
                    $Attachment = [System.Net.Mail.Attachment]::new($Attachment)
                    $SMTPMessage.Attachments.Add($Attachment)
                }
            }

            $SMTPClient = [System.Net.Mail.SmtpClient]::new($this.SMTPServer, $this.Port)

            if ($this.Port -eq 587 -or $this.Port -eq 465)
            {
                $this.HelperObject.SetTLSVersion('Tls12')
                $SMTPClient.EnableSsl = $true
            }
            try
            {
                [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { return $true }
                $SMTPClient.Send($SMTPMessage)
                if ($SMTPMessage.Attachments.Count -gt 0)
                {
                    $SMTPMessage.Attachments.Dispose()
                }
                $SMTPClient.Dispose()
            }
            catch
            {
                $MailError = $_
                Write-Array -MessageType Error -Messages "Error sending message. The error was: $($MailError | Select-Object * | Out-String)"
            }
        }
    }

    [array] static GetMailRecipients([PSCustomObject] $Config, [PSCustomObject] $MailNode)
    {
        $ReturnObject = [MailObject]::new()
        return $ReturnObject.AddMailRecipients($Config, $MailNode)
    }

    [string] static GetDefaultBody()
    {
        return [MailObject]::GetDefaultBody("Unversioned")
    }

    [string] static GetDefaultBody([string] $Version)
    {
        $ScriptInfo = [ScriptVars]::new()

        return @"
        <b>Script Script Name:</b> $($ScriptInfo.ScriptName)<br>
        <b>Script Basename:</b> $($ScriptInfo.ScriptBasename)<br>
        <b>Script Version:</b> $Version<br>
"@

    }
}

#endregion

function Write-Array
{
    param(
        [Parameter(Mandatory = $true)]
        [array]
        $Messages,

        [Parameter(Mandatory = $false)]
        [MessageChannelTypes]
        $MessageType,

        [Parameter(Mandatory = $false)]
        [switch]
        $Force
    )

    if (($null -ne $MessageType) -and ($null -ne $Force))
    {
        [HelperFunctions]::WriteArray($Messages, $MessageType, $Force)
    }
    elseif ($null -ne $MessageType)
    {
        [HelperFunctions]::WriteArray($Messages, $MessageType, $false)
    }
    elseif ($null -ne $Force)
    {
        [HelperFunctions]::WriteArray($Messages, [MessageChannelTypes]::Output, $Force)
    }
    else
    {
        [HelperFunctions]::WriteArray($Messages, [MessageChannelTypes]::Output, $false)
    }
}

function Get-TimeDefinitions
{
    return [TimeDefinitions]::new()
}

function Get-DaysEnum
{
    $Enums = [Enums]::new()
    return $Enums.Days
}

function Get-MonthsEnum
{
    $Enums = [Enums]::new()
    return $Enums.Months
}

$FunctionsToExport = @(
    "Get-DaysEnum", 
    "Get-MonthsEnum",
    "Write-Array"
)

Export-ModuleMember -Function $FunctionsToExport


#region Enums
[Flags()]
enum Months
{
    January = 1
    February = 2
    March = 4
    April = 6
    May = 8
    June = 10
    July = 12
    August = 14
    September = 16
    October = 18
    November = 20
    December = 22
}

[Flags()]
enum Days
{
    Sunday = 1
    Monday = 2
    Tuesday = 4
    Wednesday = 6
    Thursday = 8
    Friday = 10
    Saturday = 12
}

enum MessageChannelTypes
{
    Information
    Output
    Verbose
    Debug
    Warning
    Error
    Host
}

#endregion

#region HelpTexts class
class HelpTexts
{
    hidden [hashtable] $HelpText

    HelpTexts()
    {
        $this.Init()
    }

    hidden [void] _GetAvailable()
    {
        $this._GetAvailable($false)
    }

    hidden [array] _GetAvailable([bool] $ReturnAnArray)
    {
        return $this._GetAvailable($true)
    }

    static [void] GetAvailable()
    {
        [HelpTexts]::GetAvailable($false)
    }

    static [array] GetAvailable([bool] $ReturnAnArray)
    {
        $HelpObject = [HelpTexts]::new()
        $ReturnObject = @()

        foreach ($Entry in $HelpObject.HelpText.Keys)
        {
            $ReturnObject += $Entry
        }

        if ($ReturnAnArray)
        {
            return $ReturnObject
        }

        Write-Array -Messages @("Available help topics:", $ReturnObject) -MessageType Information -Force
        
        return $null
    }

    Init()
    {
        $this.HelpText = @{}
        $this.HelpText.Add("GetStringHash", @"
<#
.SYNOPSIS
    Get a hash of a string using a specified algorithm.

.DESCRIPTION
    Get a hash of a string using a specified algorithm. The default algorithm is SHA256. See the "HashName" parameter for a list of available algorithms.

.PARAMETER Activity
    Descriptive text to the left of the progress bar.
    Defaults to a random loading message.

.PARAMETER HashName
    The hash algorithm to use. The default hash algorithm is SHA256. The following algorithms are available:

    MD5
    SHA
    SHA1
    SHA256
    SHA-256
    SHA384
    SHA-384
    SHA512
    SHA-512
    System.Security.Cryptography.SHA1
    System.Security.Cryptography.HashAlgorithm
    System.Security.Cryptography.MD5
    System.Security.Cryptography.SHA256
    System.Security.Cryptography.SHA384
    System.Security.Cryptography.SHA512

.PARAMETER Help
    Display this help information for the Get-StringHash function.

.PARAMETER NullStringHash
    Provide the hash of a null string, if for some bizarre reason you'd ever want it.

.PARAMETER Prompt
    Prompt for the string to hash without echoing the input to the terminal.

.PARAMETER String
    The string to hash.

.EXAMPLE
    Get-StringHash -String "This is a string to hash"

.EXAMPLE
    Get-StringHash -String "This is a string to hash" -HashName SHA512

.EXAMPLE
    Get-StringHash -String "This is a string to hash" -Prompt -HashName SHA384

.EXAMPLE
    Get-StringHash -NullStringHash

.INPUTS
    String

.OUTPUTS
    String

.NOTES
    Author         : Christopher Conley <chris@unnx.net>
    Prerequisite   : PowerShell Version 5.1 or higher
    License        : MIT

    Copyright © 2024 Christopher Conley
#>
"@)
    }

}

#endregion