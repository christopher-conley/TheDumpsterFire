#!/usr/bin/env pwsh

using module '../modules/EasyANSI/EasyANSI.psm1';

[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'Low')]
[OutputType([PSCustomObject])]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "CertificatePassword")]
param (

    [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
    [ValidateLength(1, 64)]
    [ValidateScript( {
            $_ -is [string]
        }, ErrorMessage = "Certificate name must be a string between 1 and 64 characters in length.")]
    [Alias("Name", "Subject")]
    [string] $CertificateName,


    [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
    [ValidateScript( {
            if (
            ($null -eq $_) -or
            ([System.String]::IsNullOrWhiteSpace($_)) -or
            ((($_ -isnot [string]) -and
            ($_ -isnot [SecureString])
            ))
            )
            {
                return $false
            }
            else
            {
                return $true
            }
        }, ErrorMessage = "Certificate password must be a string, must not be null, or must be a System.Security.SecureString Type.")]
    [Alias("Password")]
    [object] $CertificatePassword,


    [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
    [ValidateScript( {
            switch ($_)
            {
                { $_ -is [string] }
                {
                    $InvalidPathChars = [System.IO.Path]::GetInvalidPathChars()
                    $PathArray = [char[]] $_
                    foreach ($InvalidPathChar in $InvalidPathChars)
                    {
                        if ($PathArray -contains $InvalidPathChar)
                        {
                            Write-Information -MessageData (Get-ANSIString -InputObject "`n[red]ERROR:[/] [bylw]-SavePath[/] contains [bblk]invalid path characters.") -InformationAction 'Continue'
                            Write-Information -MessageData (Get-ANSIString -InputObject "Invalid path character: [ylw][ul]${InvalidPathChar}[rst]`n") -InformationAction 'Continue'
                            return $false
                        }
                    }

                    if ([System.String]::IsNullOrWhiteSpace($_))
                    {
                        Write-Information -MessageData (Get-ANSIString -InputObject "`n[red]ERROR:[/]-SavePath cannot be null, empty, or whitespace.") -InformationAction 'Continue'
                        return $false
                    }
                    else
                    {
                        return $true
                    }
                }

                { $_ -is [System.IO.DirectoryInfo] -and !([System.String]::IsNullOrWhiteSpace($_.FullName)) }
                {
                    return $true
                }
                else
                {
                    return $false
                    Write-Information -MessageData (Get-ANSIString -InputObject "`n[red]ERROR:[/] The path specified in the `"FullName`" property of the provided DirectoryInfo object does not exist or is inaccessible. Please provide a valid path.") -InformationAction 'Continue'
                }
            
                default
                {
                    return $false
                    Write-Information -MessageData (Get-ANSIString -InputObject "[bylw]-SavePath[/] must be a string or a DirectoryInfo object.") -InformationAction 'Continue'
                }
            }
        }, ErrorMessage = ' ')]
    [Alias("SaveDir", "SaveDirectory", "OutputPath")]
    [object] $SavePath,

    [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
    [ValidateSet("SHA256", "SHA384", "SHA512", IgnoreCase = $true)]
    [Alias("Hash", "HashName")]
    [string] $HashAlgorithm

)
begin
{
    [hashtable] $InfoHash = [ordered]@{
        CertificateName          = $PSBoundParameters.CertificateName
        CertificatePassword      = $(
            if ($PSBoundParameters.CertificatePassword -is [string])
            {
                ConvertTo-SecureString -String $PSBoundParameters.CertificatePassword -Force -AsPlainText
                $PSBoundParameters.CertificatePassword
            }
            elseif ($PSBoundParameters.CertificatePassword -is [SecureString])
            {
                $PSBoundParameters.CertificatePassword
            }
            else
            {
                $PSBoundParameters.CertificatePassword = ' '
                while ([System.String]::IsNullOrWhiteSpace($PSBoundParameters.CertificatePassword)) {
                    $PSBoundParameters.CertificatePassword = Read-Host -MaskInput -Prompt "Enter a new password for the generated certificate" | ConvertTo-SecureString -AsPlainText -Force
                }
                $PSBoundParameters.CertificatePassword
            }
        )
        SavePath                 = $( 
            if ($PSBoundParameters.ContainsKey('SavePath'))
            {
                if ($SavePath -is [string])
                {
                    $SavePath
                }
                else
                {
                    $SavePath.FullName
                }
            }
            else
            {
                Join-Path -Path "$((Get-Location).Path)" -ChildPath "out"
            }
        )

        CertFilePrefix            = [System.String]::Empty
        CertSaveDir               = [System.String]::Empty
        HashCustomSavePath        = [bool] $PSBoundParameters.ContainsKey('SavePath')
        HasHashAlgorithm          = [bool] ($PSBoundParameters.ContainsKey('HashAlgorithm') ? $true : $false)
        HashAlgorithm             = [string] ($PSBoundParameters.ContainsKey('HashAlgorithm') ? $HashAlgorithm : "SHA512")
        InvalidPathChars          = [char[]] [System.IO.Path]::GetInvalidPathChars()
        FSAzureAuthCert           = $null                     # PowerShell object that can be converted to a raw byte array
        FSAzureAuthCertPFXBlob    = $null                     # Raw byte array of a PFX file exported from the .FSAzureAuthCert object
        FSAzureAuthCertPFXB64     = [System.String]::Empty    # Base64-encoded string of the PFX byte array at .FSAzureAuthCertPFXBlob
        FSAzureAuthCertPubKeyCer  = $null                     # Raw byte array
        FSAzureAuthCertPubKeyPem  = [System.String]::Empty    # String containing the PEM-encoded public key
        FSAzureAuthCertPrivKeyPem = [System.String]::Empty    # String containing the PEM-encoded private key

        # Any property above that will eventually end up as a raw byte array is not being preallocated here
        # because we don't know the size of the array yet, and PowerShell can dynamically allocate the
        # correct size at assignment time
    }

    [hashtable] $CertOptions = [ordered]@{
        Subject           = "CN=$($InfoHash.CertificateName)"
        CertStoreLocation = "Cert:\CurrentUser\My"
        KeyExportPolicy   = 'Exportable'
        KeySpec           = 'Signature'
        KeyLength         = 2048
        KeyAlgorithm      = 'RSA'
        HashAlgorithm     = "$($InfoHash.HashAlgorithm)"
    }

    try {
        Resolve-Path $InfoHash.SavePath -ErrorAction Stop | Out-Null
    }
    catch {
        try {
            $CreatedPath = New-Item -Path "$($InfoHash.SavePath)" -ItemType Directory -Force
            $InfoHash.SavePath = $CreatedPath.FullName
        }
        catch {
            [System.Management.Automation.ErrorRecord] $PathError = $_
            Write-Warning -Message "An error occurred while attempting to resolve the path at $($InfoHash.SavePath). The error message is: $($PathError.Message)"
            Write-Warning -Message "Falling back to the current directory instead. Cert files will be exported to: $(Join-Path -Path ((Get-Location).Path) -ChildPath 'out')"
        }
        try {
            $CreatedPath = New-Item -Path "$(Join-Path -Path ((Get-Location).Path) -ChildPath 'out')" -ItemType Directory -Force
            $InfoHash.SavePath = $CreatedPath.FullName
        }
        catch {
            [System.Management.Automation.ErrorRecord] $PathError = $_
            Write-Error -Message "An error occurred while attempting to create the directory `"out`" at $((Get-Location).Path). The error message is: $($PathError.Message)"
            Write-Error -Message "Cannot continue without a valid save path, please specify an absolute path with the -SavePath parameter. Bailing out."
            throw $PathError
        }
    }

}

process
{

    # $CertfileCer = [System.IO.MemoryMappedFiles.MemoryMappedFile]::CreateNew(
    #     $null, 
    #     1024, 
    #     [System.IO.MemoryMappedFiles.MemoryMappedFileAccess]::CopyOnWrite,
    #     [System.IO.MemoryMappedFiles.MemoryMappedFileOptions]::None,
    #     [System.IO.HandleInheritability]::Inheritable
    #     )

    #     $CertfileCer = [System.IO.MemoryMappedFiles.MemoryMappedFile]::CreateFromFile(
    #      "$($ExitingTmpFile)",
    #      [System.IO.MemoryMappedFiles.MemoryMappedFileAccess]::CopyOnWrite,
    #      [System.IO.MemoryMappedFiles.MemoryMappedFileOptions]::None,
    #      [System.IO.HandleInheritability]::Inheritable
    #      )

    try
    {
        [X509Certificate] $AzureAuthCert = New-SelfSignedCertificate @CertOptions
        $InfoHash.AzureAuthCert = $AzureAuthCert

    }
    catch
    {
        [System.Management.Automation.ErrorRecord] $GenerationError = $_
        Write-Error "An error occurred while attempting to generate the self-signed certificate. The error message is: $($GenerationError.Message)"
        Write-Error "Invocation info is: $($GenerationError.InvocationInfo)"
        throw $GenerationError
    }

    [string] $CertDir = $InfoHash.CertificateName
    [char[]] $CertDirArray = [char[]] $InfoHash.CertificateName

    foreach ($Character in $CertDirArray)
    {
        if ($Character -in $InvalidPathChar)
        {
            $CertDir = $CertDir.Replace($InvalidPathChar, '_')
            Write-Warning -Message (Get-ANSIString -InputObject "`n[bylw]Certificate Name[/] contains [bblk]invalid path characters.") -InformationAction 'Continue'
            Write-Warning -Message (Get-ANSIString -InputObject "Invalid path character: [ylw][ul]${InvalidPathChar}[rst] has been replaced with: [b][bylw]_`n") -InformationAction 'Continue'
            Write-Information -MessageData "Infohash object: " -InformationAction 'Continue'
        }
    }

    $InfoHash.CertFilePrefix = $CertDir
    $InfoHash.CertSaveDir = ([string] $CertSaveDir = (Join-Path -Path "$($InfoHash.SavePath)" -ChildPath "$CertDir"))

    if (!(Test-Path -Path "$CertSaveDir"))
    {
        try
        {
            New-Item -Path "$CertSaveDir" -ItemType Directory -Force
        }
        catch
        {
            [System.Management.Automation.ErrorRecord] $DirCreationError = $_
            Write-Warning -Message "An error occurred while attempting to create the directory at $CertSaveDir. The error message is: $($DirCreationError.Message) `nAttempting to save the certificate to the current directory ($((Get-Location).Path)) instead."
            $InfoHash.SavePath = $CertSaveDir = (Get-Location).Path

            try {
                New-Item -Path "$CertSaveDir" -ItemType Directory -Force
            }
            catch {
                [System.Management.Automation.ErrorRecord] $DirCreationError = $_
                Write-Error -Message "An error occurred while attempting to create the directory at $CertSaveDir. The error message is: $($DirCreationError.Message) `nBailing out."
                throw $DirCreationError   
            }
        }
    }
    else {
        try {
            New-Item -Path "$($InfoHash.CertSaveDir)" -ItemType Directory -Force
        }
        catch {
            [System.Management.Automation.ErrorRecord] $DirCreationError = $_
            Write-Error -Message "An error occurred while attempting to create the directory at $($InfoHash.CertSaveDir). The error message is: $($DirCreationError.Message) `nBailing out."
            throw $DirCreationError   
        }
    }

    try {
        # PFX byte array
        $InfoHash.FSAzureAuthCertPFXBlob = $InfoHash.AzureAuthCert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pkcs12, [securestring] $InfoHash.CertificatePassword)

        # The PFX byte array converted to a Base64 string, which is what will be stored in Vault.
        # This is also the format that PnP-PowerShell expects the cert to be in when used for auth.
        $InfoHash.FSAzureAuthCertPFXB64 = [Convert]::ToBase64String($InfoHash.FSAzureAuthCertPFXBlob)

        # DER-encoded binary
        $InfoHash.FSAzureAuthCertPubKeyCer = $InfoHash.AzureAuthCert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)

        # PEM public key
        $InfoHash.FSAzureAuthCertPubKeyPem = $InfoHash.AzureAuthCert.ExportCertificatePem()

        # PEM private key
        $InfoHash.FSAzureAuthCertPrivKeyPem = $InfoHash.AzureAuthCert.PrivateKey.ExportPkcs8PrivateKeyPem()
        
    }
    catch {
        [System.Management.Automation.ErrorRecord] $ExportError = $_
        Write-Error -Message "An error occurred while attempting to export the certificate. The error message is: $($ExportError.Message)"
        throw $ExportError
    }

    try {
        $PFXPath = Join-Path -Path "$($InfoHash.CertSaveDir)" -ChildPath "$($InfoHash.CertFilePrefix).pfx"
        $CERPath = Join-Path -Path "$($InfoHash.CertSaveDir)" -ChildPath "$($InfoHash.CertFilePrefix).cer"
        $PFXB64Path = Join-Path -Path "$($InfoHash.CertSaveDir)" -ChildPath "$($InfoHash.CertFilePrefix).b64.pfx.txt"
        $PubPEMPath = Join-Path -Path "$($InfoHash.CertSaveDir)" -ChildPath "$($InfoHash.CertFilePrefix).public.pem"
        $PrivPEMPath = Join-Path -Path "$($InfoHash.CertSaveDir)" -ChildPath "$($InfoHash.CertFilePrefix).private.pem"
        $ZipFilePath = Join-Path -Path "$($InfoHash.SavePath)" -ChildPath "$($InfoHash.CertFilePrefix).zip"

        [System.IO.File]::WriteAllBytes("$PFXPath", $InfoHash.FSAzureAuthCertPFXBlob)
        [System.IO.File]::WriteAllBytes("$CERPath", $InfoHash.FSAzureAuthCertPubKeyCer)
        $InfoHash.FSAzureAuthCertPFXB64.Trim() | Set-Content -Path "$PFXB64Path" -Encoding utf8NoBOM
        $InfoHash.FSAzureAuthCertPubKeyPem | Set-Content -Path "$PubPEMPath" -Encoding utf8NoBOM
        $InfoHash.FSAzureAuthCertPrivKeyPem | Set-Content -Path "$PrivPEMPath" -Encoding utf8NoBOM

        Compress-Archive -Path "$($InfoHash.CertSaveDir)" -DestinationPath "$ZipFilePath" -CompressionLevel 'Fastest'
    }
    catch {
        [System.Management.Automation.ErrorRecord] $WriteError = $_
        Write-Error -Message "An error occurred while attempting to write certificate files to disk. The error message is: $($WriteError.Message)"
        Write-Warning -Message "Please check the directory at $($InfoHash.CertSaveDir) for any certificate files that may have been written before the error occurred."
        throw $WriteError
    }

    try {
        Get-ChildItem -Path "Cert:\CurrentUser\My" | Where-Object { $_.Thumbprint -eq $InfoHash.AzureAuthCert.Thumbprint } | Remove-Item -Force -Confirm:$false
    }
    catch {
        Write-Warning -Message "An error occurred while attempting to remove the generated certificate from the certificate store. The error message is: $($_.Exception.Message)"
        Write-Warning -Message "Please remove the certificate manually from the certificate store at your convenience."
    }

    [string] $DoneMessage = Get-ANSIString -InputObject "[grn]SUCCESS:[/] Certificate generation complete. The certificate files have been saved to [bln][ylw]$ZipFilePath"
    Write-Information -MessageData "$DoneMessage" -InformationAction 'Continue'

}

    <#
https://learn.microsoft.com/en-us/entra/identity-platform/howto-create-self-signed-certificate

This article uses the New-SelfSignedCertificate PowerShell cmdlet to create the self-signed certificate and the Export-Certificate cmdlet to export it to a location that is easily accessible. These cmdlets are built-in to modern versions of Windows (Windows 8.1 and greater, and Windows Server 2012R2 and greater). The self-signed certificate will have the following configuration:

A 2048-bit key length. While longer values are supported, the 2048-bit size is highly recommended for the best combination of security and performance.
Uses the RSA cryptographic algorithm. Microsoft Entra ID currently supports only RSA.
The certificate is signed with the SHA256 hash algorithm. Microsoft Entra ID also supports certificates signed with SHA384 and SHA512 hash algorithms.
The certificate is valid for only one year.
The certificate is supported for use for both client and server authentication.

#>

end
{

}
