#Requires -RunAsAdministrator

# "High definition audio device"
# {4d36e96c-e325-11ce-bfc1-08002be10318}

# "NVIDIA high definition audio device"
# {4d36e96c-e325-11ce-bfc1-08002be10318}

# "NVIDIA Virtual Audio Device (Wave Extensible) (WDM)"
# {4d36e96c-e325-11ce-bfc1-08002be10318}

# "Microsoft Bluetooth Hands-Free Audio device" (Bragi Hands-Free AG Audio)
# {4d36e96c-e325-11ce-bfc1-08002be10318}

# "Microsoft Bluetooth A2dp Source" (Bragi Stereo)
# {4d36e96c-e325-11ce-bfc1-08002be10318}

# "Steam Streaming Speakers"
# {4d36e96c-e325-11ce-bfc1-08002be10318}

# "USB Audio Device"
# {4d36e96c-e325-11ce-bfc1-08002be10318}

# "Virtual Desktop Audio"
# {4d36e96c-e325-11ce-bfc1-08002be10318}

$GUIDs = @(
    "{4d36e96c-e325-11ce-bfc1-08002be10318}",
    "{4d36e96c-e325-11ce-bfc1-08002be10318}",
    "{4d36e96c-e325-11ce-bfc1-08002be10318}",
    "{4d36e96c-e325-11ce-bfc1-08002be10318}",
    "{4d36e96c-e325-11ce-bfc1-08002be10318}",
    "{4d36e96c-e325-11ce-bfc1-08002be10318}",
    "{4d36e96c-e325-11ce-bfc1-08002be10318}",
    "{4d36e96c-e325-11ce-bfc1-08002be10318}"
)

$RegKeys = @(
    "ConservationIdleTime",
    "IdlePowerState",
    "PerformanceIdleTime"
)

$RegPath = "HKLM:\SYSTEM\CurrentControlSet\Control\Class"
$RegNumber = "0000"

foreach ($GUID in $GUIDs)
{
    foreach ($RegKey in $RegKeys)
    {
        $RegPathFull = "$RegPath\$GUID\$RegNumber\PowerSettings"
        $RegKeyFull = "$RegPathFull\$RegKey"
        $RegKeyFull
        $CurrentSetting = Get-ItemProperty -Name $RegKey -Path $RegPathFull
        if ($CurrentSetting.$RegKey -ne 0)
        {
            Write-Information -InformationAction 'Continue' -MessageData "Value for:`n$RegKeyFull`n is not zero, setting to zero."
            Set-ItemProperty -Type Binary -Value ([byte[]](0x0, 0x0, 0x0, 0x0)) -Name $RegKey -Path $RegPathFull
        }
    }
}


#Get-ItemProperty -Name ConservationIdleTime -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}\0000\PowerSettings'
#Set-ItemProperty -Type Binary -Value ([byte[]](0x0,0x0,0x0,0x0)) -Name ConservationIdleTime -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}\0000\PowerSettings'

