$__LocalBinPath = "C:\Users\tool\.local\bin"
$__UserPATHOnLaunch = ([Environment]::GetEnvironmentVariables("User")).Path
$DefaultTermTitle = $host.UI.RawUI.WindowTitle
$DefaultTermTitlePID = "$DefaultTermTitle - PID: $PID"
function Set-LocalBinPath
{
    [CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'Low', DefaultParameterSetName = 'SetPath')]
    param (
        [Alias("Dir", "Directory", "DirArray", "DirectoryArray", "Bindir")]
        [Parameter(Mandatory = $true, ParameterSetName = 'SetPath', Position = 0, ValueFromPipeline = $true)]
        [object] $LocalBinDir,

        [Alias("Env", "Environment")]
        [Parameter(Mandatory = $false, ParameterSetName = 'SetPath')]
        [ValidateSet("User", "Machine", "System")]
        [string] $EnvironmentVariableType,

        [Alias("NonExistentAction")]
        [Parameter(Mandatory = $false, ParameterSetName = 'SetPath')]
        [ValidateSet("Add", "Skip", "Stop")]
        [string] $MissingAction = "Skip",

        [Alias("Recursive")]
        [Parameter(Mandatory = $false, ParameterSetName = 'SetPath')]
        [switch] $Recurse,

        [Parameter(Mandatory = $false, ParameterSetName = 'SetPath')]
        [switch] $Force,

        [Alias("Dupes", "Duplicates", "DuplicatesOK")]
        [Parameter(Mandatory = $false, ParameterSetName = 'SetPath')]
        [switch] $DupesOK,

        [Parameter(Mandatory = $true, ParameterSetName = 'ResetPath', Position = 0, ValueFromPipeline = $true)]
        [string] $OriginalPath,

        [Parameter(Mandatory = $true, ParameterSetName = 'ResetPath')]
        [switch] $Reset,

        [Alias("OriginalEnv", "OriginalEnvironment")]
        [Parameter(Mandatory = $false, ParameterSetName = 'ResetPath')]
        [ValidateSet("User", "Machine", "System")]
        [string] $OriginalVariableType
    )

    if ($PSCmdlet.ParameterSetName -eq 'ResetPath')
    {
        $OriginalEnvVarTarget = $null

        switch ($OriginalVariableType)
        {
            "User"
            {
                $OriginalEnvVarTarget = [EnvironmentVariableTarget]::User;
                break
            }
            "Machine"
            {
                $OriginalEnvVarTarget = [EnvironmentVariableTarget]::Machine;
                break
            }
            "System"
            {
                $OriginalEnvVarTarget = [EnvironmentVariableTarget]::Machine;
                break
            }
            default
            {
                $OriginalEnvVarTarget = $null;
                break
            }
        }

        if ($PSCmdlet.ShouldProcess($Reset, "Reset original PATH variable."))
        {

            try
            {

                if ($null -ne $OriginalEnvVarTarget)
                {
                    [Environment]::SetEnvironmentVariable("Path", "$OriginalPath", $OriginalEnvVarTarget)
                }
                else
                {
                    $ScriptOriginalTargetEnvVar = (([Environment]::GetEnvironmentVariables("User")).OriginalEnvTarget).ToLower()
                    if ($null -eq $ScriptOriginalTargetEnvVar)
                    {
                        [Environment]::SetEnvironmentVariable("Path", "$OriginalPath", [EnvironmentVariableTarget]::User)
                    }
                    else
                    {
                        if ($ScriptOriginalTargetEnvVar -eq "user")
                        {
                            [Environment]::SetEnvironmentVariable("Path", "$OriginalPath", [EnvironmentVariableTarget]::User)
                        }
                        else
                        {
                            [Environment]::SetEnvironmentVariable("Path", "$OriginalPath", [EnvironmentVariableTarget]::Machine)
                        }
                        [Environment]::SetEnvironmentVariable("OriginalEnvTarget", $null, [EnvironmentVariableTarget]::User)
                    }
                }

                [Environment]::SetEnvironmentVariable("CustomPathAlreadySet", $null, [EnvironmentVariableTarget]::User)
            }
            catch
            {
                $SetError = $_
                Write-Error "Could not reset environment variable `"Path`" to: $NewPath"
                throw $SetError
            }

            return
        }
    }

    ## Don't attempt to set variable again if it has already been added to PATH

    if ((!$Force) -and (([Environment]::GetEnvironmentVariables("User")).CustomPathAlreadySet -eq $true))
    {
        Write-Debug "Custom binpath has already been added to PATH, exiting..."
        return
    }

    [array] $OKTypes = @(
        "System.String",
        "System.IO.DirectoryInfo",
        "System.Array"
    )

    [bool] $BinDirIsArray = $LocalBinDir -is [System.Array]
    [string] $BinDirType = $LocalBinDir.GetType().ToString()

    if ($BinDirType -notin $OKTypes)
    {
        Write-Error "-LocalBinDir parameter must be a string, array, or [System.IO.DirectoryInfo] .NET type."
        Write-Error "-LocalBinDir type is: $BinDirType"
        throw $LocalBinDir
    }

    if (($BinDirType -eq "System.String") -and ([System.String]::IsNullOrWhiteSpace("$LocalBinDir")))
    {
        Write-Error "-LocalBinDir parameter cannot be null, empty, or whitespace."
        throw $LocalBinDir
    }

    $EnvVarTarget = $null

    switch ($EnvironmentVariableType)
    {
        "User"
        {
            $EnvVarTarget = [EnvironmentVariableTarget]::User;
            break
        }
        "Machine"
        {
            $EnvVarTarget = [EnvironmentVariableTarget]::Machine;
            break
        }
        "System"
        {
            $EnvVarTarget = [EnvironmentVariableTarget]::Machine;
            break
        }
        default
        {
            $EnvVarTarget = [EnvironmentVariableTarget]::User;
            break
        }
    }

    $TempPathArray = @()
    $FinalPathArray = @()

    if ($BinDirIsArray)
    {
        foreach ($Dir in $LocalBinDir)
        {
            $DirType = $Dir.GetType().ToString()

            switch ($DirType)
            {

                "System.String"
                {
                    $TempPathArray += $Dir;
                    break
                }

                "System.IO.DirectoryInfo"
                {
                    $TempPathArray += $Dir.FullName;
                    break
                }

                default
                {
                    Write-Error "Encountered invalid object `"$Dir`" in provided paths array, skipping."
                    continue
                }

            }
        }
    }

    else
    {
        $TempPathArray += $LocalBinDir
    }

    $WorkingArray = @()
    foreach ($ProvidedPath in $TempPathArray)
    {
        try
        {
            Write-Debug "Resolving $ProvidedPath"
            $WorkingArray += (Resolve-Path "$ProvidedPath" -ErrorAction Stop).Path
        }
        catch
        {
            $ExistsError = $_

            switch ($MissingAction)
            {

                "Add"
                {
                    Write-Host "Path `"$ProvidedPath`" is not a valid path, but adding anyway because '-MissingAction Add' was specified."
                    $WorkingArray += $ProvidedPath;
                    break
                }

                "Stop"
                {
                    Write-Error "The path `"$ProvidedPath`" is not a valid path, exiting because '-MissingAction Stop' was specified."
                    throw $ExistsError
                }

                default
                {
                    Write-Host "Path `"$ProvidedPath`" is not a valid path, skipping."
                    continue
                }
            }
        }
    }

    Write-Debug "Final WorkingArray is: $WorkingArray"

    $TempPathArray = $WorkingArray
    if ($Recurse)
    {
        Write-Debug "Recursively walking directories"
        foreach ($PathItem in $TempPathArray)
        {
            [array] $ChildDirs = @()

            Write-Debug "Getting child directories of path: $PathItem"
            $ChildDirs = Get-ChildItem "$PathItem" -Directory -Recurse -ErrorAction SilentlyContinue
            foreach ($ChildItem in $ChildDirs)
            {
                $TempPathArray += $ChildItem
            }
        }
    }

    [array] $FinalPathArray = ($TempPathArray | Where-Object { !([System.String]::IsNullOrWhiteSpace($_)) } | Select-Object -Unique)
    [array] $CurrentPathArray = (([Environment]::GetEnvironmentVariables("User")).Path).Split(';')

    foreach ($Cpath in $CurrentPathArray)
    {
        if ($Cpath -notin $FinalPathArray)
        {
            Write-Debug "Adding existing path `"$Cpath`" to the final path array."
            $FinalPathArray += $Cpath
        }
        else
        {
            if ($DupesOK)
            {
                Write-Debug "Existing path `"$Cpath`" was already in final path array, but adding anyway because -DupesOK was specified."
                $FinalPathArray += $Cpath
            }
            else
            {
                Write-Debug "Not adding duplicate path `"$Cpath`" to the final path array since -DupesOK was not specified."
                continue
            }
        }
    }

    if (!$DupesOK)
    {
        $TempPathArray = @()
        Write-Debug "Removing any empty, null, or duplicate paths"
        $FinalPathArray = $FinalPathArray | Select-Object -Unique
        foreach ($Dpath in $FinalPathArray)
        {
            if ([System.String]::IsNullOrWhiteSpace($Dpath))
            {
                Write-Debug "Found an empty or null path, removing from final path array."
                continue
            }
            else
            {
                $TempPathArray += $Dpath
            }
        }
        $FinalPathArray = $TempPathArray
    }

    Write-Debug "Final Path array is: $FinalPathArray"

    [string] $ExistingPath = [string] ([Environment]::GetEnvironmentVariables("User")).Path
    [string] $NewPath = [System.String]::Empty
    

    foreach ($BinDir in $FinalPathArray)
    {
        [string] $NewPath += "$BinDir;"
    }

    if (([System.String]::IsNullOrWhiteSpace($NewPath)) -or ($NewPath -eq ';'))
    {
        Write-Error "Something is very wrong, this code should never execute. The `"$NewPath`" variable is empty. Exiting."
        throw $NewPath
    }

    #[string] $NewPath = $NewPath + $ExistingPath

    $PathCharLength = $NewPath.Length
    Write-Debug "New PATH variable length is: $PathCharLength"

    $PathVariables = [ordered]@{
        RawPath     = "$NewPath"
        SplitVar    = [System.String]::Empty
        Path        = [ordered] @{
            varname  = "Path"
            varvalue = [System.String]::Empty
        }
        ExtraPath   = [ordered]@{
            varname  = "ExtraPathString"
            varvalue = [System.String]::Empty
            
        }
        IsSplitPath = $false
    }

    if ($PathCharLength -ge 32767)
    {
        Write-Debug "Path variable is over Windows PATH length limit, splitting into two variables"
        $SplitPath = $NewPath.Split(';')
        $NumDirs = $SplitPath.Count
        $HalfwayPoint = $NumDirs / 2
        $IsEven = ($HalfwayPoint % 2) -eq 0

        if ($IsEven)
        {
            $PathVariables.Path.varvalue = ($SplitPath[0..$HalfwayPoint] -join ';') + ';'
            $PathVariables.ExtraPath.varvalue = ($SplitPath[$($HalfwayPoint + 1)..$($NumDirs - 1)] -join ';') + ';'
        }
        else
        {
            $RoundedHalfway = [math]::Round($HalfwayPoint)
            $PathVariables.Path.varvalue = ($SplitPath[0..$RoundedHalfway] -join ';') + ';'
            $PathVariables.ExtraPath.varvalue = ($SplitPath[$($RoundedHalfway + 1)..$($NumDirs - 1)] -join ';') + ';'
        }
        $PathVariables.SplitVar = "$($PathVariables.Path.varvalue)%ExtraPathString%"
        $PathVariables.IsSplitPath = $true
    }

    if ($PSCmdlet.ShouldProcess("$($EnvVarTarget.ToString()) PATH environment variable", "Set: $NewPath"))
    {
        try
        {
            [Environment]::SetEnvironmentVariable("CustomPathAlreadySet", $true, [EnvironmentVariableTarget]::User)
            [Environment]::SetEnvironmentVariable("OriginalEnvTarget", "$($EnvVarTarget.ToString())", [EnvironmentVariableTarget]::User)

            if ($PathVariables.IsSplitPath)
            {
                [Environment]::SetEnvironmentVariable("ExtraPathString", "$($PathVariables.ExtraPath)", $EnvVarTarget)
                [Environment]::SetEnvironmentVariable("Path", "$($PathVariables.SplitVar)", $EnvVarTarget)
            }
            else
            {
                [Environment]::SetEnvironmentVariable("Path", "$NewPath", $EnvVarTarget)
            }

            Write-Host "PATH variable has been modified. The PATH variable is now: "
            Write-Host "$(([Environment]::GetEnvironmentVariables("$EnvVarTarget")).Path)"
        }
        catch
        {
            $EnvSetError = $_
            Write-Error "Could not set environment variable `"Path`" to: $NewPath"
            throw $EnvSetError
        }
    }
}


