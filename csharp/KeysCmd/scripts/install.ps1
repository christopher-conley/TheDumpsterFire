#!/usr/bin/env pwsh

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = "Low", DefaultParameterSetName = "Default")]
param (
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true, ParameterSetName = "Install")]
    [ValidateScript({

            if ($null -eq $_)
            {
                Write-Error -Message "`nArgument to -Install must be a string path to the OpenSSH install zipfile or a .NET FileInfo object of the zipfile`n"
                return $false
            }

            if ($_ -isnot [string] -and $_ -isnot [System.IO.FileInfo])
            {
                Write-Error -Message "`nArgument to -Install must be a string path to the OpenSSH install zipfile or a .NET FileInfo object of the zipfile`n"
                return $false
            }

            if ($_ -is [string])
            {
                if ([System.String]::IsNullOrWhiteSpace($_))
                {
                    Write-Error -Message "`nArgument to -Install must be a string path to the OpenSSH install zipfile or a .NET FileInfo object of the zipfile`n"
                    return $false
                }
            }

            if ($_ -is [System.IO.FileInfo])
            {
                if ([System.String]::IsNullOrWhiteSpace($($_.FullName)))
                {
                    Write-Error -Message "`nArgument to -Install must be a string path to the OpenSSH install zipfile or a .NET FileInfo object of the zipfile`n"
                    return $false
                }
            }
            return $true

        })]
    [object]
    $InstallLocalOpenSSHZipFile,

    [Parameter(Mandatory = $true, ParameterSetName = "Download")]
    [switch]
    $DownloadAndInstallOpenSSH,

    [Parameter(Mandatory = $false, ParameterSetName = "Install")]
    [Parameter(Mandatory = $false, ParameterSetName = "Download")]
    [Parameter(Mandatory = $true, ParameterSetName = "InstallKeysCmd")]
    [switch]
    $InstallKeysCmd,

    [Parameter(Mandatory = $false)]
    [array]
    $AllowGroups,

    [Parameter(Mandatory = $false)]
    [switch]
    $InstallIncludedSSHDConfig,

    [Parameter(Mandatory = $false)]
    [switch]
    $FixKeysCmdPermissions

)

