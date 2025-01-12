using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Text.EasyANSI {

    [CmdletBinding(ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "Default")]
    [Cmdlet(VerbsCommunications.Write, "EasyANSIString")]
    [Alias("Write-ANSIString")]
    [OutputType(typeof(string[]))]
    [OutputType(typeof(string))]
    public class WriteEasyANSIString : EasyANSIPSCmdlet {

        protected override void BeginProcessing() {
            base.init();
            string testingColor = $"{PSANSIInstance.Foreground.Red}This is a test{PSANSIInstance.Reset}";
            WriteInformation(new InformationRecord(testingColor, "Get-EasyANSIString"));
        }
        protected override void ProcessRecord() {

            WriteObject(null);
        }

    }
}