function Test-ValidFilename
{
    param(
        [Parameter(Mandatory = $true)] [string] $FileName
    )

    $IndexOfInvalidChar = $FileName.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars())

    # IndexOfAny() returns the value -1 to indicate no such character was found
    return $IndexOfInvalidChar -eq -1
}

function Test-IsNullorEmpty
{
    param(
        [Parameter(Mandatory = $false, ValueFromPipeline = $true)]
        [object]
        $TestObject
    )

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
            if ([string]::IsNullOrEmpty($TestObject) -or $TestObject.Length -eq 0)
            {
                return $true
            }
            else
            {
                return $false
            }
            break
        }

        'PSCustomObject'
        {
            if (@($TestObject.psobject.Properties).Count -eq 0)
            {
                return $true
            }
            else
            {
                return $false
            }
            break
        }

        'Hashtable'
        {
            if ($TestObject.Count -eq 0)
            {
                return $true
            }
            else
            {
                return $false
            }
            break
        }

        'DateTime'
        {
            if (!(New-TimeSpan -Start $TestObject -End (Get-Date)))
            {
                return $true
            }
            else
            {
                return $false
            }
        }

        'Object[]'
        {
            if ($TestObject.GetType().BaseType.Name -eq 'Array')
            {
                if ($TestObject.Count -eq 0)
                {
                    return $true
                }
                else
                {
                    return $false
                }
            }
            else
            {
                ## Don't know what this object is.
                Write-Error -Message "Object of type $ObjectType is not supported"
                throw New-Object System.NotSupportedException
            }
            break
        }

        Default
        {
            Write-Error -Message "Object of type $ObjectType is not supported"
            throw New-Object System.NotSupportedException
        }
    
    
    }

    ## We should never get here

    Write-Error -Message "Object of type $ObjectType is not supported"
    throw New-Object System.NotSupportedException

}

function wget
{

    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] [string] $URL,
        [Parameter(Mandatory = $false)] [string] $Filename,
        [Parameter(Mandatory = $false)] [string] $Path
    )

    begin
    {
        $PathInfo = [PSCustomObject]@{
            PathProvided      = $false
            PathNameValid     = $false
            UseRemoteFilename = $false
            PathExists        = $false
            FullPath          = ''
            URL               = $URL
        }

        $FileInfo = [PSCustomObject]@{
            Filename          = $Filename
            RemoteFilename    = ''
            UseRemoteFilename = $false
            RemoteFileInfo    = $null
            FullPath          = [ref] $PathInfo.FullPath
        }

        $PathInfo.PathProvided = $false
        $PathInfo.PathNameValid = $false
        $FileInfo.UseRemoteFilename = $false
        $PathInfo.PathExists = $false

        if (!(Test-IsNullorEmpty "$Path"))
        {
            $PathInfo.PathProvided = $true
            $PathInfo.PathNameValid = Test-Path "$Path" -IsValid
            $PathInfo.PathExists = Test-Path "$Path"
        }
        if (Test-IsNullorEmpty $FileInfo.Filename)
        {
            $FileInfo.UseRemoteFilename = $true
        }
        if (!(Test-IsNullorEmpty $FileInfo.Filename) -and !(Test-ValidFilename $FileInfo.Filename))
        {
        
            $IsValid = $false
            while (!$IsValid)
            {
                Write-Error -Message "The filename $($FileInfo.Filename) contains invalid characters, please enter a valid filename: "

                $NewFilename = Read-Host
                if (Test-ValidFilename $NewFilename)
                {
                    $IsValid = $true
                }
            }
            $FileInfo.Filename = $NewFilename
        }

        if ($PathInfo.PathProvided -and !$PathInfo.PathNameValid)
        {

            $IsValid = $false
            while (!$IsValid)
            {
                Write-Error -Message "The path $Path contains invalid characters, please enter a valid path name: "

                $NewPath = Read-Host
                if (Test-Path $NewPath -IsValid)
                {
                    $IsValid = $true
                }
            }
            $Path = $NewPath

        }

        if (Test-IsNullorEmpty $PathInfo.URL)
        {
            Write-Host "Enter the URL of the file to download: "
            $PathInfo.URL = Read-Host
        }

        $IsValid = $false
        while (!$IsValid)
        {

            ## Good God parsing a URL manually really sucks
            $SplitURL = $PathInfo.URL.Split('://')
            $Protocol = $SplitURL[0]
            $Domain = $SplitURL.Split('/')[1]
            $URLPath = $SplitURL.Split('/')
            $URLPath = $URLPath[2..$($URLPath.Count - 1)] -join '/'

            $EscapedPath = [uri]::EscapeDataString("$($URLPath[0])")
            $EscapedURL = "${Protocol}://$Domain/$EscapedPath"

            $IsValid = [uri]::IsWellFormedUriString($EscapedURL, 'Absolute') -and ([uri] $PathInfo.URL).Scheme -in 'http', 'https'
            if (!$IsValid)
            {
                Write-Error -Message "The entered URL is not a valid URL. Please enter a valid URL: "
                $PathInfo.URL = Read-Host
                continue
            }
        }
    }

    process
    {
        Write-Host -ForegroundColor Green "In process, path is: $Path"
        Write-Host -ForegroundColor Green "In process, Pathinfo is: $($PathInfo)"
    
        try
        {
            $FileInfo.RemoteFileInfo = Invoke-WebRequest -UseBasicParsing -Uri "$($PathInfo.URL)" -Method Head
        }
        catch
        {
            $ErrorResponse = $_
            $ErrorMessage = $ErrorResponse.Exception.Response.StatusCode
            $ErrorCode = $ErrorResponse.Exception.Response.StatusCode.Value__
            Write-Error -Category "$ErrorMessage" -Message "Error attempting to download file. The error was: $ErrorCode $ErrorMessage"
            throw
        }

        $FileInfo.RemoteFilename = $FileInfo.RemoteFileInfo.headers.'Content-Disposition'.Split('filename=')[1]
        if (Test-IsNullorEmpty "$($FileInfo.Filename)")
        {
            $FileInfo.Filename = "$($FileInfo.RemoteFilename)"
        }

        if (Test-IsNullorEmpty $Path)
        {
            Write-Host -ForegroundColor Green "Path not provided."
            $Path = (Get-Location).Path
            $PathInfo.PathExists = $true
        }

        Write-Host -ForegroundColor Green "Path is now: $Path"

        if ((Test-IsNullorEmpty "$Path") -and !$PathInfo.PathExists)
        {
            Write-Host "Provided path does not exist, creating it."
            try
            {
                New-Item -ItemType Directory -Path "$Path" -Verbose -Force -Confirm:$false
                $PathInfo.PathExists = $true
            }
            catch
            {
                $PathError = $_
                Write-Error "Could not create path at $Path"
                throw $PathError
            }
        }
        $PathInfo.FullPath = Join-Path -Path "$Path" -ChildPath "$($FileInfo.Filename)"

        try
        {
            Invoke-WebRequest -UseBasicParsing -Uri $PathInfo.URL -OutFile "$($PathInfo.FullPath)"
        }
        catch
        {
            $DownloadError = $_
            Write-Error "Error downloading file. The error details are: "
            Write-Error $DownloadError.Exception
            throw $DownloadError.Exception
        }
    }
}

