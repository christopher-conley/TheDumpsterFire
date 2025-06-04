using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Text.EasyANSI {

    [CmdletBinding(ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "Default")]
    [Cmdlet(VerbsCommunications.Write, "EasyANSIText")]
    [Alias("Write-ANSIString")]
    [OutputType(typeof(string[]))]
    [OutputType(typeof(string))]
    public class WriteEasyANSIText : EasyANSIPSCmdlet {

        protected override void BeginProcessing() {
            base.init();
            string testingColor = $"{PSANSIInstance.Foreground.Red}This is a test{PSANSIInstance.Reset}";
            WriteInformation(new InformationRecord(testingColor, "Get-EasyANSIText"));
        }
        protected override void ProcessRecord() {

            WriteObject(null);
        }

    }
}
