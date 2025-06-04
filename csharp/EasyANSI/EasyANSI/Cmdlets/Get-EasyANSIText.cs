using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;
using System.Collections;

namespace RosettaTools.Text.EasyANSI {

    [CmdletBinding(ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "Default")]
    [Cmdlet(VerbsCommon.Get, "EasyANSIText")]
    [Alias("Get-ANSIString")]
    [OutputType(typeof(string[]))]
    [OutputType(typeof(string))]

    public class GetEasyANSIText : EasyANSIPSCmdlet {

        private readonly string _cmdletName;
        private string[] _outputArray;

        public string CmdletName { get => _cmdletName; }

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Default")]
        [Alias("InputArray")]
        [AllowNull()]
        public object[] InputObject
        {
            get;
            private set;
        }

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Default")]
        [AllowNull()]
        public SwitchParameter AsArray
        {
            get;
            private set;
        }

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Default", DontShow = true)]
        [AllowNull()]
        public SwitchParameter Testing
        {
            get;
            private set;
        }

        protected override void BeginProcessing() {

            if (null != InputObject)
            {
                _outputArray = GetFlattenedArray(InputObject);
            }

        }
        protected override void ProcessRecord() {

            if (Testing.IsPresent)
            {
                TestingMessages(cmdletName: CmdletName);
            }

            if ((null != InputObject) && (null != _outputArray))
            {
                if (AsArray.IsPresent)
                {
                    WriteObject(_outputArray, enumerateCollection: false);
                }
                else
                {
                    foreach (var item in _outputArray)
                    {
                        if (null != item)
                        {
                            WriteObject(item, enumerateCollection: false);
                        }
                    }
                }
            }
            else
            {
                WriteObject(String.Empty);
            }
        }

        public GetEasyANSIText() {
            _cmdletName = "Get-EasyANSIText";
            StringsToParse = new();
            ;
        }
    }
}
