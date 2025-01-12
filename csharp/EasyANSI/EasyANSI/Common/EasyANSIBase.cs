using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace RosettaTools.Text.EasyANSI {
    public abstract class EasyANSIBase : PSCmdlet {
        private Dictionary<string, string> _pASANSIcodes;
        private PSStyle _ansiMap;
        private protected Dictionary<string, string> ANSIMap
        {
            get => _pASANSIcodes;
            private set => _pASANSIcodes = value;
        }
        private protected PSStyle PSANSIInstance
        {
            get => _ansiMap;
            private set => _ansiMap = value;
        }

        private protected string[] MarkupOperators
        {
            get; set;
        }

        private protected EasyANSIBase() {
            init();
        }

        private protected void init() {
            PSANSIInstance = PSStyle.Instance;
            ANSIMap = new Dictionary<string, string> {

#region Regular foreground colors
                { "[black]",    PSANSIInstance.Foreground.Black },
                { "[blk]",      PSANSIInstance.Foreground.Black },
                { "[blue]",     PSANSIInstance.Foreground.Blue },
                { "[blu]",      PSANSIInstance.Foreground.Blue },
                { "[cyan]",     PSANSIInstance.Foreground.Cyan },
                { "[cyn]",      PSANSIInstance.Foreground.Cyan },
                { "[green]",    PSANSIInstance.Foreground.Green },
                { "[grn]",      PSANSIInstance.Foreground.Green },
                { "[magenta]",  PSANSIInstance.Foreground.Magenta },
                { "[mgn]",      PSANSIInstance.Foreground.Magenta },
                { "[red]",      PSANSIInstance.Foreground.Red },
                { "[white]",    PSANSIInstance.Foreground.White },
                { "[wte]",      PSANSIInstance.Foreground.White },
                { "[yellow]",   PSANSIInstance.Foreground.Yellow },
                { "[ylw]",      PSANSIInstance.Foreground.Yellow },
#endregion
#region Bright foreground colors

                { "[bblack]",   PSANSIInstance.Foreground.BrightBlack },
                { "[bblk]",     PSANSIInstance.Foreground.BrightBlack },
                { "[bblue]",    PSANSIInstance.Foreground.BrightBlue },
                { "[bblu]",     PSANSIInstance.Foreground.BrightBlue },
                { "[bcyan]",    PSANSIInstance.Foreground.BrightCyan },
                { "[bcyn]",     PSANSIInstance.Foreground.BrightCyan },
                { "[bgreen]",   PSANSIInstance.Foreground.BrightGreen },
                { "[bgrn]",     PSANSIInstance.Foreground.BrightGreen },
                { "[bmagenta]", PSANSIInstance.Foreground.BrightMagenta },
                { "[bmgn]",     PSANSIInstance.Foreground.BrightMagenta },
                { "[bred]",     PSANSIInstance.Foreground.BrightRed },
                { "[bwhite]",   PSANSIInstance.Foreground.BrightWhite },
                { "[bwte]",     PSANSIInstance.Foreground.BrightWhite },
                { "[byellow]",  PSANSIInstance.Foreground.BrightYellow },
                { "[bylw]",     PSANSIInstance.Foreground.BrightYellow },

#endregion
#region Regular background colors
                { "[bgblack]",      PSANSIInstance.Background.Black },
                { "[bgblk]",        PSANSIInstance.Background.Black },
                { "[bgblue]",       PSANSIInstance.Background.Blue },
                { "[bgblu]",        PSANSIInstance.Background.Blue },
                { "[bgcyan]",       PSANSIInstance.Background.Cyan },
                { "[bgcyn]",        PSANSIInstance.Background.Cyan },
                { "[bggreen]",      PSANSIInstance.Background.Green },
                { "[bggrn]",        PSANSIInstance.Background.Green },
                { "[bgmagenta]",    PSANSIInstance.Background.Magenta },
                { "[bgmgn]",        PSANSIInstance.Background.Magenta },
                { "[bgred]",        PSANSIInstance.Background.Red },
                { "[bgwhite]",      PSANSIInstance.Background.White },
                { "[bgwte]",        PSANSIInstance.Background.White },
                { "[bgyellow]",     PSANSIInstance.Background.Yellow },
                { "[bgylw]",        PSANSIInstance.Background.Yellow },
#endregion
#region Bright background colors
                { "[bgbblack]",     PSANSIInstance.Background.BrightBlack },
                { "[bgbblk]",       PSANSIInstance.Background.BrightBlack },
                { "[bgbblue]",      PSANSIInstance.Background.BrightBlue },
                { "[bgbblu]",       PSANSIInstance.Background.BrightBlue },
                { "[bgbcyan]",      PSANSIInstance.Background.BrightCyan },
                { "[bgbcyn]",       PSANSIInstance.Background.BrightCyan },
                { "[bgbgreen]",     PSANSIInstance.Background.BrightGreen },
                { "[bgbgrn]",       PSANSIInstance.Background.BrightGreen },
                { "[bgbmagenta]",   PSANSIInstance.Background.BrightMagenta },
                { "[bgbmgn]",       PSANSIInstance.Background.BrightMagenta },
                { "[bgbred]",       PSANSIInstance.Background.BrightRed },
                { "[bgbwhite]",     PSANSIInstance.Background.BrightWhite },
                { "[bgbwte]",       PSANSIInstance.Background.BrightWhite },
                { "[bgbyellow]",    PSANSIInstance.Background.BrightYellow },
                { "[bgbylw]",       PSANSIInstance.Background.BrightYellow },
#endregion
#region Control codes on

                { "[blink]",    PSANSIInstance.Blink },
                { "[bln]",      PSANSIInstance.Blink },
                { "[b]",        PSANSIInstance.Bold },
                { "[bold]",     PSANSIInstance.Bold },
                { "[bld]",      PSANSIInstance.Bold },
#if NET8_0_OR_GREATER
                { "[dim]",      PSANSIInstance.Dim },
#else
                { "[dim]",      String.Empty },
#endif
                { "[hidden]",   PSANSIInstance.Hidden },
                { "[hdn]",      PSANSIInstance.Hidden },
                { "[italic]",   PSANSIInstance.Italic },
                { "[i]",        PSANSIInstance.Italic },
                { "[reverse]",  PSANSIInstance.Reverse },
                { "[rvs]",      PSANSIInstance.Reverse },
                { "[strike]",   PSANSIInstance.Strikethrough },
                { "[strk]",     PSANSIInstance.Strikethrough },
                { "[under]",    PSANSIInstance.Underline },
                { "[ul]",       PSANSIInstance.Underline },
                
#endregion
#region Control codes off

                { "[noblink]",  PSANSIInstance.BlinkOff },
                { "[/blink]",   PSANSIInstance.BlinkOff },
                { "[/bln]",     PSANSIInstance.BlinkOff },
                { "[/b]",       PSANSIInstance.BoldOff },
                { "[/bold]",    PSANSIInstance.BoldOff },
                { "[/bld]",     PSANSIInstance.BoldOff },
                { "[nobold]",   PSANSIInstance.BoldOff },
#if NET8_0_OR_GREATER
                { "[nodim]",    PSANSIInstance.DimOff },
                { "[/dim]",     PSANSIInstance.DimOff },
#else
                { "[/dim]",     String.Empty },
                { "[nodim]",    String.Empty },
#endif
                { "[nohidden]",     PSANSIInstance.HiddenOff },
                { "[/hidden]",      PSANSIInstance.HiddenOff },
                { "[/hdn]",         PSANSIInstance.HiddenOff },
                { "[noi]",          PSANSIInstance.ItalicOff },
                { "[noitalic]",     PSANSIInstance.ItalicOff },
                { "[/i]",           PSANSIInstance.ItalicOff },
                { "[/italic]",      PSANSIInstance.ItalicOff },
                { "[/]",            PSANSIInstance.Reset },
                { "[o]",            PSANSIInstance.Reset },
                { "[off]",          PSANSIInstance.Reset },
                { "[reset]",        PSANSIInstance.Reset },
                { "[rst]",          PSANSIInstance.Reset },
                { "[noreverse]",    PSANSIInstance.ReverseOff },
                { "[/reverse]",     PSANSIInstance.ReverseOff },
                { "[/rvs]",         PSANSIInstance.ReverseOff },
                { "[nostrike]",     PSANSIInstance.StrikethroughOff },
                { "[/strike]",      PSANSIInstance.StrikethroughOff },
                { "[/strk]",        PSANSIInstance.StrikethroughOff },
                { "[noul]",         PSANSIInstance.UnderlineOff },
                { "[/ul]",          PSANSIInstance.UnderlineOff },
                { "[nounder]",      PSANSIInstance.UnderlineOff },
                { "[/under]",       PSANSIInstance.UnderlineOff }
            };
#endregion
        }

        private protected void SetMarkupOperators() {
            List<string> operators = new();
            foreach (string code in ANSIMap.Keys) {
                operators.Add(code);
            }
            MarkupOperators = operators.ToArray();
        }
    }
}
