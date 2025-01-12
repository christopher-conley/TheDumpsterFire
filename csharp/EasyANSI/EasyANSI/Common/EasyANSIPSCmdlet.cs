using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace RosettaTools.Text.EasyANSI {
    public abstract partial class EasyANSIPSCmdlet : EasyANSIBase {

        private protected StringInfo StringsToParse;

        public struct StringInfo {
            public int TotalItems;
            public int TotalOriginalItems;
            public int SkippedItems;
            public string[] FlattenedArray;

            public StringInfo() {
                TotalItems = 0;
                TotalOriginalItems = 0;
                SkippedItems = 0;
                FlattenedArray = new string[] { };
            }
        }
    }
}