function Find-ADuser
{

    param
    (
        [Parameter(Mandatory = $true)]
        [string]
        $SearchString,

        [Parameter(Mandatory = $false)]
        [array]
        $ExtraProperties
    )

    $ADargs = @()
    $ADargs += "mail", "whencreated", "mail", "whenchanged", "department", "title", "manager", "PasswordLastSet", "PasswordNeverExpires", "PasswordExpired"

    foreach ($ExtraProperty in $ExtraProperties)
    {
        $ADargs += $ExtraProperty
    }

    try
    {
        Get-ADUser $SearchString -Properties $ADargs
    }
    catch
    {
        Get-ADUser -LDAPFilter "(anr=$SearchString)" -Properties $ADargs
    }
}

function Send-Email
{
    param (
        [Parameter(Mandatory = $true)][string] $From,
        [Parameter(Mandatory = $true)][string] $Subject,
        [Parameter(Mandatory = $true)] $Body,
        [Parameter(Mandatory = $true)] $Recipients,
        [Parameter(Mandatory = $false)] $ScriptConfig,
        [Parameter(Mandatory = $false)] $SMTPServer,
        [Parameter(Mandatory = $false)] $Attachments,
        [Parameter(Mandatory = $false)] $Logfile,
        [Parameter(Mandatory = $false)] $UtilityObject,
        [Parameter(Mandatory = $false)][switch] $HTML
    )

    foreach ($Recipient in $Recipients)
    {
        $SMTPMessage = New-Object System.Net.Mail.MailMessage($From, $Recipient, $Subject, $Body)
        if ($Logfile)
        {
            $Attachment = New-Object System.Net.Mail.Attachment($Logfile)
            $SMTPMessage.Attachments.Add($Attachment)
        }
        if ($Attachments)
        {
            foreach ($Attachment in $Attachments)
            {
                $Attachment = New-Object System.Net.Mail.Attachment($Attachment)
                $SMTPMessage.Attachments.Add($Attachment)
            }
        }
        if ($HTML)
        {
            $SMTPMessage.IsBodyHtml = $true
        }
        $SMTPClient = New-Object Net.Mail.SmtpClient($SMTPServer, 25)
        $SMTPClient.EnableSsl = $false
        $SMTPClient.Send($SMTPMessage)
        if ($Attachment)
        {
            $Attachment.Dispose()
        }
        $SMTPMessage.Dispose()
    }
}

function Search-Directory
{

    param (
        [Parameter(Mandatory = $false)]
        [string]
        $Directory,

        [Parameter(Mandatory = $false)]
        [string]
        $FileFilter,

        [Parameter(Mandatory = $false)]
        [switch]
        $CurrentDirectoryOnly,
        
        [Parameter(Mandatory = $true)]
        [ref]
        $FilesObject
    )
    
    ## This function expects a reference to a variable for $FilesObject. This object should be created in the calling function and 
    ## passed like ([ref]$VariableName).
    ##
    ## The referenced $FilesObject variable should be a hashtable.

    $Caller = $MyInvocation.MyCommand.Name.ToString()

    if ($FilesObject.Value.GetType().Name -ne 'Hashtable')
    {
        Write-Error "The FilesObject parameter must be a [ref] to a hashtable variable."
        throw $FilesObject
    }
    if ($FilesObject.Value.Count -eq 0)
    {
        $FilesObject.Value.Add("Files", @())
        $FilesObject.Value.Add("FilesCount", 0)
        $FilesObject.Value.Add("Errors", @())
        $FilesObject.Value.Add("ErrorCount", 0)
        $FilesObject.Value.Add("HasErrors", $false)
    }

    if ($Directory -match "\\FileSystem::\\\\")
    {
        $Directory = $Directory.Split("::")[1]
    }
    
    Write-Debug "${Caller}(): Walking directory $Directory`n"
    try
    {
        foreach ($Files in [System.IO.Directory]::GetFiles($Directory, $FileFilter))
        {
            foreach ($File in $Files)
            {
                $FilesObject.Value.Files += [System.IO.FileInfo] $File
                $FilesObject.Value.FilesCount++
                Write-Debug "${Caller}(): Found matching file $File`n"
            }
        }
        if (!$CurrentDirectoryOnly)
        {
            foreach ($Directory in [System.IO.Directory]::GetDirectories($Directory))
            {
                Write-Debug "${Caller}(): Recursively calling self to walk $Directory`n"
                $SearchSplat = @{
                    Directory            = $Directory
                    FileFilter           = $FileFilter
                    CurrentDirectoryOnly = $CurrentDirectoryOnly
                    FilesObject          = $FilesObject
                }
                Search-Directory @SearchSplat
            }
        }
    }
    catch [System.Exception]
    {
        $ErrorObject = New-ErrorObject -Caller $Caller -InputObject $_
        $FilesObject.Value.Errors += $ErrorObject
        $FilesObject.Value.ErrorCount++
        $FilesObject.Value.HasErrors = $true
    }
}

function New-AESManagedObject
{

    param(
        [Parameter(Mandatory = $false)] $Key,
        [Parameter(Mandatory = $false)] $InitializationVector
    )

    $AESManaged = New-Object "System.Security.Cryptography.AesManaged"
    $AESManaged.Mode = [System.Security.Cryptography.CipherMode]::CBC
    $AESManaged.Padding = [System.Security.Cryptography.PaddingMode]::Zeros
    $AESManaged.BlockSize = 128
    $AESManaged.KeySize = 256

    if ($InitializationVector)
    {
        if ($InitializationVector.getType().Name -eq "String")
        {
            $AESManaged.IV = [System.Convert]::FromBase64String($InitializationVector)
        }
        else
        {
            $AESManaged.IV = $InitializationVector
        }
    }
    if ($Key)
    {
        if ($Key.getType().Name -eq "String")
        {
            $AESManaged.Key = [System.Convert]::FromBase64String($Key)
        }
        else
        {
            $AESManaged.Key = $Key
        }
    }
    return $AESManaged
}

function New-AESKey
{
    $AESManaged = New-AESManagedObject
    $AESManaged.GenerateKey()
    return [System.Convert]::ToBase64String($AESManaged.Key)
}

function Get-AESEncryptedString
{
    param (
        [Parameter(Mandatory = $true)] $Key,
        [Parameter(Mandatory = $true)] $UnencryptedString
    )
    $Bytes = [System.Text.Encoding]::UTF8.GetBytes($UnencryptedString)
    $AESManaged = New-AESManagedObject $Key
    $encryptor = $AESManaged.CreateEncryptor()
    $EncryptedData = $encryptor.TransformFinalBlock($Bytes, 0, $Bytes.Length);
    [byte[]] $fullData = $AESManaged.IV + $EncryptedData
    $AESManaged.Dispose()
    [System.Convert]::ToBase64String($fullData)
}

function Get-AESDecryptedString
{

    param (
        [Parameter(Mandatory = $true)] $Key,
        [Parameter(Mandatory = $true)] $EncryptedStringWithIV
    )

    $Bytes = [System.Convert]::FromBase64String($EncryptedStringWithIV)
    $InitializationVector = $Bytes[0..15]
    $AESManaged = New-AESManagedObject $Key $InitializationVector
    $decryptor = $AESManaged.CreateDecryptor();
    $UnencryptedData = $decryptor.TransformFinalBlock($Bytes, 16, $Bytes.Length - 16);
    $AESManaged.Dispose()
    [System.Text.Encoding]::UTF8.GetString($UnencryptedData).Trim([char]0)
}

function ConvertFrom-TOML
{
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [AllowNull()]
        [Parameter(Mandatory = $true)] [Tomlyn.Model.TomlTable] $Model
    )

    # Must load the Tomlyn assembly before calling this function

    if ($null -eq $Model)
    {
        Write-Debug -Message 'Model is null, returning empty hashtable'
        return [ordered]@{}
    }

    $result = [ordered]@{}

    foreach ($Key in $Model.Keys)
    {
        $Value = $Model[$Key]

        switch ($Value.ToString())
        {
            'Tomlyn.Model.TomlTable'
            {
                Write-Debug -Message "Calling self recursively for key: $Key"
                $result[$Key] = ConvertFrom-TOML -Model $Value
            }

            'Tomlyn.Model.TomlTableArray'
            {
                foreach ($TomlArray in $Value)
                {
                    if ($TomlArray -is [Tomlyn.Model.TomlTable])
                    {
                        $result[$Key] += @(ConvertFrom-TOML -Model $TomlArray)
                    }
                    else
                    {
                        $result[$Key] = @($TomlArray)
                    }
                }
            }

            default
            {
                $result[$Key] = $Value
            }
        }
    }

    return $result
}

