Param(
    [Parameter(Mandatory=$true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true, Position = 1)]
    [string] $PFXFilePath,

    [Parameter(Mandatory=$false, ValueFromPipelineByPropertyName = $true)]
    [securestring]
    $PFXPassword = (Read-host -AsSecureString -Prompt "PFX password")
)


[string] $SNKFilePath = [System.IO.Path]::GetFileNameWithoutExtension($PFXFilePath) + ".snk"

[byte[]] $PFXBytes = [System.IO.File]::ReadAllBytes($PFXFilePath)

# Get a cert object from the pfx bytes with the private key marked as exportable
$Cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($PFXBytes, $PFXPassword, [Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
#$Cert.Import($PFXBytes, $PFXPassword, [Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)

# Export a CSP blob from the cert (which is the same format as an SNK file)
[byte[]] $SNKBytes = ([Security.Cryptography.RSACryptoServiceProvider]$Cert.PrivateKey).ExportCspBlob($true)

# Write the CSP blob/SNK bytes to the snk file
[System.IO.File]::WriteAllBytes($SNKFilePath, $SNKBytes)

