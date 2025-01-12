using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;

namespace RosettaTools.Text.EasyANSI {

    [CmdletBinding(ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "Default")]
    [Cmdlet(VerbsCommon.Get, "EasyANSIString")]
    [Alias("Get-ANSIString")]
    [OutputType(typeof(string[]))]
    [OutputType(typeof(string))]

    public class GetEasyANSIString : EasyANSIPSCmdlet {

        private string _cmdletName = "Get-EasyANSIString";
        private object? _result = null;

        public string CmdletName
        {
            get {
                return _cmdletName;
            }
            private set {
                _cmdletName = value;
            }
        }

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Default")]
        [AllowNull()]
        public dynamic Text
        {
            get; set;
        }

        protected override void BeginProcessing() {
            base.init();

            if ((null == Text)) {
                _result = null;
                ProcessRecord();
            }

            string testingColor = $"{PSANSIInstance.Foreground.Red}This is a test{PSANSIInstance.Reset}";
            string testingColor2 = $"\n{PSANSIInstance.Foreground.Red}{PSANSIInstance.Blink}This is a blinking test{PSANSIInstance.Reset}";
            ;
            ;
            ;
            GetFlattenedArray(inputArray: Text, recursiveCall: false);
            var blah = StringsToParse;
            ;
            string info = $"\nParsed {StringsToParse.TotalItems} total item(s) from {StringsToParse.TotalOriginalItems} original item(s).\n";
            info += $"Total number of item(s) to be parsed is: {StringsToParse.FlattenedArray.Length}\n";
            info += $"Skipped {StringsToParse.SkippedItems} null, empty, or unknown item(s).\n";
            ;
            WriteInformation(new InformationRecord(info, CmdletName));
            ;
            ;
            ;
            WriteInformation(new InformationRecord(testingColor, CmdletName));
            WriteInformation(new InformationRecord(testingColor2, CmdletName));

            _result = testingColor;
        }
        protected override void ProcessRecord() {
            
            WriteObject(_result);
        }

        public GetEasyANSIString() {
            ;
            ;
            ;
            ;
            ;
        }
    }
}