function Get-FlattenedArray
{
    [CmdletBinding()]
    [OutputType([array])]
    param (
        [Parameter(Mandatory = $false, ValueFromPipeline = $true)]
        [AllowNull()]
        [array]
        $InputArray
    )

    [array] $ReturnArray = (
        @(
            $InputArray | ForEach-Object { $_ })) | Where-Object {
                ($null -ne $_) -and
        !([System.String]::IsNullOrWhiteSpace($_))
    }
    return $ReturnArray
}

function Merge-JSON
{

    param (
        [Parameter(Mandatory = $true)] $OriginalJSON,
        [Parameter(Mandatory = $true)] $OverridesJSON
    )
    $OverridesJSON.psobject.Properties | ForEach-Object {
        if ($_.TypeNameOfValue -eq 'System.Management.Automation.PSCustomObject' -and $OriginalJSON."$($_.Name)" )
        {
            Merge-JSON $OriginalJSON."$($_.Name)" $_.Value
        }
        else
        {
            $OriginalJSON | Add-Member -MemberType $_.MemberType -Name $_.Name -Value $_.Value -Force
        }
    }
}

function Get-RandomPassword
{
    ( -join (1..24 | ForEach-Object { [char[]](0..127) -match '[\w\\\*\#\+\?\|\{\[\]\}\^\.\$]' | Get-Random }))
}

function Add-CodeSignature
{
    param (
        [string]$Filename
    )
    $NewestCert = $null
    $SigningCerts = @(Get-ChildItem cert:\CurrentUser\My -CodeSigning) | Where-Object { $_.NotAfter -gt $(Get-Date) }
    foreach ($Cert in $SigningCerts)
    {
        if ($null -eq $NewestCert)
        {
            $NewestCert = $Cert
        }
        else
        {
            if ($Cert.Notafter -gt $NewestCert.NotAfter)
            {
                $NewestCert = $Cert
            }
        }
    }

    if (Test-IsNullorEmpty $NewestCert)
    {
        Write-Error "No valid code-signing certificates found in the current user's ($env:USERNAME) certificate store."
        Write-Error "Certs in the current user's ($Env:USERNAME) certificate store are:"
        Write-Error "$((Get-ChildItem cert:\CurrentUser\My).ToString())"
        return
    }

    Write-Host "Signing $Filename with cert:"
    Write-Host "Thumprint: $($NewestCert.Thumbprint)"
    Write-Host "Subject: $($NewestCert.Subject)"
    Write-Host "Expiration: $($NewestCert.NotAfter)"
    Set-AuthenticodeSignature $Filename $NewestCert
}

function Get-CPUStats
{
    $TotalCPUTime = (Get-CimInstance Win32_ComputerSystem).NumberOfLogicalProcessors * 100
    $Result = (Get-Counter -Counter "\Process(*)\% Processor Time" -SampleInterval 5 -MaxSamples 12 -ErrorAction SilentlyContinue).CounterSamples.where({ $_.InstanceName -notmatch "^(_total)$" }) |
    Group-Object { $_.Instancename } | Select-Object @{Name = "Computername"; Expression = { $_.group.path.split("\\")[2] } },
    @{Name = "Process"; Expression = { $_.Name } },
    @{Name = "AvgCPUTime"; Expression = { ($_.group.cookedvalue | Measure-Object -Average).average } },
    @{Name = 'AvgCPUTime%'; E = { ($_.group.cookedvalue | Measure-Object -Average).average / $TotalCPUTime * 100 } } |
    Sort-Object AvgCPUTime -Descending
    return $Result
}

function Get-CodeSignature
{
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $Filename
    )
    
    Get-AuthenticodeSignature $Filename
}

function Get-AllCodeSignatures
{

    param(
        [Parameter(Mandatory = $false)] [string] $Path = (Get-Location).Path,
        [Parameter(Mandatory = $false)] [string] $PathFilter = "*",
        [Parameter(Mandatory = $false)] [switch] $Recurse
    )

    if ($Recurse)
    {
        Get-ChildItem -Path "$Path" -Filter $PathFilter -Recurse | ForEach-Object { Get-AuthenticodeSignature $_ }
    }
    else
    {
        Get-ChildItem -Path "$Path" -Filter $PathFilter | ForEach-Object { Get-AuthenticodeSignature $_ }
    }

}

function Get-AsciiShrug
{
    return '¯\_(ツ)_/¯'
}

function Copy-Profile ()
{
    Copy-Item "$env:USERPROFILE\Documents\PowerShell\Microsoft.PowerShell_profile.ps1" "$env:USERPROFILE\Documents\WindowsPowerShell\Profile.ps1" -Force -Confirm:$false
    Copy-Item "$env:USERPROFILE\Documents\PowerShell\Microsoft.VSCode_profile.ps1" "$env:USERPROFILE\Documents\WindowsPowerShell\Microsoft.VSCode_profile.ps1" -Force -Confirm:$false
}

function Get-UnShittifiedURL
{
    [Alias("Get-RealURL")]
    [CmdletBinding()]
    [OutputType([array])]
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [array]
        $URL,

        [Parameter(Mandatory = $false)]
        [switch]
        $RemoveTrackingBullshit,
        
        [Parameter(Mandatory = $false)]
        [switch]
        $JustRemoveTrackingBullshit,

        [Parameter(Mandatory = $false)]
        [switch]
        $Copy
    )
    $RealURLs = @()

    if ($JustRemoveTrackingBullshit)
    {
        $RealURL = [ordered]@{}
        $i = 1
        foreach ($ProvidedURL in $URL)
        {
            try
            {
                if ($ProvidedURL -match '.*://.*\.{0,63}/.*\?')
                {
                    $RealURL.Add("URL$i", ($ProvidedURL -replace '\?.*'))
                    $RealURLs += $RealURL
                }
            }
            catch
            {
                $URLError = $_
                Write-Information -InformationAction Continue -MessageData "Error parsing URL. The error was: $($URLError | Select-Object * | Out-String)"
            }
            $i++
        }
        return $RealURLs
    }
    try
    {
        foreach ($ProvidedURL in $URL)
        {
            Invoke-WebRequest -UseBasicParsing -Uri "$ProvidedURL" -Method Head -MaximumRedirection 0 -ErrorAction Stop | Out-Null
            Write-Information -InformationAction Continue -MessageData "The URL $ProvidedURL is not shittified."
            $RealURLs += $ProvidedURL
        }
    }
    catch
    {
        $Siteredirected = $_
        $RealURL = [ordered]@{
            "HTTP Error"   = $Siteredirected.Exception.Response.StatusCode.value__
            "HTTP Message" = $Siteredirected.Exception.Response.StatusCode
            "Real URL"     = $Siteredirected.Exception.Response.Headers.Location.ToString()
        }
        Write-Host "`nUnShittified details for ${ProvidedURL}:`n"

        if ($RemoveTrackingBullshit)
        {
            if ($RealURL["Real URL"] -match '.*://.*\.{0,63}/.*\?')
            {
                $RealURL.Add("Bullshit Removed", ($RealURL["Real URL"] -replace '\?.*'))
            }
        }

        if ($Copy)
        {
            $RealURL."Real URL" | Set-Clipboard
            Write-Information -InformationAction Continue -MessageData "`nReal URL copied to clipboard."
        }
        foreach ($Key in $RealURL.Keys)
        {
            if ($Key -eq "Bullshit Removed")
            {
                Write-Information -InformationAction Continue -MessageData "Bullshit Removed:`t$($RealURL[$Key])"
                continue
            }
            Write-Information -InformationAction Continue -MessageData "${Key}:`t`t$($RealURL[$Key])"
        }

        Write-Information -InformationAction Continue -MessageData ""
        $RealURLs += $RealURL["Real URL"]
    }
    return $RealURLs
}

function New-CenteredString
{
    param (
        [Parameter(Mandatory = $true)] [array] $String,
        [Parameter(Mandatory = $false)] [int] $Width = $Host.UI.RawUI.BufferSize.Width
    )
    $ReturnObject = $null
    foreach ($Line in $String.Split("`n"))
    {
        $Line = $Line.PadLeft(([Math]::Max(0, $Width / 2) + [Math]::Floor($Line.Length / 2)))
        $Line = $Line.PadRight($Width)
        $ReturnObject += $Line
    }

    return $ReturnObject
}

function Write-MultiStreamMessage
{
    param (
        [Parameter(Mandatory = $false)] [string] $Caller,
        [Parameter(Mandatory = $false)] [string] $Stream = "stdout",
        [Parameter(Mandatory = $false)] [switch] $TimeStamp,
        [Parameter(Mandatory = $true)] [array] $Messages
    )

    $Streams = @{
        output  = "Microsoft.PowerShell.Utility\Write-Output"
        stdout  = "Microsoft.PowerShell.Utility\Write-Host"
        stderr  = "Microsoft.PowerShell.Utility\Write-Error"
        verbose = "Microsoft.PowerShell.Utility\Write-Verbose"
        warning = "Microsoft.PowerShell.Utility\Write-Warning"
        info    = "Microsoft.PowerShell.Utility\Write-Information"
        debug   = "Microsoft.PowerShell.Utility\Write-Debug"
    }

    foreach ($Message in $Messages)
    {
        if ($TimeStamp)
        {
            $FormattedTimestamp = "$((Get-Date -Format 'o'))|"
            if ($Caller.Length -gt 0)
            {
                $Caller += "()"
                $Message = "$FormattedTimestamp ${Caller}: $Message"
            }
            else
            {
                $Message = "$FormattedTimestamp $Message"
            }
        }
        else
        {
            if ($Caller.Length -gt 0)
            {
                $Caller += "()"
                $Message = "${Caller}: $Message"
            }
        }
        Invoke-Expression -Command "$($Streams.$Stream) '$Message'"
    }
}

