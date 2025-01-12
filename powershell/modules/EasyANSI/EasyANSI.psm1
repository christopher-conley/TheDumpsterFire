class EasyANSI {
    [hashtable] $PSANSICodes
    [System.Management.Automation.PSStyle] $ANSIMap
    [string] $Reset
    [bool] $Below74 = ([Version] $PSVersionTable.PSVersion.ToString() -lt [Version] "7.4")
    [bool] $NoANSISupport = ([Version] $PSVersionTable.PSVersion.ToString() -lt [Version] "7.2")
    [bool] $HasOperators = $false
    [string] $NoANSIChar
    [array] $MarkupOperators
    [array] $MarkupOperators74Plus
    hidden [string] $LastParsedMessage

    EasyANSI() {
        $this.Init()
    }

    EasyANSI([string] $InputString) {
        $this.Init()
        $this.LastParsedMessage = ($this._GetANSIString($InputString))
        Write-Output -InputObject "$($this.LastParsedMessage)"
    }

    hidden Init() {
        $this.ANSIMap = ([System.Management.Automation.PSStyle]::Instance)
        $this.Reset = $this.ANSIMap.Reset
        $this.NoANSIChar = [System.String]::Empty

#region ANSI Codes
        $this.PSANSICodes = [hashtable] @{
#region Regular foreground colors
            '[black]'      = $this.ANSIMap.Foreground.Black
            '[blk]'        = $this.ANSIMap.Foreground.Black
            '[blue]'       = $this.ANSIMap.Foreground.Blue
            '[blu]'        = $this.ANSIMap.Foreground.Blue
            '[cyan]'       = $this.ANSIMap.Foreground.Cyan
            '[cyn]'        = $this.ANSIMap.Foreground.Cyan
            '[green]'      = $this.ANSIMap.Foreground.Green
            '[grn]'        = $this.ANSIMap.Foreground.Green
            '[magenta]'    = $this.ANSIMap.Foreground.Magenta
            '[mgn]'        = $this.ANSIMap.Foreground.Magenta
            '[red]'        = $this.ANSIMap.Foreground.Red
            '[white]'      = $this.ANSIMap.Foreground.White
            '[wte]'        = $this.ANSIMap.Foreground.White
            '[yellow]'     = $this.ANSIMap.Foreground.Yellow
            '[ylw]'        = $this.ANSIMap.Foreground.Yellow
#endregion
#region Bright foreground colors
            '[bblack]'     = $this.ANSIMap.Foreground.BrightBlack
            '[bblk]'       = $this.ANSIMap.Foreground.BrightBlack
            '[bblue]'      = $this.ANSIMap.Foreground.BrightBlue
            '[bblu]'       = $this.ANSIMap.Foreground.BrightBlue
            '[bcyan]'      = $this.ANSIMap.Foreground.BrightCyan
            '[bcyn]'       = $this.ANSIMap.Foreground.BrightCyan
            '[bgreen]'     = $this.ANSIMap.Foreground.BrightGreen
            '[bgrn]'       = $this.ANSIMap.Foreground.BrightGreen
            '[bmagenta]'   = $this.ANSIMap.Foreground.BrightMagenta
            '[bmgn]'       = $this.ANSIMap.Foreground.BrightMagenta
            '[bred]'       = $this.ANSIMap.Foreground.BrightRed
            '[bwhite]'     = $this.ANSIMap.Foreground.BrightWhite
            '[bwte]'       = $this.ANSIMap.Foreground.BrightWhite
            '[byellow]'    = $this.ANSIMap.Foreground.BrightYellow
            '[bylw]'       = $this.ANSIMap.Foreground.BrightYellow
#endregion
#region Regular background colors
            '[bgblack]'    = $this.ANSIMap.Background.Black
            '[bgblk]'      = $this.ANSIMap.Background.Black
            '[bgblue]'     = $this.ANSIMap.Background.Blue
            '[bgblu]'      = $this.ANSIMap.Background.Blue
            '[bgcyan]'     = $this.ANSIMap.Background.Cyan
            '[bgcyn]'      = $this.ANSIMap.Background.Cyan
            '[bggreen]'    = $this.ANSIMap.Background.Green
            '[bggrn]'      = $this.ANSIMap.Background.Green
            '[bgmagenta]'  = $this.ANSIMap.Background.Magenta
            '[bgmgn]'      = $this.ANSIMap.Background.Magenta
            '[bgred]'      = $this.ANSIMap.Background.Red
            '[bgwhite]'    = $this.ANSIMap.Background.White
            '[bgwte]'      = $this.ANSIMap.Background.White
            '[bgyellow]'   = $this.ANSIMap.Background.Yellow
            '[bgylw]'      = $this.ANSIMap.Background.Yellow
#endregion
#region Bright background colors
            '[bgbblack]'   = $this.ANSIMap.Background.BrightBlack
            '[bgbblk]'     = $this.ANSIMap.Background.BrightBlack
            '[bgbblue]'    = $this.ANSIMap.Background.BrightBlue
            '[bgbblu]'     = $this.ANSIMap.Background.BrightBlue
            '[bgbcyan]'    = $this.ANSIMap.Background.BrightCyan
            '[bgbcyn]'     = $this.ANSIMap.Background.BrightCyan
            '[bgbgreen]'   = $this.ANSIMap.Background.BrightGreen
            '[bgbgrn]'     = $this.ANSIMap.Background.BrightGreen
            '[bgbmagenta]' = $this.ANSIMap.Background.BrightMagenta
            '[bgbmgn]'     = $this.ANSIMap.Background.BrightMagenta
            '[bgbred]'     = $this.ANSIMap.Background.BrightRed
            '[bgbwhite]'   = $this.ANSIMap.Background.BrightWhite
            '[bgbwte]'     = $this.ANSIMap.Background.BrightWhite
            '[bgbyellow]'  = $this.ANSIMap.Background.BrightYellow
            '[bgbylw]'     = $this.ANSIMap.Background.BrightYellow
#endregion
#region Control codes on
            '[blink]'      = $this.ANSIMap.Blink
            '[bln]'        = $this.ANSIMap.Blink
            '[b]'          = $this.ANSIMap.Bold
            '[bold]'       = $this.ANSIMap.Bold
            '[bld]'        = $this.ANSIMap.Bold
            '[dim]'        = $this.ANSIMap.Dim
            '[hidden]'     = $this.ANSIMap.Hidden
            '[hdn]'        = $this.ANSIMap.Hidden
            '[italic]'     = $this.ANSIMap.Italic
            '[i]'          = $this.ANSIMap.Italic
            '[reverse]'    = $this.ANSIMap.Reverse
            '[rvs]'        = $this.ANSIMap.Reverse
            '[strike]'     = $this.ANSIMap.Strikethrough
            '[strk]'       = $this.ANSIMap.Strikethrough
            '[under]'      = $this.ANSIMap.Underline
            '[ul]'         = $this.ANSIMap.Underline
#endregion
#region Control codes off
            '[noblink]'    = $this.ANSIMap.BlinkOff
            '[/blink]'     = $this.ANSIMap.BlinkOff
            '[/bln]'       = $this.ANSIMap.BlinkOff
            '[/b]'         = $this.ANSIMap.BoldOff
            '[/bold]'      = $this.ANSIMap.BoldOff
            '[/bld]'       = $this.ANSIMap.BoldOff
            '[nobold]'     = $this.ANSIMap.BoldOff
            '[nodim]'      = $this.ANSIMap.DimOff
            '[/dim]'       = $this.ANSIMap.DimOff
            '[nohidden]'   = $this.ANSIMap.HiddenOff
            '[/hidden]'    = $this.ANSIMap.HiddenOff
            '[/hdn]'       = $this.ANSIMap.HiddenOff
            '[noi]'        = $this.ANSIMap.ItalicOff
            '[noitalic]'   = $this.ANSIMap.ItalicOff
            '[/i]'         = $this.ANSIMap.ItalicOff
            '[/italic]'    = $this.ANSIMap.ItalicOff
            '[/]'          = $this.ANSIMap.Reset
            '[o]'          = $this.ANSIMap.Reset
            '[off]'        = $this.ANSIMap.Reset
            '[reset]'      = $this.ANSIMap.Reset
            '[rst]'        = $this.ANSIMap.Reset
            '[noreverse]'  = $this.ANSIMap.ReverseOff
            '[/reverse]'   = $this.ANSIMap.ReverseOff
            '[/rvs]'       = $this.ANSIMap.ReverseOff
            '[nostrike]'   = $this.ANSIMap.StrikethroughOff
            '[/strike]'    = $this.ANSIMap.StrikethroughOff
            '[/strk]'      = $this.ANSIMap.StrikethroughOff
            '[noul]'       = $this.ANSIMap.UnderlineOff
            '[/ul]'        = $this.ANSIMap.UnderlineOff
            '[nounder]'    = $this.ANSIMap.UnderlineOff
            '[/under]'     = $this.ANSIMap.UnderlineOff
#endregion
        }
#endregion

        $this.MarkupOperators = [array] $this.PSANSICodes.Keys
        $this.MarkupOperators74Plus = @(
            '[dim]', '[nodim]'
        )
    }

    [string] Inline([string] $InputString) {
        return [string] ($this._GetANSIString($InputString))
    }

    [array] Inline([array] $InputString) {
        return [string] ($this._GetANSIString($InputString))
    }

    hidden [string] _GetANSIString([string] $InputString) {
        [string] $ReturnString = $InputString
        [string] $ReplaceChar = $this.NoANSIChar
        [string] $EscapedInput = [regex]::Escape($InputString)
        [array] $MatchArray = @()

        if ([System.String]::IsNullOrWhiteSpace($InputString)) {
            return [string] ($this.NoANSIChar)
        }

        ($this.MarkupOperators + $this.MarkupOperators74Plus) | ForEach-Object {
            $Operator = $_
            $EscapedOperator = [regex]::Escape($_)
            if ("$EscapedInput" -match "$EscapedOperator") {
                $this.HasOperators = $true
                $MatchArray += $Operator
            }
        }

        if (!($this.HasOperators)) {
            return [string] ($InputString)
        }

        foreach ($MarkupOperator in $MatchArray) {
            $ReplaceChar = $this.PSANSICodes["$MarkupOperator"]
            if (($MarkupOperator -in $this.MarkupOperators74Plus) -or ($this.NoANSISupport)) {
                if (($MarkupOperator -in $this.MarkupOperators74Plus) -and ($this.Below74)) {
                    $ReplaceChar = $this.NoANSIChar
                }
                if ($this.NoANSISupport) {
                    $ReplaceChar = $this.NoANSIChar
                }
            }
            else {
                $ReturnString = $ReturnString.Replace("$MarkupOperator", "$ReplaceChar")
            }
        }
        $ReturnString += $this.Reset
        return [string] ($ReturnString)
    }

    static [array] GetANSIString([array] $InputArray) {
        [array] $ReturnArray = @()
        if ([System.String]::IsNullOrEmpty($InputArray)) {
            Write-Warning -Message "$([EasyANSI]::GetANSIString("Encountered a [bblack]null or empty[/] object.: ${InputArray}"))"
            return ([array][System.String]::Empty)
        }
        if (($InputArray -isnot [string]) -and ($InputArray -isnot [array])) {
            Write-Warning -Message "$([EasyANSI]::GetANSIString("Encountered a non-string or non-array object in input array. [yellow][blink]Skipping:[/] ${InputArray}."))"
            return ([array] [System.String]::Empty)
        }
        foreach ($InputString in $InputArray) {
            $ReturnArray += [EasyANSI]::GetANSIString($InputString)
        }
        $ReturnArray = $ReturnArray | Where-Object { !([System.String]::IsNullOrWhiteSpace($_)) }
        return [array]($ReturnArray)
    }

    static [string] GetANSIString([string] $InputString) {
        if ([System.String]::IsNullOrWhiteSpace($InputString)) {
            return [System.String]::Empty
        }
        [EasyANSI] $ANSI = [EasyANSI]::new()
        return [string] ($ANSI._GetANSIString($InputString))
    }
}

