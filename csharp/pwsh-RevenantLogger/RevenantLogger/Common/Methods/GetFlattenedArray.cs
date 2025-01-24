using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common {
    public abstract partial class RevenantLoggerPSCmdlet : RevenantLoggerBase {

        private string[]? _flattenedArray;
        private List<string> _arrayList = [];
        private int _totalOriginalItems = 0;
        private int _totalItems = 0;
        private int _skippedItems = 0;
        private int _arrayIterator = 0;
        private bool _inRecursion;

        private int ArrayIterator
        {
            get => _arrayIterator;
            set {
                _totalItems += value;
                if (InRecursion) {
                    return;
                }
                else {
                    _arrayIterator += value;
                }
            }
        }

        public string[]? FlattenedArray { get => _flattenedArray; set => _flattenedArray = value; }
        public List<string> ArrayList { get => _arrayList; set => _arrayList = value; }
        private protected int TotalOriginalItems { get => _totalOriginalItems; set => _totalOriginalItems = value; }
        private protected int TotalItems { get => _totalItems; set => _totalItems = value; }
        private protected int SkippedItems { get => _skippedItems; set => _skippedItems = value; }
        private protected bool InRecursion { get => _inRecursion; set => _inRecursion = value; }

        private protected void GetFlattenedArray(object[] inputArray, bool recursiveCall) {
            _arrayIterator++;

            if (recursiveCall) {
                InRecursion = true;
            }
            else {
                InRecursion = false;
                TotalOriginalItems = inputArray.Length;
            }

            if (inputArray.Length == 0) {
                _arrayIterator++;
                SkippedItems++;
                if ((!InRecursion) && (TotalOriginalItems == 0)) {
                    StringsToParse = new StringInfo();
                }
                return;
            }

            List<object>? arrayList = [];
            object[]? loopArray;

            foreach (var item in inputArray)
            {
                if (null == item)
                {
                    _arrayIterator++;
                    SkippedItems++;
                    continue;
                }

                arrayList.Add(((PSObject)item).BaseObject);
            }

            loopArray = arrayList.ToArray();

            foreach (var item in loopArray) {
                if (null == item) {
                    _arrayIterator++;
                    SkippedItems++;
                    continue;
                }

                if (item is System.String)
                {
                    if ((String.IsNullOrWhiteSpace(item as string)))
                    {
                        _arrayIterator++;
                        SkippedItems++;
                        continue;
                    }
                    ArrayList.Add((string)item);
                }

                else if (item is System.Array)
                {
                    if ((null == item) || ((item as object[]).Length == 0))
                    {
                        _arrayIterator++;
                        SkippedItems++;
                        continue;
                    }
                    GetFlattenedArray(inputArray: (object[])item, recursiveCall: true);
                }

                else if (item is DirectoryInfo)
                {
                    if ((String.IsNullOrWhiteSpace((item as DirectoryInfo).FullName)))
                    {
                        _arrayIterator++;
                        SkippedItems++;
                        continue;
                    }
                    ArrayList.Add((item as DirectoryInfo).FullName);
                }

                else if (item is FileInfo)
                {
                    if ((String.IsNullOrWhiteSpace((item as FileInfo).FullName)))
                    {
                        _arrayIterator++;
                        SkippedItems++;
                        continue;
                    }
                    ArrayList.Add((item as FileInfo).FullName);
                }

                else
                {
                    WriteDebug("Encountered unknown/invalid item type, calling .ToString() method.");
                    try
                    {
                        ArrayList.Add(item.ToString());
                    }
                    catch (Exception ex)
                    {
                        WriteError(new ErrorRecord(ex, "Error converting item to string", ErrorCategory.InvalidOperation, item));
                        _arrayIterator++;
                        SkippedItems++;
                        continue;
                    }
                }
            }

            if (!InRecursion) {
                FlattenedArray = ArrayList.ToArray();
                StringsToParse = new StringInfo {
                    TotalItems = TotalItems,
                    TotalOriginalItems = TotalOriginalItems,
                    SkippedItems = SkippedItems,
                    FlattenedArray = FlattenedArray
                };
            }

            InRecursion = false;
        }
    }
}