function Write-ProgressBar
{
    param(
        [Parameter(Mandatory = $true)] [hashtable] $SettingsObject
    )

    ## SettingsObject hashtable should contain the following properties:
    ##     [string] LoadingMessage - The message to display while the progress bar is running
    ##     [Diagnostics.Stopwatch] Timer - The stopwatch object to use for timing
    ##     [int] Interval - The number of seconds to run the progress bar for

    if (Test-IsNullorEmpty $SettingsObject.LoadingMessage)
    {
        $Message = "Loading..."
    }

    if (Test-IsNullorEmpty $SettingsObject.Interval)
    {
        $SettingsObject.Interval = 60
    }

    if (Test-IsNullorEmpty $SettingsObject.Timer)
    {
        Write-Error -Message "No timer object provided. Please provide a .NET [Diagnostics.Stopwatch] timer object to use for timing."
        throw $SettingsObject
        return
    }

    if (!($SettingsObject.Timer.IsRunning))
    {
        $SettingsObject.Timer.Start()
    }

    $Message = $SettingsObject.LoadingMessage
    $ConsoleWidth = ($Host.UI.RawUI.BufferSize.Width - $Message.Length - 9)

    while ($SettingsObject.Timer.Elapsed.Seconds -lt $SettingsObject.Interval)
    {
        $Progress = [math]::Round(($Settings.Timer.Elapsed.Seconds / $SettingsObject.Interval) * 100)
        $ProgressBar = ([math]::Round(($Progress / 100) * $ConsoleWidth))
        $ProgressBarString = "#" * $ProgressBar
        $ProgressString = "{0} {1}% [{2}{3}]" -f $Message, $Progress, $ProgressBarString, (' ' * ($ConsoleWidth - $ProgressBar))

        Write-Host -ForegroundColor Yellow $ProgressString -NoNewline
        Write-Host -ForegroundColor Yellow "`r" -NoNewline
    }

    $SettingsObject.Timer.Stop()
    $ProgressString = "{0} {1}% [{2}]" -f $Message, "100", ("#" * ($ConsoleWidth - 1))
    Write-Host -ForegroundColor Yellow $ProgressString -NoNewline
    Write-Host
    Clear-Host
}


function prompt
{
    #"PS $env:USERNAME@$env:COMPUTERNAME`|$($executionContext.SessionState.Path.CurrentLocation)$(if ($PromptIsAdmin) { '#' * ($nestedPromptLevel + 1) } else { '$' * ($nestedPromptLevel + 1) }) "
    #Write-Host -NoNewline "PS "

    $PromptIsAdmin = [bool](([System.Security.Principal.WindowsIdentity]::GetCurrent()).groups -match "S-1-5-32-544")
    if ($PromptIsAdmin)
    {
        Write-Host -NoNewline -ForegroundColor Red "$env:USERNAME"
    }
    else
    {
        Write-Host -NoNewline -ForegroundColor Green "$env:USERNAME"
    }
    Write-Host -NoNewline -ForegroundColor Cyan "`@"
    Write-Host -NoNewline -ForegroundColor Red "$env:COMPUTERNAME"
    Write-Host -NoNewline -ForegroundColor Yellow "|"

    if (($executionContext.SessionState.Path.CurrentLocation).Path -like "*Microsoft.PowerShell.Core\FileSystem::\\*")
    {
        Write-Host -NoNewline $(($executionContext.SessionState.Path.CurrentLocation.Path).Split("::")[2])
    }

    else
    {
        Write-Host -NoNewline "$($executionContext.SessionState.Path.CurrentLocation)"
    }
    
    if ($PromptIsAdmin)
    {
        Write-Host -NoNewline -ForegroundColor Red $('#' * ($nestedPromptLevel + 1))
    }
    else
    {
        Write-Host -NoNewline -ForegroundColor Green $('$' * ($nestedPromptLevel + 1))
    }

    Remove-Variable PromptIsAdmin

    # This return statement is important. Without it, PowerShell will append "PS>" to the end of the custom prompt.
    return " "
}

function Lock-KeyfileDrive ($Drive = "S:")
{
    manage-bde -lock $Drive -forcedismount
    return $null
}

function New-Symlink
{
    param(
        [string]
        $Target,
        
        [string] $Link
    )

    try
    {
        New-Item -ItemType SymbolicLink -Target $target -Name $link -ErrorAction Stop
    }
    catch
    {
        $LinkError = $_
        Write-Error "Error creating symlink. The error was: $LinkError"
        Write-Error "Stack trace: $($LinkError.Exception.StackTrace)"
    }
    
}

function ltc ()
{
    Set-PSReadLineKeyHandler -Key Tab -Function Complete
}

function dtc ()
{
    Set-PSReadLineKeyHandler -Key Tab -Function TabCompleteNext
}

function Get-MaskedReadHost
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string]
        $Prompt
    )

    if ($PSBoundParameters.TryGetValue('Prompt', [ref]$Prompt))
    {
        $TempString = Read-Host -AsSecureString -Prompt $PSBoundParameters.Item('Prompt')
        $String = (New-Object PSCredential 0, $TempString).GetNetworkCredential().Password
        return $String
    }
    else
    {
        $TempString = Read-Host -AsSecureString
        $String = (New-Object PSCredential 0, $TempString).GetNetworkCredential().Password
        return $String
    }
}