begin
{

    [PSCustomObject] $OpenSSHReleases = Invoke-RestMethod -Uri 'https://api.github.com/repos/PowerShell/Win32-OpenSSH/releases/latest'
    [string] $OpenSSHDownloadURL = ($OpenSSHReleases.assets | Where-Object { $_.name -eq 'OpenSSH-Win64.zip' }).browser_download_url
    [bool] $HostisWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
    [string] $SSHInstallDir = 'C:\Program Files\OpenSSH'
    [bool] $ExistingInstall = Test-Path -Path "$SSHInstallDir"
    [bool] $OverwriteExisting = $false

    if (!$HostisWindows)
    {
        throw [System.PlatformNotSupportedException]::new("This script is only supported on Windows")
        exit 1
    }

    [string] $SourceZipFile = [System.String]::Empty

    # Check if running as admin
    if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))
    {
        Write-Information -MessageData "" -InformationAction Continue
        Write-Warning -Message "This script must be run as an administrator`n"
        [string] $Response = Read-Host -Prompt "Do you want to relaunch as an administrator with the same arguments? (Y/N)"
        switch ($Response)
        {
            { ($_ -eq 'y') -or { $_ -eq "yes" } }
            {
                [bool] $Pwsh7Exists = [System.IO.File]::Exists("C:\Program Files\PowerShell\7\pwsh.exe")
                [string] $PwshBinary = [System.String]::Empty

                if ($Pwsh7Exists)
                {
                    $PwshBinary = "C:\Program Files\PowerShell\7\pwsh.exe"
                }
                else
                {
                    $PwshBinary = "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"
                }


                [hashtable] $ParmsHash = [ordered]@{}

                foreach ($Key in $PSBoundParameters.Keys)
                {
                    $ParmsHash.Add($Key, $PSBoundParameters[$Key])
                }

                [string] $ArgList = "-ExecutionPolicy Bypass -File `"$PSCommandPath`""
                foreach ($Key in $ParmsHash.Keys)
                {
                    $ArgList += " -$Key `"$($ParmsHash[$Key])`""
                }

                Start-Process -FilePath "$PwshBinary" -ArgumentList "$ArgList" -Verb RunAs
                exit 0
            }

            { ($_ -eq 'n') -or { $_ -eq "no" } }
            {
                Write-Information -MessageData "Exiting" -InformationAction Continue
                exit 0
            }

            default
            {
                Write-Warning -Message "Invalid response"
                exit 1
            }
        }
    }

    # Check if OpenSSH is already installed
    if ((Get-Service -Name sshd -ErrorAction SilentlyContinue) -and ($PSCmdlet.ParameterSetName -eq "Install" -or $PSCmdlet.ParameterSetName -eq "Download"))
    {
        [string] $Response = [System.String]::Empty
        [bool] $ResponseIsValid = $false
        Write-Information -MessageData "" -InformationAction Continue
        Write-Warning -Message "OpenSSH is already installed. Reinstalling will REMOVE all files and directories in the existing installation at: $SSHInstallDir`n"

        do
        {
            [string] $Response = Read-Host -Prompt "Do you want to REMOVE the existing OpenSSH installation and reinstall? (Y/N)"
            switch ($Response)
            {

                # Yes, I know that checking to see if the user responded with LITERAL quotes in their response
                # seems dumb, but let me tell you this: I've been a Sysadmin for a long time. I've seen and dealt
                # with the aftermath of people doing something FAR dumber than this MANY times, a number which would
                # overflow a 64-bit unsigned int.
                # 
                # So no, it's not dumb.

                { ($_ -eq 'y') -or { $_ -eq "yes" } -or ($_ -eq "`"y`"") -or { $_ -eq "`"yes`"" } -or ($_ -eq "'y'") -or { $_ -eq "'yes'" } }
                {
                    Write-Warning -Message "Overwriting existing OpenSSH installation"
                    $OverwriteExisting = $true
                    $ResponseIsValid = $true
                    break;
                }
    
                { ($_ -eq 'n') -or { $_ -eq "no" } -or ($_ -eq "`"n`"") -or { $_ -eq "`"no`"" } -or ($_ -eq "'n'") -or { $_ -eq "'no'" } }
                {
                    Write-Information -MessageData "Exiting" -InformationAction Continue
                    $ResponseIsValid = $true
                    exit 0
                }
    
                default
                {
                    Write-Warning -Message "$($Response.ToString()) is not a valid response. Please enter `"y`", `"yes`", `"n`", or `"no`""
                }
            }
        } until ($ResponseIsValid)
    }

    # Check if OpenSSH install zipfile exists
    if ($InstallLocalOpenSSHZipFile -is [string])
    {
        if (-not (Test-Path -Path $InstallLocalOpenSSHZipFile))
        {
            throw [System.IO.FileNotFoundException]::new("OpenSSH install zipfile at `"$InstallLocalOpenSSHZipFile`" is inaccessible or does not exist. Please specify a valid path to the OpenSSH install zipfile.")
            exit 1
        }
        $SourceZipFile = $InstallLocalOpenSSHZipFile
    }
    elseif ($InstallLocalOpenSSHZipFile -is [System.IO.FileInfo])
    {
        if (-not (Test-Path -Path $InstallLocalOpenSSHZipFile.FullName))
        {
            throw [System.IO.FileNotFoundException]::new("OpenSSH install zipfile at `"$($InstallLocalOpenSSHZipFile.FullName)`" is inaccessible or does not exist. Please specify a valid path to the OpenSSH install zipfile.")
            exit 1
        }
        $SourceZipFile = $InstallLocalOpenSSHZipFile.FullName
    }

    [Microsoft.PowerShell.Commands.ComputerInfo] $HostInfo = Get-ComputerInfo
    [string] $HostDomain = [System.String]::Empty
    [string] $HostRole = $HostInfo.CsDomainRole
    [string] $LocalHostName = $HostInfo.CsName
    [string] $LocalAdminsGroup = 'Administrators'
    [string] $HostRSAKey = "C:\ProgramData\ssh\ssh_host_rsa_key"
    [string] $HostECDSAKey = "C:\ProgramData\ssh\ssh_host_ecdsa_key"
    [string] $HostED25519Key = "C:\ProgramData\ssh\ssh_host_ed25519_key"
    [string] $DomainBuiltinAdminsGroup = [System.String]::Empty
    [string] $SSHDAllowGroupsLine = [System.String]::Empty
    [string] $PWSH51BinaryPath = 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe'
    [string] $SSHTempDir = Join-Path -Path "$SSHInstallDir" -ChildPath "OpenSSH-Win64"
    [string] $SSHKeygenPath = 'C:\Program Files\OpenSSH\ssh-keygen.exe'
    [string] $SSHServiceInstallArgs = '-NonInteractive -ExecutionPolicy Bypass -File "C:\Program Files\OpenSSH\install-sshd.ps1"'
    [string] $SSHFixHostPermsArgs = '-NonInteractive -ExecutionPolicy Bypass -File "C:\Program Files\OpenSSH\FixHostFilePermissions.ps1"'
    [string] $SSHFixUserPermsArgs = '-NonInteractive -ExecutionPolicy Bypass -File "C:\Program Files\OpenSSH\FixUserFilePermissions.ps1"'
    [array] $SSHHostKeygenArgs = @(
        @{
            KeyType = "rsa"
            KeyFilepath = $HostRSAKey
            KeyArgs = "-q -t rsa -f `"$HostRSAKey`" -C `"`" -N `"`""
        },
        @{
            KeyType = "ecdsa"
            KeyFilepath = $HostECDSAKey
            KeyArgs = "-q -t ecdsa -f `"$HostECDSAKey`" -C `"`" -N `"`""
        },
        @{
            KeyType = "ed25519"
            KeyFilepath = $HostED25519Key
            KeyArgs = "-q -t ed25519 -f `"$HostED25519Key`" -C `"`" -N `"`""
        }
    )
    [array] $SSHDconfig = Get-Content -Path "$PSScriptRoot\sshd_config"

    [System.Security.Principal.NTAccount] $TrustedInstallerAcct = [System.Security.Principal.NTAccount]::new('NT Service\TrustedInstaller')
    [Microsoft.PowerShell.Commands.LocalPrincipal] $LocalAdminGroup = Get-LocalGroup -SID 'S-1-5-32-544'

    switch -Wildcard ($HostRole)
    {
        "StandaloneWorkstation"
        {
            $HostDomain = $HostInfo.CsDomain.ToLower()
        }
        "DomainController"
        {
            if ($HostInfo.CsDomain.IndexOf('.') -eq -1)
            {
                $HostDomain = $HostInfo.CsDomain.ToLower()
            }
            else
            {
                $HostDomain = $HostInfo.CsDomain.Split('.')[0].ToLower()
            }
            $DomainBuiltinAdminsGroup = "$HostDomain\$LocalAdminsGroup"
        }
        "MemberServer"
        {
            if ($HostInfo.CsDomain.IndexOf('.') -eq -1)
            {
                $HostDomain = $HostInfo.CsDomain.ToLower()
            }
            else
            {
                $HostDomain = $HostInfo.CsDomain.Split('.')[0].ToLower()
            }
            $DomainBuiltinAdminsGroup = "$HostDomain\$LocalAdminsGroup"
        }
        default
        {
            $HostDomain = $HostInfo.CsDomain.ToLower()
            $DomainBuiltinAdminsGroup = "$HostDomain\$LocalAdminsGroup"
        }
    }

    $SSHDAllowGroupsLine += "`"$DomainBuiltinAdminsGroup`" `"$LocalHostName\$LocalAdminGroup`" `"$LocalAdminsGroup`""

    if ($null -ne $AllowGroups -and $AllowGroups.Count -gt 0)
    {
        $SSHDAllowGroupsLine = [System.String]::Empty
        foreach ($Group in $AllowGroups)
        {
            if ($Group -isnot [string])
            {
                Write-Warning -Message "Encountered unknown object $($Group.ToString()) in AllowGroups array. Skipping."
                continue
            }

            Write-Information -MessageData "Adding group $Group to sshd_config AllowGroups line" -InformationAction Continue
            $SSHDAllowGroupsLine += "`"$($Group.ToLower())`" "
        }
    }

    $SSHDconfig = $SSHDconfig.Replace("PLACEHOLDERVALUECHANGEDBYINSTALLSCRIPT", "$SSHDAllowGroupsLine")

}

process
{

    if ($PSCmdlet.ParameterSetName -eq "Download")
    {
        Invoke-RestMethod -Uri "$OpenSSHDownloadURL" -OutFile "$env:TEMP\OpenSSH-Win64.zip"
        $SourceZipFile = "$env:TEMP\OpenSSH-Win64.zip"
    }

    if ($PSCmdlet.ParameterSetName -eq "Install" -or $PSCmdlet.ParameterSetName -eq "Download")
    {
        if ($ExistingInstall -and $OverwriteExisting)
        {
            try
            {
                if ($PSCmdlet.ShouldProcess("$SSHInstallDir", "Remove existing OpenSSH installation"))
                {
                    Remove-Item -Path "$SSHInstallDir" -Recurse -Force -Confirm:$false
                }
            }
            catch
            {
                $RemoveError = $_
                throw [System.IO.IOException]::new("Error removing existing OpenSSH installation: $RemoveError")
            }
        }

        try
        {
            if ($PSCmdlet.ShouldProcess("$SSHInstallDir", "Install OpenSSH"))
            {
                Expand-Archive -Path "$SourceZipFile" -DestinationPath "$SSHInstallDir"
            }
        }
        catch
        {
            $UnzipError = $_
            throw [System.IO.IOException]::new("Error extracting OpenSSH zipfile: $UnzipError")
            exit 1
        }

        try
        {
            Copy-Item -Path "$SSHTempDir\*" -Destination "$SSHInstallDir" -Force -Confirm:$false
            Remove-Item -Path "$SSHTempDir" -Recurse -Force -Confirm:$false
        }
        catch
        {
            $CopyError = $_
            throw [System.IO.IOException]::new("Error copying OpenSSH files and removing temp install dir: $CopyError")
            exit 1
        }

        if ($PSCmdlet.ShouldProcess("sshd and ssh-agent", "Install SSHD and SSH Agent as services"))
        {
            Start-Process -Wait -FilePath "$PWSH51BinaryPath" -ArgumentList "$SSHServiceInstallArgs" -Verb RunAs
            Start-Process -Wait -FilePath "$PWSH51BinaryPath" -ArgumentList "$SSHServiceInstallArgs" -Verb RunAs
            Start-Process -Wait -FilePath "$PWSH51BinaryPath" -ArgumentList "$SSHFixHostPermsArgs" -Verb RunAs
            Start-Process -Wait -FilePath "$PWSH51BinaryPath" -ArgumentList "$SSHFixUserPermsArgs" -Verb RunAs
        }

        # Set TrustedInstaller as owner of the OpenSSH dir
        [System.Security.AccessControl.FileSystemSecurity] $InstallACL = Get-Acl -Path $SSHInstallDir
        $InstallACL.SetOwner($TrustedInstallerAcct)
        Set-Acl -Path $SSHInstallDir -AclObject $InstallACL


        [array] $SSHFilesList = Get-ChildItem -Path "$SSHInstallDir" -Recurse -Force

        # Set TrustedInstaller as owner of all OpenSSH files/dirs
        foreach ($Item in $SSHFilesList)
        {
            $ACL = $null
            [System.Security.AccessControl.FileSystemSecurity] $ACL = Get-Acl -Path $Item.FullName
            $ACL.SetOwner($TrustedInstallerAcct)
            Set-Acl -Path $Item.FullName -AclObject $ACL
        }


        foreach ($KeyAlgorithm in $SSHHostKeygenArgs)
        {
            [string] $KeyType = $KeyAlgorithm.KeyType
            [string] $KeyFilepath = $KeyAlgorithm.KeyFilepath
            [string] $KeyArgs = $KeyAlgorithm.KeyArgs

            if ([System.IO.File]::Exists($KeyAlgorithm.KeyFilepath)) {
                Write-Information -MessageData "Host key for $KeyType algorithm already exists at $KeyFilepath, creating a backup"
                [string] $BackupKeyFile = "$KeyFilepath.$((Get-Date).ToString('yyyy-MM-ddTHH.mm.ss.fff')).bak"
                Rename-Item -Path $KeyFilepath -NewName $BackupKeyFile -Force -Confirm:$false
                Write-Information -MessageData "Existing host $KeyType key backed up to: $BackupKeyFile"
            }

            Start-Process -Wait -FilePath "$SSHKeygenPath" -ArgumentList "$KeyArgs" -Verb RunAs
        }

        Set-Service -Name "sshd" -StartupType Automatic
        Set-Service -Name "ssh-agent" -StartupType Automatic

        [CimInstance] $ExistingFirewallRule = Get-NetFirewallRule -DisplayNam "OpenSSH Remote Shell (KeysCmd added)" -ErrorAction SilentlyContinue

        if ($null -eq $ExistingFirewallRule) {
            [hashtable] $FirewallRuleOptions = @{
                DisplayName = "OpenSSH Remote Shell (KeysCmd added)"
                Description = "Allows incoming connections to the OpenSSH daemon listening on port 22 for remote shell access."
                Direction = "Inbound"
                LocalPort = 22
                Protocol = "TCP"
                Action = "Allow"
                EdgeTraversalPolicy = "Allow"
                Enabled = "True"
                InterfaceType = "Any"
                PolicyStore = "PersistentStore"
                Profile = "Any"
            }
            
            New-NetFirewallRule @FirewallRuleOptions
        }
    }

    if ($InstallIncludedSSHDConfig)
    {
        if ([System.IO.File]::Exists("C:\ProgramData\ssh\sshd_config"))
        {
            [string] $BackupFile = "C:\ProgramData\ssh\sshd_config.$((Get-Date).ToString('yyyy-MM-ddTHH.mm.ss.fff')).bak"
                Rename-Item -Path "C:\ProgramData\ssh\sshd_config" -NewName "$BackupFile" -Force -Confirm:$false
                Write-Warning -Message "An existing sshd_config file was found and backed up to: $BackupFile"
        }

        if ($PSCmdlet.ShouldProcess("C:\ProgramData\ssh\sshd_config", "Backup existing sshd_config and install included"))
        {
            Set-Content -Path "C:\ProgramData\ssh\sshd_config" -Value $SSHDconfig -Encoding utf8
            Start-Process -Wait -FilePath "$PWSH51BinaryPath" -ArgumentList "$SSHFixHostPermsArgs" -Verb RunAs
            Start-Process -Wait -FilePath "$PWSH51BinaryPath" -ArgumentList "$SSHFixUserPermsArgs" -Verb RunAs
        }
    }

    if ($InstallKeysCmd -or $FixKeysCmdPermissions)
    {
        [System.Security.AccessControl.FileSystemRights] $FullControl = [System.Security.AccessControl.FileSystemRights]::FullControl
        [System.Security.AccessControl.AccessControlType] $AllowPermission = [System.Security.AccessControl.AccessControlType]::Allow
        [System.Security.Principal.NTAccount] $SystemACCT = [System.Security.Principal.NTAccount]::new('NT Authority\SYSTEM')

        if ($InstallKeysCmd)
        {
            if ($PSCmdlet.ShouldProcess("C:\ProgramData\keyscmd", "Install KeysCmd"))
            {
                New-Item -ItemType Directory -Path "C:\ProgramData\keyscmd" -Force -Confirm:$false
                [array] $KeysCmdFiles = Get-ChildItem -Path "$PSScriptRoot\..\KeysCmd\bin\Release\net48\publish" -Recurse -Force
        
                foreach ($Item in $KeysCmdFiles)
                {
                    Copy-Item -Path $Item.FullName -Destination "C:\ProgramData\keyscmd" -Force -Confirm:$false
                }
            }
        }

        ## OpenSSH REQUIRES that the "AuthorizedKeysCommand" executable be owned by TrustedInstaller, and that
        ## the ONLY two ACLs set on the file are "FullControl" "Allow" for SYSTEM and the local Administrators group.
        ## That's it. No other permissions are allowed. At all. Period.
        ##
        ## Do not deviate from the ACLs set below or it WILL NOT work, I promise.
        ##
        ## The -FixKeysCmdPermissions switch to this script will set the proper permissions on the KeysCmd.exe file
        ## in case the permissions are modified for whatever reason and shit's broken because of it.

        if ($PSCmdlet.ShouldProcess("C:\ProgramData\keyscmd\KeysCmd.exe", "Set proper permissions on KeysCmd.exe"))
        {
            [System.Security.AccessControl.FileSystemSecurity] $KeysACL = Get-Acl -Path "C:\ProgramData\keyscmd\KeysCmd.exe"
            $KeysACL.SetAccessRuleProtection($true, $false)
            foreach ($Rule in $KeysACL.Access)
            {
                $KeysACL.RemoveAccessRule($Rule)
            }

            $KeysACL.SetOwner($TrustedInstallerAcct)
            [System.Security.AccessControl.FileSystemAccessRule] $AccessRule = [System.Security.AccessControl.FileSystemAccessRule]::new($SystemACCT, $FullControl, $AllowPermission)
            $KeysACL.AddAccessRule($AccessRule)
            $AccessRule = [System.Security.AccessControl.FileSystemAccessRule]::new($LocalAdminGroup, $FullControl, $AllowPermission)
            $KeysACL.AddAccessRule($AccessRule)

            Set-Acl -Path "C:\ProgramData\keyscmd\KeysCmd.exe" -AclObject $KeysACL
        }
    }
}

end
{

}



















# SIG # Begin signature block
# MIIooAYJKoZIhvcNAQcCoIIokTCCKI0CAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCDCEd5zFqQz5c2E
# QGmySYI/QOhzOoI0MqvYvZAmLjmQEKCCDaUwgga5MIIEoaADAgECAhEAmaOACiZV
# O2Wr3G6EprPqOTANBgkqhkiG9w0BAQwFADCBgDELMAkGA1UEBhMCUEwxIjAgBgNV
# BAoTGVVuaXpldG8gVGVjaG5vbG9naWVzIFMuQS4xJzAlBgNVBAsTHkNlcnR1bSBD
# ZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTEkMCIGA1UEAxMbQ2VydHVtIFRydXN0ZWQg
# TmV0d29yayBDQSAyMB4XDTIxMDUxOTA1MzIxOFoXDTM2MDUxODA1MzIxOFowVjEL
# MAkGA1UEBhMCUEwxITAfBgNVBAoTGEFzc2VjbyBEYXRhIFN5c3RlbXMgUy5BLjEk
# MCIGA1UEAxMbQ2VydHVtIENvZGUgU2lnbmluZyAyMDIxIENBMIICIjANBgkqhkiG
# 9w0BAQEFAAOCAg8AMIICCgKCAgEAnSPPBDAjO8FGLOczcz5jXXp1ur5cTbq96y34
# vuTmflN4mSAfgLKTvggv24/rWiVGzGxT9YEASVMw1Aj8ewTS4IndU8s7VS5+djSo
# McbvIKck6+hI1shsylP4JyLvmxwLHtSworV9wmjhNd627h27a8RdrT1PH9ud0IF+
# njvMk2xqbNTIPsnWtw3E7DmDoUmDQiYi/ucJ42fcHqBkbbxYDB7SYOouu9Tj1yHI
# ohzuC8KNqfcYf7Z4/iZgkBJ+UFNDcc6zokZ2uJIxWgPWXMEmhu1gMXgv8aGUsRda
# CtVD2bSlbfsq7BiqljjaCun+RJgTgFRCtsuAEw0pG9+FA+yQN9n/kZtMLK+Wo837
# Q4QOZgYqVWQ4x6cM7/G0yswg1ElLlJj6NYKLw9EcBXE7TF3HybZtYvj9lDV2nT8m
# FSkcSkAExzd4prHwYjUXTeZIlVXqj+eaYqoMTpMrfh5MCAOIG5knN4Q/JHuurfTI
# 5XDYO962WZayx7ACFf5ydJpoEowSP07YaBiQ8nXpDkNrUA9g7qf/rCkKbWpQ5bou
# fUnq1UiYPIAHlezf4muJqxqIns/kqld6JVX8cixbd6PzkDpwZo4SlADaCi2JSplK
# ShBSND36E/ENVv8urPS0yOnpG4tIoBGxVCARPCg1BnyMJ4rBJAcOSnAWd18Jx5n8
# 58JSqPECAwEAAaOCAVUwggFRMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFN10
# XUwA23ufoHTKsW73PMAywHDNMB8GA1UdIwQYMBaAFLahVDkCw6A/joq8+tT4HKbR
# Og79MA4GA1UdDwEB/wQEAwIBBjATBgNVHSUEDDAKBggrBgEFBQcDAzAwBgNVHR8E
# KTAnMCWgI6Ahhh9odHRwOi8vY3JsLmNlcnR1bS5wbC9jdG5jYTIuY3JsMGwGCCsG
# AQUFBwEBBGAwXjAoBggrBgEFBQcwAYYcaHR0cDovL3N1YmNhLm9jc3AtY2VydHVt
# LmNvbTAyBggrBgEFBQcwAoYmaHR0cDovL3JlcG9zaXRvcnkuY2VydHVtLnBsL2N0
# bmNhMi5jZXIwOQYDVR0gBDIwMDAuBgRVHSAAMCYwJAYIKwYBBQUHAgEWGGh0dHA6
# Ly93d3cuY2VydHVtLnBsL0NQUzANBgkqhkiG9w0BAQwFAAOCAgEAdYhYD+WPUCia
# U58Q7EP89DttyZqGYn2XRDhJkL6P+/T0IPZyxfxiXumYlARMgwRzLRUStJl490L9
# 4C9LGF3vjzzH8Jq3iR74BRlkO18J3zIdmCKQa5LyZ48IfICJTZVJeChDUyuQy6rG
# DxLUUAsO0eqeLNhLVsgw6/zOfImNlARKn1FP7o0fTbj8ipNGxHBIutiRsWrhWM2f
# 8pXdd3x2mbJCKKtl2s42g9KUJHEIiLni9ByoqIUul4GblLQigO0ugh7bWRLDm0Cd
# Y9rNLqyA3ahe8WlxVWkxyrQLjH8ItI17RdySaYayX3PhRSC4Am1/7mATwZWwSD+B
# 7eMcZNhpn8zJ+6MTyE6YoEBSRVrs0zFFIHUR08Wk0ikSf+lIe5Iv6RY3/bFAEloM
# U+vUBfSouCReZwSLo8WdrDlPXtR0gicDnytO7eZ5827NS2x7gCBibESYkOh1/w1t
# VxTpV2Na3PR7nxYVlPu1JPoRZCbH86gc96UTvuWiOruWmyOEMLOGGniR+x+zPF/2
# DaGgK2W1eEJfo2qyrBNPvF7wuAyQfiFXLwvWHamoYtPZo0LHuH8X3n9C+xN4YaNj
# t2ywzOr+tKyEVAotnyU9vyEVOaIYMk3IeBrmFnn0gbKeTTyYeEEUz/Qwt4HOUBCr
# W602NCmvO1nm+/80nLy5r0AZvCQxaQ4wggbkMIIEzKADAgECAhA7U5aXFldstcsh
# Wwg7IMKaMA0GCSqGSIb3DQEBCwUAMFYxCzAJBgNVBAYTAlBMMSEwHwYDVQQKExhB
# c3NlY28gRGF0YSBTeXN0ZW1zIFMuQS4xJDAiBgNVBAMTG0NlcnR1bSBDb2RlIFNp
# Z25pbmcgMjAyMSBDQTAeFw0yNDAzMjUxNjU3NDVaFw0yNTAzMjUxNjU3NDRaMIGJ
# MQswCQYDVQQGEwJVUzERMA8GA1UECAwIVmlyZ2luaWExEzARBgNVBAcMCkFsZXhh
# bmRyaWExHjAcBgNVBAoMFU9wZW4gU291cmNlIERldmVsb3BlcjEyMDAGA1UEAwwp
# T3BlbiBTb3VyY2UgRGV2ZWxvcGVyLCBDaHJpc3RvcGhlciBDb25sZXkwggIiMA0G
# CSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQC0ePATYBR3lyqet0e5n7lSGxpIcBDc
# wbmsHV7JhnRBEGfCrbWIJSqrGnRtthyxQH0xb55GFxW+U2dOjx74HclR7GfiBl5B
# fwZtUSI4vaWkRw/FkHNeEc+L9j4w1exnUev0WaL+yNSTuOLF7q9vygY8TC38eC2b
# pCJ9KlczloGWj4CM/dwPmBjZTavru5+Hie7M4RyxE8gYCgC+6mbvhzvtxhdxPXFy
# L2/6fVRAHZIBJc6ta5K0NYXc//Rfb2f7c2y+twThr6KnAOW+nhUpoZ9mU8JfnD53
# T11q5GxCYjc56PUrtOdjV5wHee2EPuuI8xRR4eBL5FZl7D5cvWgGWpX43g7+NF2G
# WNLab25P0z6h1ogfXB4fW3Rx/BU4vww6LU5tk9Z9mopE+V5lWV7BBSb+KgyoTTCI
# p9afwEU3YgLCgfVw6eVrYAEfpEeMGQY97m/sv6NvSaQcTZo6wM4CgrjmFFFlDNey
# xOszB9Nm6zRBfb9wOmKiY4P4DwnMKc4PZx7JB8shW05IygWCifN9U0eHe3qLoSvh
# NYqSGSflvVAxa0aJASPaYCffR8dDPeumWMRacAn0uFPJtxydoI4X732XcqUblgvC
# C8mmqZo1SvgkW/vJrAByQmpURLEmabwgMe1l2L6nX0cvhbovtjIUWuSfGGNKN2fk
# Wa32gxFTlweSGQIDAQABo4IBeDCCAXQwDAYDVR0TAQH/BAIwADA9BgNVHR8ENjA0
# MDKgMKAuhixodHRwOi8vY2NzY2EyMDIxLmNybC5jZXJ0dW0ucGwvY2NzY2EyMDIx
# LmNybDBzBggrBgEFBQcBAQRnMGUwLAYIKwYBBQUHMAGGIGh0dHA6Ly9jY3NjYTIw
# MjEub2NzcC1jZXJ0dW0uY29tMDUGCCsGAQUFBzAChilodHRwOi8vcmVwb3NpdG9y
# eS5jZXJ0dW0ucGwvY2NzY2EyMDIxLmNlcjAfBgNVHSMEGDAWgBTddF1MANt7n6B0
# yrFu9zzAMsBwzTAdBgNVHQ4EFgQUQ4COF3+Ix+Ok+JBZ3PoIdagMkPIwSwYDVR0g
# BEQwQjAIBgZngQwBBAEwNgYLKoRoAYb2dwIFAQQwJzAlBggrBgEFBQcCARYZaHR0
# cHM6Ly93d3cuY2VydHVtLnBsL0NQUzATBgNVHSUEDDAKBggrBgEFBQcDAzAOBgNV
# HQ8BAf8EBAMCB4AwDQYJKoZIhvcNAQELBQADggIBAC1Oi6ByZPHnRP4fTQWfYNcf
# hfKKekheLYXW1adY2TZzChwTe5l0PAn15Nt+1JkouusgOfIKYltzFtO3PaOdjZ2H
# N0diw6twJRsyHs/B/cGSHQ1taJdmb6JZ6rZUsck46QkzuSuceBXFQywNji3T6Th7
# nAJpS21oVgdolQKdalwAEXO3P8AKgGGbjMWam6dHML28cbTFZKRoC6A4nNeXmTCq
# D2mT68l/Ybmic8BOBdMQ8X3yScDIOvoHcn9or2jH5lfLY8JkUto5wMYCIXg8id5J
# CvWu+D3mnEYFjoymf/Z5OaOE60w8w199T/c8d0EknuKaJCroBdPlvDjjqKHfLc8G
# BY5Aa6eYukWxCJ3Mo609lHrb33q2gVpadhRsCoGLOboAD7/KQcDih0kg2FWgLhWs
# Qw2QOiNMPVL7kvkKuhU5NoFDjWuT7sUx8vb7cyDfhiK4dSmt6ChmfO11dIfQwkFk
# I/AAjuKuX+NtQNDfLcAyIBbzMFGg5Inoubz//F3RHqPpz/W4oebThCHpMIF8JOVO
# DNtODSXa5/IrCYdsTJwJE0rTc0sC35w6+c6VLVtFlBdEWZU/jmkG7qEK2H3FuDLO
# KCa5dYMaHBMh0pxUpKKF7slvKyk3s3gLluNg1WGoQVHip+IUooka+OYtA95tSfEc
# EIuGjXJneFe9xX3eJDNfMYIaUTCCGk0CAQEwajBWMQswCQYDVQQGEwJQTDEhMB8G
# A1UEChMYQXNzZWNvIERhdGEgU3lzdGVtcyBTLkEuMSQwIgYDVQQDExtDZXJ0dW0g
# Q29kZSBTaWduaW5nIDIwMjEgQ0ECEDtTlpcWV2y1yyFbCDsgwpowDQYJYIZIAWUD
# BAIBBQCgfDAQBgorBgEEAYI3AgEMMQIwADAZBgkqhkiG9w0BCQMxDAYKKwYBBAGC
# NwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAvBgkqhkiG9w0BCQQx
# IgQgtD+/fgnfwDaG2Oizz8Zu4Rr9AZTatYLcyAByT/8WTSkwDQYJKoZIhvcNAQEB
# BQAEggIACfOGQi2Z4/DuGbNWpUcvU5O1oUQefjzV9qHg/yOOWfSPPjrnM8Rnoav4
# j+6a3UNUxovRqLr+wuOOzvceNbLpdQfc3wLnkcYhzCoPwffd7FjIbFlLOwA/5XlA
# SVyW4xKrOLle7uIbqWlcE7m1D+y7y+GlWQjVH9+oQp3glDI3rD6DUPQesW9saD6S
# bLunvOqHVopoKEmovDsH7QNY2atIneHkGW+mgY1YsPXBuQ8s2VWG12FpwLHECZYi
# UYXLD6DwY4HowywfU9XJyecVmi3ehUQWFeLUFn1kWBhbYnV7s4kIFp1DSwJ5mKE5
# iuFRrhboQ16qbX8jK2h62+kxsqhdX1s5B1fVGOE4n5YMpJ1NvQc6Od4UZ1m0r7Ep
# Ju3Fz0JGGRPUBObvMbr6ctX3N2anYPEeVorAMrV5B4Fpd+yRcd6NEJEDXuhPAZP2
# ylY06j9mTDUieCd1bSgjOoiMZXv9bdlpwak7sJOIJkDJg9Za/Ec6WcpJ6rucIVtb
# q8NxOpc7kmWeBetSoujwI0oid41EcAxOnT+4TwYzi4n8SIRpj8Paz3v/u/GZrOs1
# bVrejExI1KQj6yJqUgpLtmm5tTLMzIFxFFaK6kKInW+r+pNnJ7uWTp0QID/r6J7K
# RXY+qAtFwZH6mkBJy60GWzv9sLF9WalVB+XbJEzpR76tuJaR1Zihghc6MIIXNgYK
# KwYBBAGCNwMDATGCFyYwghciBgkqhkiG9w0BBwKgghcTMIIXDwIBAzEPMA0GCWCG
# SAFlAwQCAQUAMHgGCyqGSIb3DQEJEAEEoGkEZzBlAgEBBglghkgBhv1sBwEwMTAN
# BglghkgBZQMEAgEFAAQg2cylI+gksM9pLvs5ZFMu0K/TVvNeHig+I7EFTT1G5AIC
# EQCTlx5r1E1olMYxNdvVtemGGA8yMDI1MDExMjE4MzIxMFqgghMDMIIGvDCCBKSg
# AwIBAgIQC65mvFq6f5WHxvnpBOMzBDANBgkqhkiG9w0BAQsFADBjMQswCQYDVQQG
# EwJVUzEXMBUGA1UEChMORGlnaUNlcnQsIEluYy4xOzA5BgNVBAMTMkRpZ2lDZXJ0
# IFRydXN0ZWQgRzQgUlNBNDA5NiBTSEEyNTYgVGltZVN0YW1waW5nIENBMB4XDTI0
# MDkyNjAwMDAwMFoXDTM1MTEyNTIzNTk1OVowQjELMAkGA1UEBhMCVVMxETAPBgNV
# BAoTCERpZ2lDZXJ0MSAwHgYDVQQDExdEaWdpQ2VydCBUaW1lc3RhbXAgMjAyNDCC
# AiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAL5qc5/2lSGrljC6W23mWaO1
# 6P2RHxjEiDtqmeOlwf0KMCBDEr4IxHRGd7+L660x5XltSVhhK64zi9CeC9B6lUdX
# M0s71EOcRe8+CEJp+3R2O8oo76EO7o5tLuslxdr9Qq82aKcpA9O//X6QE+AcaU/b
# yaCagLD/GLoUb35SfWHh43rOH3bpLEx7pZ7avVnpUVmPvkxT8c2a2yC0WMp8hMu6
# 0tZR0ChaV76Nhnj37DEYTX9ReNZ8hIOYe4jl7/r419CvEYVIrH6sN00yx49boUuu
# mF9i2T8UuKGn9966fR5X6kgXj3o5WHhHVO+NBikDO0mlUh902wS/Eeh8F/UFaRp1
# z5SnROHwSJ+QQRZ1fisD8UTVDSupWJNstVkiqLq+ISTdEjJKGjVfIcsgA4l9cbk8
# Smlzddh4EfvFrpVNnes4c16Jidj5XiPVdsn5n10jxmGpxoMc6iPkoaDhi6JjHd5i
# bfdp5uzIXp4P0wXkgNs+CO/CacBqU0R4k+8h6gYldp4FCMgrXdKWfM4N0u25OEAu
# Ea3JyidxW48jwBqIJqImd93NRxvd1aepSeNeREXAu2xUDEW8aqzFQDYmr9ZONuc2
# MhTMizchNULpUEoA6Vva7b1XCB+1rxvbKmLqfY/M/SdV6mwWTyeVy5Z/JkvMFpnQ
# y5wR14GJcv6dQ4aEKOX5AgMBAAGjggGLMIIBhzAOBgNVHQ8BAf8EBAMCB4AwDAYD
# VR0TAQH/BAIwADAWBgNVHSUBAf8EDDAKBggrBgEFBQcDCDAgBgNVHSAEGTAXMAgG
# BmeBDAEEAjALBglghkgBhv1sBwEwHwYDVR0jBBgwFoAUuhbZbU2FL3MpdpovdYxq
# II+eyG8wHQYDVR0OBBYEFJ9XLAN3DigVkGalY17uT5IfdqBbMFoGA1UdHwRTMFEw
# T6BNoEuGSWh0dHA6Ly9jcmwzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydFRydXN0ZWRH
# NFJTQTQwOTZTSEEyNTZUaW1lU3RhbXBpbmdDQS5jcmwwgZAGCCsGAQUFBwEBBIGD
# MIGAMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC5kaWdpY2VydC5jb20wWAYIKwYB
# BQUHMAKGTGh0dHA6Ly9jYWNlcnRzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydFRydXN0
# ZWRHNFJTQTQwOTZTSEEyNTZUaW1lU3RhbXBpbmdDQS5jcnQwDQYJKoZIhvcNAQEL
# BQADggIBAD2tHh92mVvjOIQSR9lDkfYR25tOCB3RKE/P09x7gUsmXqt40ouRl3lj
# +8QioVYq3igpwrPvBmZdrlWBb0HvqT00nFSXgmUrDKNSQqGTdpjHsPy+LaalTW0q
# VjvUBhcHzBMutB6HzeledbDCzFzUy34VarPnvIWrqVogK0qM8gJhh/+qDEAIdO/K
# kYesLyTVOoJ4eTq7gj9UFAL1UruJKlTnCVaM2UeUUW/8z3fvjxhN6hdT98Vr2FYl
# CS7Mbb4Hv5swO+aAXxWUm3WpByXtgVQxiBlTVYzqfLDbe9PpBKDBfk+rabTFDZXo
# Uke7zPgtd7/fvWTlCs30VAGEsshJmLbJ6ZbQ/xll/HjO9JbNVekBv2Tgem+mLptR
# 7yIrpaidRJXrI+UzB6vAlk/8a1u7cIqV0yef4uaZFORNekUgQHTqddmsPCEIYQP7
# xGxZBIhdmm4bhYsVA6G2WgNFYagLDBzpmk9104WQzYuVNsxyoVLObhx3RugaEGru
# +SojW4dHPoWrUhftNpFC5H7QEY7MhKRyrBe7ucykW7eaCuWBsBb4HOKRFVDcrZgd
# waSIqMDiCLg4D+TPVgKx2EgEdeoHNHT9l3ZDBD+XgbF+23/zBjeCtxz+dL/9NWR6
# P2eZRi7zcEO1xwcdcqJsyz/JceENc2Sg8h3KeFUCS7tpFk7CrDqkMIIGrjCCBJag
# AwIBAgIQBzY3tyRUfNhHrP0oZipeWzANBgkqhkiG9w0BAQsFADBiMQswCQYDVQQG
# EwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNl
# cnQuY29tMSEwHwYDVQQDExhEaWdpQ2VydCBUcnVzdGVkIFJvb3QgRzQwHhcNMjIw
# MzIzMDAwMDAwWhcNMzcwMzIyMjM1OTU5WjBjMQswCQYDVQQGEwJVUzEXMBUGA1UE
# ChMORGlnaUNlcnQsIEluYy4xOzA5BgNVBAMTMkRpZ2lDZXJ0IFRydXN0ZWQgRzQg
# UlNBNDA5NiBTSEEyNTYgVGltZVN0YW1waW5nIENBMIICIjANBgkqhkiG9w0BAQEF
# AAOCAg8AMIICCgKCAgEAxoY1BkmzwT1ySVFVxyUDxPKRN6mXUaHW0oPRnkyibaCw
# zIP5WvYRoUQVQl+kiPNo+n3znIkLf50fng8zH1ATCyZzlm34V6gCff1DtITaEfFz
# sbPuK4CEiiIY3+vaPcQXf6sZKz5C3GeO6lE98NZW1OcoLevTsbV15x8GZY2UKdPZ
# 7Gnf2ZCHRgB720RBidx8ald68Dd5n12sy+iEZLRS8nZH92GDGd1ftFQLIWhuNyG7
# QKxfst5Kfc71ORJn7w6lY2zkpsUdzTYNXNXmG6jBZHRAp8ByxbpOH7G1WE15/teP
# c5OsLDnipUjW8LAxE6lXKZYnLvWHpo9OdhVVJnCYJn+gGkcgQ+NDY4B7dW4nJZCY
# OjgRs/b2nuY7W+yB3iIU2YIqx5K/oN7jPqJz+ucfWmyU8lKVEStYdEAoq3NDzt9K
# oRxrOMUp88qqlnNCaJ+2RrOdOqPVA+C/8KI8ykLcGEh/FDTP0kyr75s9/g64ZCr6
# dSgkQe1CvwWcZklSUPRR8zZJTYsg0ixXNXkrqPNFYLwjjVj33GHek/45wPmyMKVM
# 1+mYSlg+0wOI/rOP015LdhJRk8mMDDtbiiKowSYI+RQQEgN9XyO7ZONj4KbhPvbC
# dLI/Hgl27KtdRnXiYKNYCQEoAA6EVO7O6V3IXjASvUaetdN2udIOa5kM0jO0zbEC
# AwEAAaOCAV0wggFZMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFLoW2W1N
# hS9zKXaaL3WMaiCPnshvMB8GA1UdIwQYMBaAFOzX44LScV1kTN8uZz/nupiuHA9P
# MA4GA1UdDwEB/wQEAwIBhjATBgNVHSUEDDAKBggrBgEFBQcDCDB3BggrBgEFBQcB
# AQRrMGkwJAYIKwYBBQUHMAGGGGh0dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBBBggr
# BgEFBQcwAoY1aHR0cDovL2NhY2VydHMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0VHJ1
# c3RlZFJvb3RHNC5jcnQwQwYDVR0fBDwwOjA4oDagNIYyaHR0cDovL2NybDMuZGln
# aWNlcnQuY29tL0RpZ2lDZXJ0VHJ1c3RlZFJvb3RHNC5jcmwwIAYDVR0gBBkwFzAI
# BgZngQwBBAIwCwYJYIZIAYb9bAcBMA0GCSqGSIb3DQEBCwUAA4ICAQB9WY7Ak7Zv
# mKlEIgF+ZtbYIULhsBguEE0TzzBTzr8Y+8dQXeJLKftwig2qKWn8acHPHQfpPmDI
# 2AvlXFvXbYf6hCAlNDFnzbYSlm/EUExiHQwIgqgWvalWzxVzjQEiJc6VaT9Hd/ty
# dBTX/6tPiix6q4XNQ1/tYLaqT5Fmniye4Iqs5f2MvGQmh2ySvZ180HAKfO+ovHVP
# ulr3qRCyXen/KFSJ8NWKcXZl2szwcqMj+sAngkSumScbqyQeJsG33irr9p6xeZmB
# o1aGqwpFyd/EjaDnmPv7pp1yr8THwcFqcdnGE4AJxLafzYeHJLtPo0m5d2aR8XKc
# 6UsCUqc3fpNTrDsdCEkPlM05et3/JWOZJyw9P2un8WbDQc1PtkCbISFA0LcTJM3c
# HXg65J6t5TRxktcma+Q4c6umAU+9Pzt4rUyt+8SVe+0KXzM5h0F4ejjpnOHdI/0d
# KNPH+ejxmF/7K9h+8kaddSweJywm228Vex4Ziza4k9Tm8heZWcpw8De/mADfIBZP
# J/tgZxahZrrdVcA6KYawmKAr7ZVBtzrVFZgxtGIJDwq9gdkT/r+k0fNX2bwE+oLe
# Mt8EifAAzV3C+dAjfwAL5HYCJtnwZXZCpimHCUcr5n8apIUP/JiW9lVUKx+A+sDy
# Divl1vupL0QVSucTDh3bNzgaoSv27dZ8/DCCBY0wggR1oAMCAQICEA6bGI750C3n
# 79tQ4ghAGFowDQYJKoZIhvcNAQEMBQAwZTELMAkGA1UEBhMCVVMxFTATBgNVBAoT
# DERpZ2lDZXJ0IEluYzEZMBcGA1UECxMQd3d3LmRpZ2ljZXJ0LmNvbTEkMCIGA1UE
# AxMbRGlnaUNlcnQgQXNzdXJlZCBJRCBSb290IENBMB4XDTIyMDgwMTAwMDAwMFoX
# DTMxMTEwOTIzNTk1OVowYjELMAkGA1UEBhMCVVMxFTATBgNVBAoTDERpZ2lDZXJ0
# IEluYzEZMBcGA1UECxMQd3d3LmRpZ2ljZXJ0LmNvbTEhMB8GA1UEAxMYRGlnaUNl
# cnQgVHJ1c3RlZCBSb290IEc0MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKC
# AgEAv+aQc2jeu+RdSjwwIjBpM+zCpyUuySE98orYWcLhKac9WKt2ms2uexuEDcQw
# H/MbpDgW61bGl20dq7J58soR0uRf1gU8Ug9SH8aeFaV+vp+pVxZZVXKvaJNwwrK6
# dZlqczKU0RBEEC7fgvMHhOZ0O21x4i0MG+4g1ckgHWMpLc7sXk7Ik/ghYZs06wXG
# XuxbGrzryc/NrDRAX7F6Zu53yEioZldXn1RYjgwrt0+nMNlW7sp7XeOtyU9e5TXn
# Mcvak17cjo+A2raRmECQecN4x7axxLVqGDgDEI3Y1DekLgV9iPWCPhCRcKtVgkEy
# 19sEcypukQF8IUzUvK4bA3VdeGbZOjFEmjNAvwjXWkmkwuapoGfdpCe8oU85tRFY
# F/ckXEaPZPfBaYh2mHY9WV1CdoeJl2l6SPDgohIbZpp0yt5LHucOY67m1O+Skjqe
# PdwA5EUlibaaRBkrfsCUtNJhbesz2cXfSwQAzH0clcOP9yGyshG3u3/y1YxwLEFg
# qrFjGESVGnZifvaAsPvoZKYz0YkH4b235kOkGLimdwHhD5QMIR2yVCkliWzlDlJR
# R3S+Jqy2QXXeeqxfjT/JvNNBERJb5RBQ6zHFynIWIgnffEx1P2PsIV/EIFFrb7Gr
# hotPwtZFX50g/KEexcCPorF+CiaZ9eRpL5gdLfXZqbId5RsCAwEAAaOCATowggE2
# MA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFOzX44LScV1kTN8uZz/nupiuHA9P
# MB8GA1UdIwQYMBaAFEXroq/0ksuCMS1Ri6enIZ3zbcgPMA4GA1UdDwEB/wQEAwIB
# hjB5BggrBgEFBQcBAQRtMGswJAYIKwYBBQUHMAGGGGh0dHA6Ly9vY3NwLmRpZ2lj
# ZXJ0LmNvbTBDBggrBgEFBQcwAoY3aHR0cDovL2NhY2VydHMuZGlnaWNlcnQuY29t
# L0RpZ2lDZXJ0QXNzdXJlZElEUm9vdENBLmNydDBFBgNVHR8EPjA8MDqgOKA2hjRo
# dHRwOi8vY3JsMy5kaWdpY2VydC5jb20vRGlnaUNlcnRBc3N1cmVkSURSb290Q0Eu
# Y3JsMBEGA1UdIAQKMAgwBgYEVR0gADANBgkqhkiG9w0BAQwFAAOCAQEAcKC/Q1xV
# 5zhfoKN0Gz22Ftf3v1cHvZqsoYcs7IVeqRq7IviHGmlUIu2kiHdtvRoU9BNKei8t
# tzjv9P+Aufih9/Jy3iS8UgPITtAq3votVs/59PesMHqai7Je1M/RQ0SbQyHrlnKh
# SLSZy51PpwYDE3cnRNTnf+hZqPC/Lwum6fI0POz3A8eHqNJMQBk1RmppVLC4oVaO
# 7KTVPeix3P0c2PR3WlxUjG/voVA9/HYJaISfb8rbII01YBwCA8sgsKxYoA5AY8WY
# IsGyWfVVa88nq2x2zm8jLfR+cWojayL/ErhULSd+2DrZ8LaHlv1b0VysGMNNn3O3
# AamfV6peKOK5lDGCA3YwggNyAgEBMHcwYzELMAkGA1UEBhMCVVMxFzAVBgNVBAoT
# DkRpZ2lDZXJ0LCBJbmMuMTswOQYDVQQDEzJEaWdpQ2VydCBUcnVzdGVkIEc0IFJT
# QTQwOTYgU0hBMjU2IFRpbWVTdGFtcGluZyBDQQIQC65mvFq6f5WHxvnpBOMzBDAN
# BglghkgBZQMEAgEFAKCB0TAaBgkqhkiG9w0BCQMxDQYLKoZIhvcNAQkQAQQwHAYJ
# KoZIhvcNAQkFMQ8XDTI1MDExMjE4MzIxMFowKwYLKoZIhvcNAQkQAgwxHDAaMBgw
# FgQU29OF7mLb0j575PZxSFCHJNWGW0UwLwYJKoZIhvcNAQkEMSIEIEDkprP0DNzz
# QUpUJtua+DJz4ye1Iy4OBVe/28zZEFLcMDcGCyqGSIb3DQEJEAIvMSgwJjAkMCIE
# IHZ2n6jyYy8fQws6IzCu1lZ1/tdz2wXWZbkFk5hDj5rbMA0GCSqGSIb3DQEBAQUA
# BIICAAwDdDG3r2WvnjrrLdcYtG70iogWZOqsqZKGXg4007WNWnbeIbGD/LrmkM1r
# G5hbNz2SI6MBNsz7NCsg3eeNMZ0l9UtEIDVmsecdCSidRyl6s7+FyhDmt5lwj3xD
# 16dwwsNFR87EnRGJZe0ZJRNzJwRgF/9om4HAV1iOOGvBGTzIq1BLPBHb+yFyRxgV
# TDyRNMTjkxw8lRlY7/2hAtwgje7QX9d+we86muH5ra4I10x/r8GpmOF6DnwU1Rh+
# u9nWjCtg8anr1J6kbojDsXdVwyeCC+N3StZfgPnA9K+4e1NyH/D9eaTwBmLEnqxt
# yY5U4Lv8kDGWfYC0ZIHwRqREqrwPDbIP+SI6DJvd364cibkpITPxO/gow6cJQuSN
# RYSkTFk7gU2NYmovPW2XMYSCsmGtwm+pg+2aRr0+Qhsqin9MioovUyJyygoVIKRc
# dx764MAeJJm9MprY7/4TQIijSkpHa8+tKqhpE8puQK68mZkhmMX+ghszPVcmyxyD
# icJBhjI9pj+5FLKZbX5ECvTN4SMMdOkuaNJR1KU0UgZG2RnNCHKYy3pHsx9ZJLab
# bZuQp3bah1rbUTJhEcfLEAfNYjlh6hekmCo4J2AUdCvUCGOIHjm0SqRZnGCgfOM+
# NkCMT9Y9mURzJmeBwTALX3MUcNrBHxtsBh22GiLy2ocGG+12
# SIG # End signature block
