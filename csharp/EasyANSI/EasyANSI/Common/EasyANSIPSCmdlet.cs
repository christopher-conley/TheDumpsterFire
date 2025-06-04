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

            [Hidden]
            public bool Initialized = false;

            public StringInfo() {
                TotalItems = 0;
                TotalOriginalItems = 0;
                SkippedItems = 0;
                FlattenedArray = new string[] { };
                Initialized = true;
            }
        }

        protected internal void TestingMessages(string cmdletName)
        {
            if ((null == PSANSIInstance) || (StringsToParse.Initialized == false))
            {
                return;
            }
            List<string> testingList = [];
            string[] testingArray;

            testingList.Add($"{PSANSIInstance.Foreground.Red}This is a test{PSANSIInstance.Reset}");
            testingList.Add($"\n{PSANSIInstance.Foreground.Red}{PSANSIInstance.Blink}This is a blinking test{PSANSIInstance.Reset}");
            testingList.Add($"\nParsed {StringsToParse.TotalItems} total item(s) from {StringsToParse.TotalOriginalItems} original item(s).\n");

            if ((null != StringsToParse.FlattenedArray) && (StringsToParse.FlattenedArray.Length > 0))
            {
                testingList.Add($"Total number of item(s) to be parsed is: {StringsToParse.FlattenedArray.Length}\n");
            }
            else
            {
                testingList.Add($"Flattened array is empty.\n");
            }
            testingList.Add($"Skipped {StringsToParse.SkippedItems} null, empty, or unknown item(s).\n");

            testingArray = testingList.ToArray();

            foreach (var item in testingArray)
            {
                if (null != item)
                {
                    WriteInformation(new InformationRecord(item, cmdletName));
                }
            }
        }
    }
}