function Get-StringHash
{
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
    param
    (
        [Parameter(Mandatory = $false, ValueFromPipeline = $true)]
        [string]
        $String,

        [Parameter(Mandatory = $false)]
        [string]
        $HashName = "SHA256",

        [Parameter(Mandatory = $false)]
        [switch]
        $Help,

        [Parameter(Mandatory = $false)]
        [switch]
        $NullStringHash,

        [Parameter(Mandatory = $false)]
        [switch]
        $Prompt
    )

    if ($Help)
    {
        Get-Help Get-StringHash -Full
        return
    }

    if ($NullStringHash)
    {
        Write-Host "Providing $HashName hash of null string"
    }

    if (!($NullStringHash) -and (Test-IsNullorEmpty $String) -and (!$Prompt))
    {
        Get-Help Get-StringHash -Full
        return
    }

    if ($Prompt)
    {
        $TempString = Read-Host -AsSecureString "Enter the string to hash"
        $String = (New-Object PSCredential 0, $TempString).GetNetworkCredential().Password
        Clear-Variable TempString
        Remove-Variable TempString
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

function Get-CodeSigningCertStatus
{
    param
    (
        [switch] $Manual
    )
    $CodeSigningCerts = (Get-ChildItem cert:\CurrentUser\My -CodeSigning)
    $Banner = @"

        ************************************************************
        **               CODE-SIGNING CERT CHECKER                **
        ************************************************************

"@
    foreach ($Cert in $CodeSigningCerts)
    {
        $ExpirationDate = $cert.NotAfter
        $DaysToExpire = ($ExpirationDate - $(Get-Date)).Days
        $BannerDisplayed = $false
        
        if ($DaysToExpire -lt 0)
        {
            if (!$BannerDisplayed)
            {
                Write-Host $Banner
            }
            Write-Host "The following code-signing cert is " -NoNewline
            Write-Host -ForegroundColor Red "expired" -NoNewline
            Write-Host " and should be removed or renewed:"
            $Cert | Format-List
            $BannerDisplayed = $true
            continue
        }
        elseif ($DaysToExpire -le 14)
        {
            if (!$BannerDisplayed)
            {
                Write-Host $Banner
            }
            Write-Host "The following code-signing cert is " -NoNewline
            Write-Host -ForegroundColor Red "expiring in $DaysToExpire day(s)" -NoNewline
            Write-Host " and should be renewed:"
            $Cert | Format-List
            $BannerDisplayed = $true
        }
        elseif ($Manual)
        {
            if (!$BannerDisplayed)
            {
                Write-Host $Banner
            }
            Write-Host "The following code-signing cert " -NoNewline
            Write-Host -ForegroundColor Red "expires in $DaysToExpire day(s):" -NoNewline
            $Cert | Format-List
            $BannerDisplayed = $true
        }
    }
    return
}

function Start-WindowsTerminal
{

    <#
    .SYNOPSIS
    Start Windows Terminal from PowerShell.

    .DESCRIPTION
    Start Windows Terminal from PowerShell. This can be done as the current user, another user, and optionally
    elevated through UAC.

    .PARAMETER Credential
    Custom user credential to use when starting windows terminal. Unless UAC is disabled or not applied to the target
    account, the process will be a limited process. Use with -Elevated to elevate the process.

    .PARAMETER Elevated
    Whether to run as an elevated user or not. This will also result in a interactive UAC prompt. Can be combined with
    -Credential to start Windows Terminal elevated as another user when the current user is also an admin.

    .NOTES
    To start WindowsTerminal as another user, the WindowsTerminal app must be installed on that user's profile. If it
    is not then this cmdlet will fail with wt.exe not found.
    #>
    [CmdletBinding()]
    param (
        [PSCredential]
        $Credential,

        [Switch]
        $Elevated
    )

    $splitUser = {
        param ([PSCredential]$Credential)

        # ProcessStartInfo requires a NETLOGON username form 'DOMAIN\user' to be split up into different values. This
        # extracts the domain part if that user is in that form otherwise keeps it as is.
        $username = $Credential.UserName
        $domain = [NullString]::Value
        if ($username.Contains('\'))
        {
            $domain, $username = $username.Split('\\', 2)
        }

        $domain, $username
    }

    if ($Credential -and $Elevated)
    {
        # This is a tricky process, we cannot elevate and run as another user at the same time so we need to do it
        # step by step. These steps are:
        #
        #   1. Start a process as another user
        #   2. Create an elevated process for that user using UAC
        #   3. Start wt.exe from that elevated process
        #
        # We can also only start wt.exe from that elevated process, we cannot start it as elevated from the get go.
        # See 'elseif ($Elevated)' for more details.
        $elevation = {
            Start-Process -FilePath powershell.exe -ArgumentList 'wt.exe' -WindowStyle Hidden -Verb Runas
        }
        $elevationCommand = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($elevation.ToString()))

        # Start powershell as another user which will elevate itself through UAC then call wt.exe.
        $domain, $username = &$splitUser -Credential $Credential
        $psi = [System.Diagnostics.ProcessStartInfo]@{
            FileName        = 'powershell.exe'
            Arguments       = "-WindowStyle Hidden -EncodedCommand $elevationCommand"
            LoadUserProfile = $true
            UserName        = $username
            Domain          = $domain
            Password        = $Credential.Password
            UseShellExecute = $false
            WindowStyle     = 'Hidden'
        }
        $null = [System.Diagnostics.Process]::Start($psi)
    }
    elseif ($Credential)
    {
        # Using 'Start-Process -Credential $Credential' will spawn the process as the other user but the window
        # doesn't allow any interaction. .NET has no limitations here so we use that instead.

        $domain, $username = &$splitUser -Credential $Credential
        $psi = [System.Diagnostics.ProcessStartInfo]@{
            FileName        = 'wt.exe'
            LoadUserProfile = $true
            UserName        = $username
            Domain          = $domain
            Password        = $Credential.Password
            UseShellExecute = $false
            WindowStyle     = 'Hidden'
        }
        $null = [System.Diagnostics.Process]::Start($psi)
    }
    elseif ($Elevated)
    {
        # Want to start it elevated through UAC. Unfortunately we cannot just call wt.exe through UAC as that won't
        # place the proper groups to the new token that are needed to access wt.exe. By starting a new powershell
        # that is elevated then wt.exe, it will inherit the elevated access token and have the proper token
        # capabilities applied.
        Start-Process -FilePath powershell.exe -ArgumentList 'wt.exe' -WindowStyle Hidden -Verb Runas
    }
    else
    {
        # Just want to start a new instance, call normally.
        Start-Process -FilePath wt.exe
    }
}

function Get-RandomPassword
{
    ( -join (1..24 | ForEach-Object { [char[]](0..127) -match '[\w\\\*\#\+\?\|\{\[\]\}\^\.\$]' | Get-Random }))
}
function Get-DirectoryTreeSize
{
    <#
    .SYNOPSIS
        This is used to get the file count, subdirectory count and folder size for the path specified. The output will show the current folder stats unless you specify the "AllItemsAndAllFolders" property.
        Since this uses Get-ChildItem as the underlying structure, this supports local paths, network UNC paths and mapped drives.
     
    .NOTES
        Name: Get-DirectoryTreeSize
        Author: theSysadminChannel
        Version: 1.0
        DateCreated: 2020-Feb-11
     
     
    .LINK
        https://thesysadminchannel.com/get-directory-tree-size-using-powershell -
     
     
    .PARAMETER Recurse
        Using this parameter will drill down to the end of the folder structure and output the filecount, foldercount and size of each folder respectively.
     
    .PARAMETER AllItemsAndAllFolders
        Using this parameter will get the total file count, total directory count and total folder size in MB for everything under that directory recursively.
     
    .EXAMPLE
        Get-DirectoryTreeSize "C:\Some\Folder"
     
        Path            FileCount DirectoryCount FolderSizeInMB
        ----            --------- -------------- --------------
        C:\Some\folder          3              3          0.002
     
    .EXAMPLE
        Get-DirectoryTreeSize "\\MyServer\Folder" -Recurse
     
        Path                 FileCount DirectoryCount FolderSizeInMB
        ----                 --------- -------------- --------------
        \\MyServer\Folder            2              1         40.082
        .\Subfolder                  1              0         26.555
     
    .EXAMPLE
        Get-DirectoryTreeSize "Z:\MyMapped\folder" -AllItemsAndAllFolders
     
        Path                  TotalFileCount TotalDirectoryCount TotalFolderSizeInMB
        ----                  -------------- ------------------- -------------------
        Z:\MyMapped\folder                 3                   1              68.492
     
    #>
     
    [CmdletBinding(DefaultParameterSetName = "Default")]
     
    param(
        [Parameter(
            Position = 0,
            Mandatory = $true
        )]
        [string]  $Path,
     
     
     
        [Parameter(
            Mandatory = $false,
            ParameterSetName = "ShowRecursive"
        )]
        [switch]  $Recurse,
     
     
     
        [Parameter(
            Mandatory = $false,
            ParameterSetName = "ShowTopFolderAllItemsAndAllFolders"
        )]
        [switch]  $AllItemsAndAllFolders
    )
     
    begin
    {
        #Adding a trailing slash at the end of $path to make it consistent.
        if (-not $Path.EndsWith('\'))
        {
            $Path = "$Path\"
        }
    }
     
    process
    {
        try
        {
            if (-not $PSBoundParameters.ContainsKey("AllItemsAndAllFolders") -and -not $PSBoundParameters.ContainsKey("Recurse"))
            {
                $FileStats = Get-ChildItem -Path $Path -File -ErrorAction Stop | Measure-Object -Property Length -Sum
                $FileCount = $FileStats.Count
                $DirectoryCount = Get-ChildItem -Path $Path -Directory | Measure-Object | Select-Object -ExpandProperty Count
                $SizeMB = "{0:F3}" -f ($FileStats.Sum / 1MB) -as [decimal]
     
                [PSCustomObject]@{
                    Path           = $Path#.Replace($Path,".\")
                    FileCount      = $FileCount
                    DirectoryCount = $DirectoryCount
                    FolderSizeInMB = $SizeMB
                }
            }
     
            if ($PSBoundParameters.ContainsKey("AllItemsAndAllFolders"))
            {
                $FileStats = Get-ChildItem -Path $Path -File -Recurse -ErrorAction Stop | Measure-Object -Property Length -Sum
                $FileCount = $FileStats.Count
                $DirectoryCount = Get-ChildItem -Path $Path -Directory -Recurse | Measure-Object | Select-Object -ExpandProperty Count
                $SizeMB = "{0:F3}" -f ($FileStats.Sum / 1MB) -as [decimal]
     
                [PSCustomObject]@{
                    Path                = $Path#.Replace($Path,".\")
                    TotalFileCount      = $FileCount
                    TotalDirectoryCount = $DirectoryCount
                    TotalFolderSizeInMB = $SizeMB
                }
            }
     
            if ($PSBoundParameters.ContainsKey("Recurse"))
            {
                Get-DirectoryTreeSize -Path $Path
                $FolderList = Get-ChildItem -Path $Path -Directory -Recurse | Select-Object -ExpandProperty FullName
     
                if ($FolderList)
                {
                    foreach ($Folder in $FolderList)
                    {
                        $FileStats = Get-ChildItem -Path $Folder -File | Measure-Object -Property Length -Sum
                        $FileCount = $FileStats.Count
                        $DirectoryCount = Get-ChildItem -Path $Folder -Directory | Measure-Object | Select-Object -ExpandProperty Count
                        $SizeMB = "{0:F3}" -f ($FileStats.Sum / 1MB) -as [decimal]
     
                        [PSCustomObject]@{
                            Path           = $Folder.Replace($Path, ".\")
                            FileCount      = $FileCount
                            DirectoryCount = $DirectoryCount
                            FolderSizeInMB = $SizeMB
                        }
                        #clearing variables
                        $null = $FileStats
                        $null = $FileCount
                        $null = $DirectoryCount
                        $null = $SizeMB
                    }
                }
            }
        }
        catch
        {
            Write-Error $_.Exception.Message
        }
     
    }
     
    end
    {

    }
}

Set-Alias -Name "which" -Value "where.exe"
ltc
Get-CodeSigningCertStatus

$env:LC_ALL = 'C.UTF-8'

$env:LESSCHARSET = 'UTF-8'

#region Function: Get-UnixStylePath
function Get-UnixStylePath
{
    [Alias("Get-WindowsStylePath", "Get-LinuxStylePath")]
    [CmdletBinding(DefaultParameterSetName = 'Unix')]
    [OutputType([PSCustomObject])]
    param (
        [Parameter(Position = 0, Mandatory = $true, ParameterSetName = 'Unix', ValueFromPipeline = $true)]
        [Parameter(Position = 0, Mandatory = $true, ParameterSetName = 'Windows', ValueFromPipeline = $true)]
        [Alias("Dir", "Directory", "Folder")]
        $Path,

        [Parameter(Mandatory = $false, ParameterSetName = 'Unix')]
        [Alias("Linux")]
        [switch] $Unix,

        [Parameter(Mandatory = $false, ParameterSetName = 'Unix', ValueFromPipelineByPropertyName = $true, Position = 1)]
        [Alias("Root")]
        [string] $RootPath,

        [Parameter(Mandatory = $false, ParameterSetName = 'Windows')]
        [Alias("Win")]
        [switch] $Windows,

        [Parameter(Mandatory = $false, ParameterSetName = 'Windows', ValueFromPipelineByPropertyName = $true, Position = 1)]
        [Alias("Drive", "DriveLetter", "DriveName")]
        [string] $WindowsVolume,

        [Parameter(Mandatory = $false)]
        [Alias("Copy", "Clipboard", "Clip")]
        [switch] $CopyToClipboard
    )

    begin
    {
        if (Test-IsNullorEmpty -TestObject $Path)
        {
            throw "Path parameter is null or empty. Please provide the -Path parameter a string, a DirectoryInfo object, " `
                + "an array of strings, an array of DirectoryInfo objects, or a mixed array of strings and DirectoryInfo objects."
        }

        [hashtable] $TruthTable = [ordered]@{
            HostIsWindows          = (($null -eq $IsWindows) -or ($IsWindows -eq $false)) ? $false : $true
            HostIsLinux            = (($null -eq $IsLinux) -or ($IsLinux -eq $false)) ? $false : $true
            ValidPathSeparator     = [System.IO.Path]::DirectorySeparatorChar
            AltPathSeparator       = [System.IO.Path]::AltDirectorySeparatorChar
            WindowsVolumeSeparator = ':'
            UnixVolumeSeparator    = '/'
            InputIsArray           = ($Path -is [array]) ? $true : $false
            InputIsString          = ($Path -is [string]) ? $true : $false
            InputIsDirectoryInfo   = ($Path -is [System.IO.DirectoryInfo]) ? $true : $false
            IsCopyOperation        = $CopyToClipboard
            IsConversionToUnix     = $PSCmdlet.ParameterSetName -eq 'Unix'
            IsConversionToWindows  = $PSCmdlet.ParameterSetName -eq 'Windows'
            HasUnixRootPath        = (Test-IsNullorEmpty $RootPath) ? $false : $true
            HasWindowsVolume       = (Test-IsNullorEmpty $WindowsVolume) ? $false : $true
        }

        [PSCustomObject] $ReturnObject = [PSCustomObject]@{
            ConvertedPaths = [array] @()
            NumConverted   = 0
            NumSkipped     = 0
            NumErrors      = 0
            TruthTable     = $TruthTable
        }

        if ($TruthTable.IsConversionToUnix)
        {
            $ReturnObject | Add-Member -MemberType NoteProperty -Name "UnixPaths" -Value ([ordered]@{})
            $ReturnObject.UnixPaths.Add("Style_WSL", [array] @())
            $ReturnObject.UnixPaths.Add("Style_MSYS2", [array] @())
            $ReturnObject.UnixPaths.Add("Style_Git", [array] @())
        }

        function Parse-ArrayObject
        {
            param (
                [Parameter(Mandatory = $true)] [array] $InputObject,
                [Parameter(Mandatory = $true)] [hashtable] $TruthTable,
                [Parameter(Mandatory = $true)] [PSVariable] $CircuitBreaker,
                [Parameter(Mandatory = $false)] [int] $LoopCounter
            )
            [array] $FoundPaths = @()

            if ($CircuitBreaker.Value.IsRunning)
            {
                Write-Debug "Function Parse-ArrayObject has been recursively called $LoopCounter times " `
                    + "and has been running for $($CircuitBreaker.Value.Elapsed.TotalSeconds) seconds."
            }

            else
            {
                $OriginalErrorActionPreference = $ErrorActionPreference
                $ErrorActionPreference = 'SilentlyContinue'
                $CircuitBreaker.Value.Stop() | Out-Null
                $CircuitBreaker.Value.Reset() | Out-Null
                $ErrorActionPreference = $OriginalErrorActionPreference
                $LoopCounter = 0
            }
    
            foreach ($PathItem in $InputObject)
            {
                switch ($PathItem)
                {
                    { $PathItem -is [array] }
                    {
                        $LoopCounter++

                        if (($CircuitBreaker.Value.IsRunning) -and ($CircuitBreaker.Value.Elapsed.TotalSeconds -ge 15))
                        {
                            Write-Error "The function Parse-ArrayObject has been recursively called $LoopCounter times and " `
                                + "has been running for longer than 15 seconds. This is strongly indicative of an infinite loop due " `
                                + "to a circular reference in the input object. The array at index $($InputObject.IndexOf($PathItem)) " `
                                + "of the original input object is being skipped."

                            $CircuitBreaker.Value.Stop()
                            $CircuitBreaker.Value.Reset()
                            $LoopCounter = 0
                            $ReturnObject.NumSkipped++

                            # Breaking out of the foreach loop here, not the switch statement.
                            continue
                        }

                        Write-Debug "Recursively calling Parse-ArrayObject on a nested array."
                                
                        if ($CircuitBreaker.Value.IsRunning -eq $false)
                        {
                            $CircuitBreaker.Value.Start()
                        }                                
                        $FoundPaths += Parse-ArrayObject -InputObject $PathItem -TruthTable $TruthTable -LoopCounter $LoopCounter -CircuitBreaker $CircuitBreaker
                    }

                    { $PathItem -is [string] }
                    {
                        $FoundPaths += $PathItem.Trim()
                        break
                    }
    
                    { $PathItem -is [System.IO.DirectoryInfo] }
                    {
                        if (($null -eq $PathItem.FullName) -or !(Test-Path -Path $PathItem))
                        {
                            Write-Error "Was passed a DirectoryInfo object that does not exist, skipping."
                            $ReturnObject.NumSkipped++
                            break
                        }
                        $FoundPaths += $PathItem.FullName
                        break
                    }
    
                    default
                    {
                        Write-Error "Object of type $($Path.GetType().Name) is not supported. Please provide a string, a DirectoryInfo object, " `
                            + "an array of strings, an array of DirectoryInfo objects, or a mixed array of strings and DirectoryInfo objects." `
                            + "Skipping item of type $($PathItem.GetType().Name)."
                    }
                }
                # else
                # {
                #     $ReturnArray += $FinalPath.Replace($ValidPathSeparator, $AltPathSeparator)
                # }

            }

            return $FoundPaths
        }
    }

    process
    {
        $PathsToConvert = [array] @()
        $CircuitBreaker = [System.Diagnostics.Stopwatch]::new()

        switch ($Path)
        {
            { $Path -is [array] }
            {
                $PathsToConvert = Parse-ArrayObject -InputObject $Path -TruthTable $TruthTable -CircuitBreaker (Get-Variable -Name CircuitBreaker)
                break
            }

            { $Path -is [string] }
            {
                $PathsToConvert += $Path.Trim()
                break
            }

            { $Path -is [System.IO.DirectoryInfo] }
            {
                if ([System.String]::IsNullOrWhiteSpace($Path.FullName))
                {
                    Write-Error "Was passed a DirectoryInfo object that does not exist, skipping."
                    $ReturnObject.NumSkipped++
                    break
                }
                $PathsToConvert += $Path.FullName
                break
            }
        }

        foreach ($PathToConvert in $PathsToConvert)
        {

            if ($TruthTable.IsConversionToUnix)
            {
                [string] $VolumeRoot = [System.String]::Empty

                if ($TruthTable.HasUnixRootPath)
                {
                    $VolumeRoot = $RootPath
                }
                else
                {
                    $VolumeRoot = '/'
                }

                if ($VolumeRoot.LastIndexOf('/') -ne $VolumeRoot.Length)
                {
                    if (($VolumeRoot.Length -eq 1) -and $VolumeRoot.LastIndexOf('/') -eq 0)
                    {
                        ;
                    }
                    else
                    {
                        $VolumeRoot += "/"
                    }
                }

                try
                {
                    [string] $DrivePortion = [System.String]::Empty
                    [string] $SplitPath = [System.String]::Empty
                    [bool] $IsUNCPath = $PathToConvert -match '^\\\\(?<Server>[\w-]+)(?<ServerPath>.+(?!\\))' -or
                    $PathToConvert -match '^\\\\(?<Server>[\w-]+)$' -or
                    $PathToConvert -match '^\\\\(?<Server>[\w-]+)\\$'

                    if ($IsUNCPath)
                    {
                        $DrivePortion = (Test-IsNullorEmpty $Matches.Server) ? "//malformed-unc-path" : "//" + $Matches.Server
                        $SplitPath = (Test-IsNullorEmpty $Matches.ServerPath) ? [System.String]::Empty : ($Matches.ServerPath).Replace('\', '/')
                    }
                    else
                    {
                        if ($PathToConvert -notmatch $TruthTable.WindowsVolumeSeparator)
                        {
                            Write-Warning "The path {$PathToConvert} does not contain a Windows volume separator. Defaulting to /"
                            ($DrivePortion, $SplitPath) = ([System.String]::Empty, $PathToConvert.Replace('\', '/'))
                        }
                        else
                        {
                            ($DrivePortion, $SplitPath) = ($PathToConvert.Split($TruthTable.WindowsVolumeSeparator)[0], $PathToConvert.Split($TruthTable.WindowsVolumeSeparator)[1])
                        }
                    }

                    [string] $WSLStylePath = ("/mnt/" + "$($DrivePortion.ToLower())$SplitPath").Replace('\', '/')

                    if ($ReturnObject.HasUnixRootPath)
                    {
                        $ReturnObject.ConvertedPaths += "${VolumeRoot}${DrivePortion}${SplitPath}".Replace('\', '/')
                        $ReturnObject.UnixPaths.Style_WSL += $WSLStylePath
                        $ReturnObject.UnixPaths.Style_MSYS2 +=
                        $ReturnObject.Style_Git += ("${VolumeRoot}$($DrivePortion.ToLower())$SplitPath").Replace('\', '/')
                    }
                    else
                    {
                        $ReturnObject.ConvertedPaths += "$WSLStylePath"
                        $ReturnObject.UnixPaths.Style_WSL += $WSLStylePath
                        $ReturnObject.UnixPaths.Style_MSYS2 +=
                        $ReturnObject.UnixPaths.Style_Git += ("${VolumeRoot}$($DrivePortion.ToLower())$SplitPath").Replace('\', '/')
                    }
                    
                    $ReturnObject.NumConverted++
                }
                catch
                {
                    $ConversionError = $_
                    Write-Error "An error occurred while converting the path {$PathToConvert} to a Unix-style path. The error was: "
                    Write-Error "$($ConversionError.Exception.Message)"
                    $ReturnObject.NumErrors++
                }
            }

            # if ($IsconversionToWindows) {
            #     [string] $DriveLetter = [System.String]::Empty
            #     [string] $WSLRegex = '^/mnt/(?<DriveLetter>[a-zA-Z])'
            #     [string] $MSYS2Regex = '^/((?<DriveLetter>[a-zA-Z])'

            #     [bool] $ContainsDrive = ($PathToConvert -imatch $WSLRegex) -or ($PathToConvert -imatch $MSYS2Regex)
            #     $DriveLetter = (Test-IsNullorEmpty $Matches) ? $Matches.DriveLetter : $null

            #     if ($PSCmdlet.ParameterSetName -eq 'WindowsPath') {

            #     }

            #     if ($PathToConvert -notmatch $TruthTable.WindowsVolumeSeparator) {
            #         Write-Warning "The path {$PathToConvert} does not contain a Windows volume separator. Defaulting to C:"
            #     }
            #     else {
            #         $PathBuilder = [System.Text.StringBuilder]::new("$VolumeRoot")
            #     }
            # }

        }

    }

    end
    {
        # Adding this here regardless if it was a copy operation or not because people
        # change their minds, and it's much better to have something and not need it than
        # to need it and not have it.

        [ScriptBlock] $ReturnObjectCopyMethod = {
            if (($null -eq $this.ConvertedPaths) -or ($this.ConvertedPaths.Count -eq 0))
            {
                throw "The ConvertedPaths property array is null or empty. Clipboard data not altered."
            }

            [System.Text.StringBuilder] $StringBuilder = [System.Text.StringBuilder]::new()
            foreach ($ConvertedPath in $this.ConvertedPaths)
            {
                if ($this.ConvertedPaths.Count -eq 1)
                {
                    $StringBuilder.Append($ConvertedPath).Append($([System.Convert]::ToChar(0))) | Out-Null

                    # Append a null character to the end of the string to ensure there's no trailing newline
                    # when pasting, because for some reason .Trim() doesn't work. It's probably operating on the
                    # string beforehand and Set-Clipboard is adding a newline character regardless.

                    $ClipData = $StringBuilder.ToString()
                    Set-Clipboard -Value "$ClipData"
                    Write-Information -MessageData "1 path copied to clipboard."
                    return
                }
                $StringBuilder.AppendLine($ConvertedPath) | Out-Null
            }

            # No need for the null char here, because the newline is expected in an array.
            $ClipData = $StringBuilder.ToString()
            Set-Clipboard -Value "$ClipData"
            Write-Information -MessageData "$($this.ConvertedPaths.Count) paths copied to clipboard."
        }

        $ReturnObject | Add-Member -MemberType ScriptMethod -Name "Copy" -Value $ReturnObjectCopyMethod

        if ($TruthTable.IsCopyOperation)
        {
            $ReturnObject.Copy()
        }

        return $ReturnObject
    }
}
#endregion

# try {
#     Set-LocalBinPath -LocalBinDir "$env:USERPROFILE\.local\bin" -Recurse -Confirm:$false -Debug
# }
# catch {
#     $BinPathError = $_
#     Write-Host "Could not set local bin path. The error was: "
#     $BinPathError.Exception.Message
#     $BinPathError.InvocationInfo
# }
# try {
#     Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
#         Set-LocalBinPath -OriginalPath $__UserPATHOnLaunch -Reset -Debug -Confirm:$false
#     } | Out-Null
# }
# catch {
#     $RegisterEventError = $_
#     Write-Host "Could not register PATH reset powershell exiting event. The error was: "
#     $RegisterEventError.Exception.Message
#     $RegisterEventError.InvocationInfo
# }

# Import the Chocolatey Profile that contains the necessary code to enable
# tab-completions to function for `choco`.
# Be aware that if you are missing these lines from your profile, tab completion
# for `choco` will not function.
# See https://ch0.co/tab-completion for details.
$ChocolateyProfile = "$env:ChocolateyInstall\helpers\chocolateyProfile.psm1"
if (Test-Path($ChocolateyProfile))
{
    Import-Module "$ChocolateyProfile"
}

function Test-IsAdmin
{
    if ($IsWindows)
    {
        $id = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = [Security.Principal.WindowsPrincipal] $id
        $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
    else
    {
        $id = id -u
        $id -eq 0
    }
}


$env:DOTNET_CLI_TELEMETRY_OPTOUT = "true"

Set-Alias grep Select-String
Set-Alias zip Compress-Archive
Set-Alias unzip Expand-Archive

function repos()
{
    Set-Location "$env:USERPROFILE\source\repos"
}

function garbagecan()
{
    Set-Location "$env:USERPROFILE\source\repos\TheDumpsterFire"
}


$isAdmin = (Test-IsAdmin)
$hostname = [Environment]::MachineName.ToLower()

try
{
    Import-Module -Name posh-git -MinimumVersion 1.0.0 -ErrorAction Stop
    # Use a minimalish Git status.
    $GitPromptSettings.BeforeStatus = ""
    $GitPromptSettings.AfterStatus = ""
    $GitPromptSettings.PathStatusSeparator = ""
    $GitPromptSettings.BranchIdenticalStatusSymbol.ForegroundColor = [ConsoleColor]::DarkGreen
    $GitPromptSettings.BranchAheadStatusSymbol.ForegroundColor = [ConsoleColor]::DarkYellow
    $GitPromptSettings.BranchBehindAndAheadStatusSymbol.ForegroundColor = [ConsoleColor]::DarkMagenta
}
  
catch
{
    Write-Warning "posh-git is not installed! Try:"
    Write-Warning "Install-Module -Name posh-git -AllowPrerelease -Scope CurrentUser"
}

<#
.SYNOPSIS
  Idempotently add/move paths to the front/back of your PATH.
.DESCRIPTION
  Will only add the path if it exists (but does not error). Defaults to adding
  to the back as this is safer. Can be called repeatedly (such as in a profile)
  but will always result in the same output (idempotent).
#>
function Edit-Path
{
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [ValidateScript({ Test-Path -Path $_ -IsValid })]
        # List of paths to add.
        [string[]]$Path,
        # Switch to add paths to the front instead of the back.
        [switch]$Front = $false
    )
    begin { $acc = [Collections.Generic.List[string]]::new() }
    process
    {
        $Path | Where-Object { Test-Path $_ } |
        ForEach-Object { $acc.Add([string](Resolve-Path $_)) }
    }
    end
    {
        $old = $env:PATH.split([IO.Path]::PathSeparator) | Where-Object { $_ -notin $acc }
        $new = if ($Front) { @($acc; $old) } else { @($old; $acc) }
        $p = $new -join [IO.Path]::PathSeparator
        if ($PSCmdlet.ShouldProcess("Updating PATH from:`n$env:PATH`nto:`n$p", $env:PATH, $p))
        {
            $env:PATH = $p
        }
    }
}

Edit-Path -Front "$env:USERPROFILE\.local\bin"

function Set-TerminalPID
{
    [CmdletBinding()]
    param (
        [Parameter()]
        [switch]
        $FirstRun
    )
    $CurrentTitle = $Host.UI.RawUI.WindowTitle
    
    if ($FirstRun)
    {
        ## Set the terminal titlebar to contain the current pwsh PID
        $Host.UI.RawUI.WindowTitle = "$CurrentTitle - PID: $PID"
        return
    }

    if ($CurrentTitle -eq $DefaultTermTitlePID)
    {
        ## Already has PID and is default prompt, no need to do anything at all
        return
    }

    if ($CurrentTitle -match "(?<WindowTitle>.*) - PID: ")
    {
        ## Something else set the title, capture that custom title and tack the PID onto the end of it
        $CurrentCustomTitle = $Matches.WindowTitle
        $Host.UI.RawUI.WindowTitle = "$CurrentCustomTitle - PID: $PID"
    }

    else
    {
        ## Shouldn't ever get here, but just in case
        $Host.UI.RawUI.WindowTitle = "$CurrentTitle - PID: $PID"
    }
}

Set-TerminalPID -FirstRun

#New-Item -Path "Env:\" -Name "VCPKG_KEEP_ENV_VARS" -Value "VSCMD_SKIP_SENDTELEMETRY"
New-Item -Path "Env:\" -Name "VSCMD_SKIP_SENDTELEMETRY" -Value 1 -Force | Out-Null
