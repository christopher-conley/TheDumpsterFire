function Send-Toast
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [string] $AppID,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [AllowNull()]
        [string] $Title,

        [Parameter(Mandatory = $false, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
        [AllowNull()]
        [string] $MessageContent,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [AllowNull()]
        [string] $ActionButtonLabel,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [AllowNull()]
        [string] $ActionButtonActivity,

        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)]
        [AllowNull()]
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


























# SIG # Begin signature block
# MIIonwYJKoZIhvcNAQcCoIIokDCCKIwCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCAl72QNborHl5jc
# TYi2mAUfuz2eDLH9Yv6kiD7P8moq+KCCDaUwgga5MIIEoaADAgECAhEAmaOACiZV
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
# EIuGjXJneFe9xX3eJDNfMYIaUDCCGkwCAQEwajBWMQswCQYDVQQGEwJQTDEhMB8G
# A1UEChMYQXNzZWNvIERhdGEgU3lzdGVtcyBTLkEuMSQwIgYDVQQDExtDZXJ0dW0g
# Q29kZSBTaWduaW5nIDIwMjEgQ0ECEDtTlpcWV2y1yyFbCDsgwpowDQYJYIZIAWUD
# BAIBBQCgfDAQBgorBgEEAYI3AgEMMQIwADAZBgkqhkiG9w0BCQMxDAYKKwYBBAGC
# NwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAvBgkqhkiG9w0BCQQx
# IgQg9wkL9c5rJ1+n7UTjCrrRKyGAnl7eh3fWngFPVGSXvDUwDQYJKoZIhvcNAQEB
# BQAEggIAIEzs+uBCnntdxLdu52Hz4XldfvvTe+bHuriFHObdS+XMGbNz/FR7ffX/
# ywB72EY45Wdmz3E/NZsWV7OjRQkAx/YsWnIx25J2fDHBqdJzQDRwU/hk7ZF4Xaru
# M4s08M+Oy545iGG4+eLjDcOwGfpeJCS3DmrSnrxWQnMCOg6OYFMGxUL9kL5uL0Nv
# sO5rK+oMmvvIIDtRJUA1slLehvP3ZaKCTnMp4UEZ9j8qcv1lNK3Ck1maKM2Cdm1L
# LaKvNCuOTZFdL7dg+X2ruAPK8j2gC61MU9YPo7CpeF8l+o7O+gkPtQUhIkT96gZj
# TyqOpcghrS65oM+g5XhJAkzQPQsnoD3UlITEcnMpnerbcScIradWxfNlfTa12QuA
# ldUu5inJpW9cxtLAiwejTGSiseeuA8WZ8UKNBVQZnPagowJ9d4Vo0NQFu76PGePq
# lMpScupEQIpfGrcJAtoMsuMKwsMyBwNJie68OUKGdrnHvUiatftDPCCpnCc+yycs
# iSOvyJfEFahRSbYJwQn/By4GfD0S6DVpOg86dxToQ1sEhInw5mjvmkOihUf8+xgZ
# diuf7EOPjbq0ot/MZQAVSIqTv/qek6fbNOLgTESlvpQMVSwWruFr6nzD4SrTmqm/
# y5/G2+ikNTVvvZKw3U7prQfXOixRxqN5Qv30GqX4p0VTpU9kR2uhghc5MIIXNQYK
# KwYBBAGCNwMDATGCFyUwghchBgkqhkiG9w0BBwKgghcSMIIXDgIBAzEPMA0GCWCG
# SAFlAwQCAQUAMHcGCyqGSIb3DQEJEAEEoGgEZjBkAgEBBglghkgBhv1sBwEwMTAN
# BglghkgBZQMEAgEFAAQgmNS7/6jnzITID00zqNoxZhXK/dcBwcA2mYF0RnNYEicC
# EHGBe6jItPpO3ZkMk7Rg+UMYDzIwMjUwMTEyMTgzNTA1WqCCEwMwgga8MIIEpKAD
# AgECAhALrma8Wrp/lYfG+ekE4zMEMA0GCSqGSIb3DQEBCwUAMGMxCzAJBgNVBAYT
# AlVTMRcwFQYDVQQKEw5EaWdpQ2VydCwgSW5jLjE7MDkGA1UEAxMyRGlnaUNlcnQg
# VHJ1c3RlZCBHNCBSU0E0MDk2IFNIQTI1NiBUaW1lU3RhbXBpbmcgQ0EwHhcNMjQw
# OTI2MDAwMDAwWhcNMzUxMTI1MjM1OTU5WjBCMQswCQYDVQQGEwJVUzERMA8GA1UE
# ChMIRGlnaUNlcnQxIDAeBgNVBAMTF0RpZ2lDZXJ0IFRpbWVzdGFtcCAyMDI0MIIC
# IjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAvmpzn/aVIauWMLpbbeZZo7Xo
# /ZEfGMSIO2qZ46XB/QowIEMSvgjEdEZ3v4vrrTHleW1JWGErrjOL0J4L0HqVR1cz
# SzvUQ5xF7z4IQmn7dHY7yijvoQ7ujm0u6yXF2v1CrzZopykD07/9fpAT4BxpT9vJ
# oJqAsP8YuhRvflJ9YeHjes4fduksTHulntq9WelRWY++TFPxzZrbILRYynyEy7rS
# 1lHQKFpXvo2GePfsMRhNf1F41nyEg5h7iOXv+vjX0K8RhUisfqw3TTLHj1uhS66Y
# X2LZPxS4oaf33rp9HlfqSBePejlYeEdU740GKQM7SaVSH3TbBL8R6HwX9QVpGnXP
# lKdE4fBIn5BBFnV+KwPxRNUNK6lYk2y1WSKour4hJN0SMkoaNV8hyyADiX1xuTxK
# aXN12HgR+8WulU2d6zhzXomJ2PleI9V2yfmfXSPGYanGgxzqI+ShoOGLomMd3mJt
# 92nm7Mheng/TBeSA2z4I78JpwGpTRHiT7yHqBiV2ngUIyCtd0pZ8zg3S7bk4QC4R
# rcnKJ3FbjyPAGogmoiZ33c1HG93Vp6lJ415ERcC7bFQMRbxqrMVANiav1k425zYy
# FMyLNyE1QulQSgDpW9rtvVcIH7WvG9sqYup9j8z9J1XqbBZPJ5XLln8mS8wWmdDL
# nBHXgYly/p1DhoQo5fkCAwEAAaOCAYswggGHMA4GA1UdDwEB/wQEAwIHgDAMBgNV
# HRMBAf8EAjAAMBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMIMCAGA1UdIAQZMBcwCAYG
# Z4EMAQQCMAsGCWCGSAGG/WwHATAfBgNVHSMEGDAWgBS6FtltTYUvcyl2mi91jGog
# j57IbzAdBgNVHQ4EFgQUn1csA3cOKBWQZqVjXu5Pkh92oFswWgYDVR0fBFMwUTBP
# oE2gS4ZJaHR0cDovL2NybDMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0VHJ1c3RlZEc0
# UlNBNDA5NlNIQTI1NlRpbWVTdGFtcGluZ0NBLmNybDCBkAYIKwYBBQUHAQEEgYMw
# gYAwJAYIKwYBBQUHMAGGGGh0dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBYBggrBgEF
# BQcwAoZMaHR0cDovL2NhY2VydHMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0VHJ1c3Rl
# ZEc0UlNBNDA5NlNIQTI1NlRpbWVTdGFtcGluZ0NBLmNydDANBgkqhkiG9w0BAQsF
# AAOCAgEAPa0eH3aZW+M4hBJH2UOR9hHbm04IHdEoT8/T3HuBSyZeq3jSi5GXeWP7
# xCKhVireKCnCs+8GZl2uVYFvQe+pPTScVJeCZSsMo1JCoZN2mMew/L4tpqVNbSpW
# O9QGFwfMEy60HofN6V51sMLMXNTLfhVqs+e8haupWiArSozyAmGH/6oMQAh078qR
# h6wvJNU6gnh5OruCP1QUAvVSu4kqVOcJVozZR5RRb/zPd++PGE3qF1P3xWvYViUJ
# Lsxtvge/mzA75oBfFZSbdakHJe2BVDGIGVNVjOp8sNt70+kEoMF+T6tptMUNlehS
# R7vM+C13v9+9ZOUKzfRUAYSyyEmYtsnpltD/GWX8eM70ls1V6QG/ZOB6b6Yum1Hv
# IiulqJ1Elesj5TMHq8CWT/xrW7twipXTJ5/i5pkU5E16RSBAdOp12aw8IQhhA/vE
# bFkEiF2abhuFixUDobZaA0VhqAsMHOmaT3XThZDNi5U2zHKhUs5uHHdG6BoQau75
# KiNbh0c+hatSF+02kULkftARjsyEpHKsF7u5zKRbt5oK5YGwFvgc4pEVUNytmB3B
# pIiowOIIuDgP5M9WArHYSAR16gc0dP2XdkMEP5eBsX7bf/MGN4K3HP50v/01ZHo/
# Z5lGLvNwQ7XHBx1yomzLP8lx4Q1zZKDyHcp4VQJLu2kWTsKsOqQwggauMIIElqAD
# AgECAhAHNje3JFR82Ees/ShmKl5bMA0GCSqGSIb3DQEBCwUAMGIxCzAJBgNVBAYT
# AlVTMRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2Vy
# dC5jb20xITAfBgNVBAMTGERpZ2lDZXJ0IFRydXN0ZWQgUm9vdCBHNDAeFw0yMjAz
# MjMwMDAwMDBaFw0zNzAzMjIyMzU5NTlaMGMxCzAJBgNVBAYTAlVTMRcwFQYDVQQK
# Ew5EaWdpQ2VydCwgSW5jLjE7MDkGA1UEAxMyRGlnaUNlcnQgVHJ1c3RlZCBHNCBS
# U0E0MDk2IFNIQTI1NiBUaW1lU3RhbXBpbmcgQ0EwggIiMA0GCSqGSIb3DQEBAQUA
# A4ICDwAwggIKAoICAQDGhjUGSbPBPXJJUVXHJQPE8pE3qZdRodbSg9GeTKJtoLDM
# g/la9hGhRBVCX6SI82j6ffOciQt/nR+eDzMfUBMLJnOWbfhXqAJ9/UO0hNoR8XOx
# s+4rgISKIhjf69o9xBd/qxkrPkLcZ47qUT3w1lbU5ygt69OxtXXnHwZljZQp09ns
# ad/ZkIdGAHvbREGJ3HxqV3rwN3mfXazL6IRktFLydkf3YYMZ3V+0VAshaG43IbtA
# rF+y3kp9zvU5EmfvDqVjbOSmxR3NNg1c1eYbqMFkdECnwHLFuk4fsbVYTXn+149z
# k6wsOeKlSNbwsDETqVcplicu9Yemj052FVUmcJgmf6AaRyBD40NjgHt1biclkJg6
# OBGz9vae5jtb7IHeIhTZgirHkr+g3uM+onP65x9abJTyUpURK1h0QCirc0PO30qh
# HGs4xSnzyqqWc0Jon7ZGs506o9UD4L/wojzKQtwYSH8UNM/STKvvmz3+DrhkKvp1
# KCRB7UK/BZxmSVJQ9FHzNklNiyDSLFc1eSuo80VgvCONWPfcYd6T/jnA+bIwpUzX
# 6ZhKWD7TA4j+s4/TXkt2ElGTyYwMO1uKIqjBJgj5FBASA31fI7tk42PgpuE+9sJ0
# sj8eCXbsq11GdeJgo1gJASgADoRU7s7pXcheMBK9Rp6103a50g5rmQzSM7TNsQID
# AQABo4IBXTCCAVkwEgYDVR0TAQH/BAgwBgEB/wIBADAdBgNVHQ4EFgQUuhbZbU2F
# L3MpdpovdYxqII+eyG8wHwYDVR0jBBgwFoAU7NfjgtJxXWRM3y5nP+e6mK4cD08w
# DgYDVR0PAQH/BAQDAgGGMBMGA1UdJQQMMAoGCCsGAQUFBwMIMHcGCCsGAQUFBwEB
# BGswaTAkBggrBgEFBQcwAYYYaHR0cDovL29jc3AuZGlnaWNlcnQuY29tMEEGCCsG
# AQUFBzAChjVodHRwOi8vY2FjZXJ0cy5kaWdpY2VydC5jb20vRGlnaUNlcnRUcnVz
# dGVkUm9vdEc0LmNydDBDBgNVHR8EPDA6MDigNqA0hjJodHRwOi8vY3JsMy5kaWdp
# Y2VydC5jb20vRGlnaUNlcnRUcnVzdGVkUm9vdEc0LmNybDAgBgNVHSAEGTAXMAgG
# BmeBDAEEAjALBglghkgBhv1sBwEwDQYJKoZIhvcNAQELBQADggIBAH1ZjsCTtm+Y
# qUQiAX5m1tghQuGwGC4QTRPPMFPOvxj7x1Bd4ksp+3CKDaopafxpwc8dB+k+YMjY
# C+VcW9dth/qEICU0MWfNthKWb8RQTGIdDAiCqBa9qVbPFXONASIlzpVpP0d3+3J0
# FNf/q0+KLHqrhc1DX+1gtqpPkWaeLJ7giqzl/Yy8ZCaHbJK9nXzQcAp876i8dU+6
# WvepELJd6f8oVInw1YpxdmXazPByoyP6wCeCRK6ZJxurJB4mwbfeKuv2nrF5mYGj
# VoarCkXJ38SNoOeY+/umnXKvxMfBwWpx2cYTgAnEtp/Nh4cku0+jSbl3ZpHxcpzp
# SwJSpzd+k1OsOx0ISQ+UzTl63f8lY5knLD0/a6fxZsNBzU+2QJshIUDQtxMkzdwd
# eDrknq3lNHGS1yZr5Dhzq6YBT70/O3itTK37xJV77QpfMzmHQXh6OOmc4d0j/R0o
# 08f56PGYX/sr2H7yRp11LB4nLCbbbxV7HhmLNriT1ObyF5lZynDwN7+YAN8gFk8n
# +2BnFqFmut1VwDophrCYoCvtlUG3OtUVmDG0YgkPCr2B2RP+v6TR81fZvAT6gt4y
# 3wSJ8ADNXcL50CN/AAvkdgIm2fBldkKmKYcJRyvmfxqkhQ/8mJb2VVQrH4D6wPIO
# K+XW+6kvRBVK5xMOHds3OBqhK/bt1nz8MIIFjTCCBHWgAwIBAgIQDpsYjvnQLefv
# 21DiCEAYWjANBgkqhkiG9w0BAQwFADBlMQswCQYDVQQGEwJVUzEVMBMGA1UEChMM
# RGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMSQwIgYDVQQD
# ExtEaWdpQ2VydCBBc3N1cmVkIElEIFJvb3QgQ0EwHhcNMjIwODAxMDAwMDAwWhcN
# MzExMTA5MjM1OTU5WjBiMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQg
# SW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMSEwHwYDVQQDExhEaWdpQ2Vy
# dCBUcnVzdGVkIFJvb3QgRzQwggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoIC
# AQC/5pBzaN675F1KPDAiMGkz7MKnJS7JIT3yithZwuEppz1Yq3aaza57G4QNxDAf
# 8xukOBbrVsaXbR2rsnnyyhHS5F/WBTxSD1Ifxp4VpX6+n6lXFllVcq9ok3DCsrp1
# mWpzMpTREEQQLt+C8weE5nQ7bXHiLQwb7iDVySAdYyktzuxeTsiT+CFhmzTrBcZe
# 7FsavOvJz82sNEBfsXpm7nfISKhmV1efVFiODCu3T6cw2Vbuyntd463JT17lNecx
# y9qTXtyOj4DatpGYQJB5w3jHtrHEtWoYOAMQjdjUN6QuBX2I9YI+EJFwq1WCQTLX
# 2wRzKm6RAXwhTNS8rhsDdV14Ztk6MUSaM0C/CNdaSaTC5qmgZ92kJ7yhTzm1EVgX
# 9yRcRo9k98FpiHaYdj1ZXUJ2h4mXaXpI8OCiEhtmmnTK3kse5w5jrubU75KSOp49
# 3ADkRSWJtppEGSt+wJS00mFt6zPZxd9LBADMfRyVw4/3IbKyEbe7f/LVjHAsQWCq
# sWMYRJUadmJ+9oCw++hkpjPRiQfhvbfmQ6QYuKZ3AeEPlAwhHbJUKSWJbOUOUlFH
# dL4mrLZBdd56rF+NP8m800ERElvlEFDrMcXKchYiCd98THU/Y+whX8QgUWtvsauG
# i0/C1kVfnSD8oR7FwI+isX4KJpn15GkvmB0t9dmpsh3lGwIDAQABo4IBOjCCATYw
# DwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQU7NfjgtJxXWRM3y5nP+e6mK4cD08w
# HwYDVR0jBBgwFoAUReuir/SSy4IxLVGLp6chnfNtyA8wDgYDVR0PAQH/BAQDAgGG
# MHkGCCsGAQUFBwEBBG0wazAkBggrBgEFBQcwAYYYaHR0cDovL29jc3AuZGlnaWNl
# cnQuY29tMEMGCCsGAQUFBzAChjdodHRwOi8vY2FjZXJ0cy5kaWdpY2VydC5jb20v
# RGlnaUNlcnRBc3N1cmVkSURSb290Q0EuY3J0MEUGA1UdHwQ+MDwwOqA4oDaGNGh0
# dHA6Ly9jcmwzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydEFzc3VyZWRJRFJvb3RDQS5j
# cmwwEQYDVR0gBAowCDAGBgRVHSAAMA0GCSqGSIb3DQEBDAUAA4IBAQBwoL9DXFXn
# OF+go3QbPbYW1/e/Vwe9mqyhhyzshV6pGrsi+IcaaVQi7aSId229GhT0E0p6Ly23
# OO/0/4C5+KH38nLeJLxSA8hO0Cre+i1Wz/n096wwepqLsl7Uz9FDRJtDIeuWcqFI
# tJnLnU+nBgMTdydE1Od/6Fmo8L8vC6bp8jQ87PcDx4eo0kxAGTVGamlUsLihVo7s
# pNU96LHc/RzY9HdaXFSMb++hUD38dglohJ9vytsgjTVgHAIDyyCwrFigDkBjxZgi
# wbJZ9VVrzyerbHbObyMt9H5xaiNrIv8SuFQtJ37YOtnwtoeW/VvRXKwYw02fc7cB
# qZ9Xql4o4rmUMYIDdjCCA3ICAQEwdzBjMQswCQYDVQQGEwJVUzEXMBUGA1UEChMO
# RGlnaUNlcnQsIEluYy4xOzA5BgNVBAMTMkRpZ2lDZXJ0IFRydXN0ZWQgRzQgUlNB
# NDA5NiBTSEEyNTYgVGltZVN0YW1waW5nIENBAhALrma8Wrp/lYfG+ekE4zMEMA0G
# CWCGSAFlAwQCAQUAoIHRMBoGCSqGSIb3DQEJAzENBgsqhkiG9w0BCRABBDAcBgkq
# hkiG9w0BCQUxDxcNMjUwMTEyMTgzNTA1WjArBgsqhkiG9w0BCRACDDEcMBowGDAW
# BBTb04XuYtvSPnvk9nFIUIck1YZbRTAvBgkqhkiG9w0BCQQxIgQgwXOM8piaR7cx
# axPcev3QU05zSZUGCbOdsW+XpBZkQzwwNwYLKoZIhvcNAQkQAi8xKDAmMCQwIgQg
# dnafqPJjLx9DCzojMK7WVnX+13PbBdZluQWTmEOPmtswDQYJKoZIhvcNAQEBBQAE
# ggIAAUfvjGDOqmYIXEUs83RCpspV3K4gM2KAt1LSUHRF9izK+JlPr2dITQDbujtV
# CeQDIeymN2n/At2bEbhAxsAydpKoARlCOdJLLpSvUfKsXzR3fPPk8rcjbk+xzD2W
# 6BqwzB+LDwYK+TrtNWrl3XiJc8YdClXwamk7hlpM5Lojeh5aBlv62jeJ9COp5IES
# 9n9slNlURTrhC2yV1EN32CSl635GPdpw02YSs6iqfMdqCxc4Le1Fh3JWmyPY2rkW
# btNhaj4Uxyenv/eRnvTEdpbPvP8L2SV23t9EGt+hP5kkhVpEvk5oEOrL2yIo+Y1o
# K1WMgNKuN/jC+s9Q96/ton/2tAdM09OtIoFROcHR+BzQg5uqpLTi43FPBpMvVCYq
# WwtrxhBoBwj/ZadVctNFKeVZy/SZUq4CzwJYK/dmFIvDHtg+yK7LmbrFckj0hcuk
# js+ugbOYUoyxK/YUk38mOExoZEutb/6qpDbURz/UjXWkWgNEZW7cAJKOiVtWRLqd
# 5wdMFS84AKCV0FoXCEXa1NGxmZcxMMnDLowlcsJnnNDV0WimJoTtuNOcVWIZZTF0
# Oc5fH1hNrDunX8OSPOoZz4RUnHLBQsKDsoQiZv8xl2qCsl6jN/2vug01IoluBX1D
# qWz03wWUQr2oYAYGT9FMbl6P8V/Lm7nIs+uTtEHJ9yDieFA=
# SIG # End signature block