function Get-ANSIString() {
    [CmdletBinding(DefaultParameterSetName = 'NoSet')]
    [OutputType([string])]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true, Position = 0, ParameterSetName = 'NoSet')]
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true, Position = 0,  ParameterSetName = 'Concat')]
        [ValidateScript( {
            (($_ -is [string]) -or ($_ -is [array]))
        }, ErrorMessage = "Input object must be a string, an array of strings, an array of arrays, or a mix of both.")]
        [object]
        $InputObject,

        [Parameter(Mandatory = $true, ParameterSetName = 'Concat')]
        [switch]
        $JoinArray,

        [Parameter(Mandatory = $false, ParameterSetName = 'Concat')]
        [ValidateScript( {
            (([System.String]::IsNullOrWhiteSpace($_)) -or ($_ -is [string]))
        })]
        [AllowNull()]
        [string]
        $JoinWith
    )

    begin {
        [bool] $InputIsArray = $InputObject -is [array]
        [bool] $InputIsString = $InputObject -is [string]
        [array] $ReturnArray = @()
        [EasyANSI] $ANSIObject = [EasyANSI]::new()
    }

    process {
        if (($null -eq $InputObject) -or ([System.String]::IsNullOrWhiteSpace($InputObject)) ) {
            return $null
        }

        if ($InputObject -is [string]) {
            return $ANSIObject.Inline($InputObject)
        }
        else {
            [array] $FlattenedArray = [array] (Get-FlattenedArray -InputArray $InputObject)
            [array] $StringArray = $FlattenedArray | Where-Object { $_ -is [string] }
            if ($FlattenedArray.Count -ne $StringArray.Count) {
                [int] $Skipped = $FlattenedArray.Count - $StringArray.Count
                [string] $Plural = {
                    if ($Skipped -eq 1) {
                        "element"
                    }
                    else {
                        "elements"
                    }
                }
                Write-Warning -Message "Input array contains $($ANSIObject.Inline("[b][ul]${Skipped}[/b][/ul] non-string ${Plural}, which [red][ul]will be skipped."))"
            }

            foreach ($InputString in $StringArray) {
                $ReturnArray += $ANSIObject.Inline($InputString)
            }
        }

    }

    end {
        if ($PSBoundParameters.ParameterSetName -eq 'ConCat') {
            if ($PSBoundParameters.JoinWith.IsPresent) {
                return ($ReturnArray -join $JoinWith)
            }
            else {
                return ($ReturnArray -join "`n")
            }
        }
        else {
            return $ReturnArray
        }
    }

}

function Get-FlattenedArray() {
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
            $InputArray | ForEach-Object {$_})) | Where-Object {
                ($null -ne $_) -and
                !([System.String]::IsNullOrWhiteSpace($_))
            }
    return $ReturnArray
}



Export-ModuleMember -Function * -Alias *
