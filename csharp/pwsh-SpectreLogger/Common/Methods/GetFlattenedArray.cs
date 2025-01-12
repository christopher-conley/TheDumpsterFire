using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.SpectreLogger.Common {
    public abstract partial class SpectreLoggerPSCmdlet : SpectreLoggerBase {

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
            ArrayIterator++;

            if (recursiveCall) {
                InRecursion = true;
            }
            else {
                InRecursion = false;
                TotalOriginalItems = inputArray.Length;
            }

            if (inputArray.Length == 0) {
                ArrayIterator++;
                SkippedItems++;
                if ((!InRecursion) && (TotalOriginalItems == 0)) {
                    StringsToParse = new StringInfo();
                }
                return;
            }

            foreach (var item in inputArray) {
                if (null == item) {
                    ArrayIterator++;
                    SkippedItems++;
                    continue;
                }
                if (item is System.String) {
                    if ((String.IsNullOrWhiteSpace(item as string))) {
                        ArrayIterator++;
                        SkippedItems++;
                        continue;
                    }
                    ArrayList.Add((string)item);
                }

                else if (item is System.Array) {
                    if ((null == item) || ((item as object[]).Length == 0)) {
                        ArrayIterator++;
                        SkippedItems++;
                        continue;
                    }
                    GetFlattenedArray(inputArray: (object[])item, recursiveCall: true);
                }

                else {
                    WriteDebug("Encountered unknown/invalid item type, skipping.");
                    ArrayIterator++;
                    SkippedItems++;
                    continue;
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
