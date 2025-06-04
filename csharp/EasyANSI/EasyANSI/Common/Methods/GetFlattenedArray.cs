using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Text.EasyANSI {
    public abstract partial class EasyANSIPSCmdlet : EasyANSIBase {

        private List<string> _scratchList = [];
        private object[] _scratchArray;
        private List<string> _arrayList = [];

        private protected string[] GetFlattenedArray(object[] InputArray) {

            foreach (var item in InputArray)
            {
                if (null == item)
                {
                    WriteDebug("Encountered null item in input array, skipping.");
                    continue;
                }
                if (item is string strItem)
                {
                    _scratchList.Add(strItem);
                }
                else if (item is System.Array arrItem)
                {
                    _scratchList.Add(GetStringFromArray(arrItem));
                }
                else
                {
                    _scratchList.Add(item.ToString() ?? String.Empty);
                }
            }

            _scratchArray = _scratchList.ToArray();

            foreach (string? item in _scratchArray)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    WriteDebug("Encountered empty or whitespace item in staging array, skipping.");
                    continue;
                }
                _arrayList.Add(item);
            }

            return _arrayList.ToArray();
        }

        private string GetStringFromArray<TObject>(TObject inputArray) where TObject : IEnumerable
        {

            if (null == inputArray)
            {
                return string.Empty;
            }

            var inputItem = inputArray as object[];

            if (null == inputItem || inputItem.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var item in inputItem)
            {
                if (item == null)
                {
                    WriteDebug("Encountered null item in nested input array, skipping.");
                    continue;
                }

                else if (item is System.Array arrayItem)
                {
                    sb.Append(Environment.NewLine);
                    // Recursively call self to handle n amount of nested arrays
                    sb.Append(GetStringFromArray(arrayItem.Cast<object>().ToArray()));
                    continue;
                }

                else if (item is IEnumerable<object> enumerableItem)
                {
                    foreach (var subItem in enumerableItem)
                    {
                        sb.Append(subItem?.ToString() ?? String.Empty);
                        sb.Append(Environment.NewLine);
                    }
                    continue;
                }

                else if (item is string stringItem)
                {
                    sb.Append(item);
                }
            }

            return sb.ToString();
        }
    }
}
