using System;
using System.Management.Automation;
using System.Text;

#nullable enable

/// Use in PowerShell like:
/// 
/// $ImportTypeData = Get-Content -Raw "Path/to/PowerShellStringBuilder.cs"
/// Add-Type -TypeDefinition $ImportTypeData -Language CSharp
/// $MyAwesomeStringBuilder = [RosettaTools.Pwsh.Utilities.StringBuilder]::new()
/// 
/// $MyAwesomeStringBuilder.Append("Hello, World!")
/// $MyAwesomeStringBuilder.AppendLine("This is a new line.")
/// $MyAwesomeStringBuilder.AppendLineLinux("This is another line with a UNIX-style line ending.")
/// $MyAwesomeStringBuilder.AppendLineArrayLinux(@("This is an arrary", "appended", "with \n"))
/// $MyAwesomeStringBuilder.AppendLineArrayWindows(@("This is an arrary", "appended", "with \r\n"))
/// $MyAwesomeStringBuilder.ToString()
/// 
///
/// Yes, I know that Types.ps1xml is a thing.
/// 
/// No, I didn't find out about it until after I'd already written 95% of this, and I needed something to just work.
/// I'll convert it over later.
/// 
/// Stop judging me.
/// 

namespace RosettaTools.Pwsh.Utilities
{
    public class StringBuilder
    {
        private System.Text.StringBuilder? _pwshStringBuilder;
        public System.Text.StringBuilder PwshStringBuilder
        {
            get {
                _pwshStringBuilder ??= new System.Text.StringBuilder();
                return _pwshStringBuilder;
            }
            set => _pwshStringBuilder = value;
        }

        public StringBuilder()
        {
            Init();
        }
        public void Init()
        {
            // PowerShell refuses to initialize the type via Add-Type when this is set here instead of
            // in the constructor and it's not originally declared as nullable, even though its value
            // is filled before the constructor exits. PowerShell is drunk sometimes.

            _pwshStringBuilder = new System.Text.StringBuilder();
        }
        public static StringBuilder NewPlainStringBuilder()
        {
            StringBuilder SBObject = new StringBuilder();
            return SBObject;
        }

        public string ToStringWithNewLine() => PwshStringBuilder.ToString() + System.Environment.NewLine;
        public string ToStringWithNewLineWindows() => PwshStringBuilder.ToString() + "\r\n";
        public string ToStringWithNewLineUNIX() => PwshStringBuilder.ToString() + "\n";
        public string ToStringWithNewLineLinux() => PwshStringBuilder.ToString() + "\n";

        public static string ToStringWithNewLine(StringBuilder sb) => sb.PwshStringBuilder.ToString() + System.Environment.NewLine;
        public static string ToStringWithNewLine(System.Text.StringBuilder sb) => sb.ToString() + System.Environment.NewLine;

        public static string ToStringWithNewLineWindows(StringBuilder sb) => sb.PwshStringBuilder.ToString() + "\r\n";
        public static string ToStringWithNewLineWindows(System.Text.StringBuilder sb) =>  sb.ToString() + "\r\n";

        public static string ToStringWithNewLineUNIX(StringBuilder sb) => sb.PwshStringBuilder.ToString() + "\n";
        public static string ToStringWithNewLineUNIX(System.Text.StringBuilder sb) => sb.ToString() + "\n";

        public static string ToStringWithNewLineLinux(StringBuilder sb) => sb.PwshStringBuilder.ToString() + "\n";
        public static string ToStringWithNewLineLinux(System.Text.StringBuilder sb) => sb.ToString() + "\n";


        public System.Text.StringBuilder Append(string text) => PwshStringBuilder.Append(text);
        public System.Text.StringBuilder Append(params object[] args) => PwshStringBuilder.Append(args);
        public System.Text.StringBuilder AppendFormat(string format, params object[] args)
        {
            return PwshStringBuilder.AppendFormat(format, args);
        }

        public System.Text.StringBuilder AppendLine() => PwshStringBuilder.AppendLine();
        public System.Text.StringBuilder AppendLine(string text) => PwshStringBuilder.AppendLine(text);

        public System.Text.StringBuilder AppendLine(string text, string newlineChar)
        {
            return PwshStringBuilder.Append(text).Append(newlineChar);
        }

        public System.Text.StringBuilder AppendLineWindows(string text) => PwshStringBuilder.Append(text).Append("\r\n");
        public System.Text.StringBuilder AppendLineUNIX(string text) => PwshStringBuilder.Append(text).Append("\n");
        public System.Text.StringBuilder AppendLineLinux(string text) => PwshStringBuilder.Append(text).Append("\n");

        public System.Text.StringBuilder Clear() => PwshStringBuilder.Clear();
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count) {
            PwshStringBuilder.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public void CopyTo(int sourceIndex, Span<char> destination, int count)
        {
            PwshStringBuilder.CopyTo(sourceIndex, destination, count);
        }

        public void EnsureCapacity(int capacity) => PwshStringBuilder.EnsureCapacity(capacity);

        public System.Text.StringBuilder.ChunkEnumerator GetChunks() => PwshStringBuilder.GetChunks();

        public override string ToString() => PwshStringBuilder.ToString();
        public string ToString(int startIndex, int length) => PwshStringBuilder.ToString(startIndex, length);

        public System.Text.StringBuilder AppendLineArrayWindows(string[] textArray)
        {
            return AppendLineArray(textArray, "\r\n");
        }

        public System.Text.StringBuilder AppendLineArrayUNIX(string[] textArray)
        {
            return AppendLineArray(textArray, "\n");
        }

        public System.Text.StringBuilder AppendLineArrayLinux(string[] textArray)
        {
            return AppendLineArray(textArray, "\n");
        }

        public System.Text.StringBuilder AppendLineArray(string[] textArray, string? newlineChar = null)
        {
            string newline = newlineChar ?? System.Environment.NewLine;
            if (textArray == null || textArray.Length == 0)
            {
                return PwshStringBuilder;
            }
            foreach (var text in textArray)
            {
                PwshStringBuilder.Append(text).Append(newline);
            }
            return PwshStringBuilder;
        }

        public System.Text.StringBuilder AppendArray(PSObject textArray)
        {
            if (PSObjectIsArray(textArray))
            {
                object[]? appendArray = (object[])textArray.BaseObject;
                foreach (var text in appendArray)
                {
                    PwshStringBuilder.Append(text);
                }
                return PwshStringBuilder;
            }
            else
            {
                throw new ArgumentException("Input object is not an array.", nameof(textArray));
            }
        }

        public System.Text.StringBuilder AppendArray(string[] textArray)
        {
            if (textArray == null || textArray.Length == 0)
            {
                return PwshStringBuilder;
            }
            foreach (var text in textArray)
            {
                PwshStringBuilder.Append(text);
            }
            return PwshStringBuilder;
        }
        private bool PSObjectIsArray(PSObject? psObject)
        {
            if (psObject == null || psObject.BaseObject == null)
            {
                return false;
            }
            Type baseType = psObject.BaseObject.GetType();
            return baseType.IsArray || (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>));
        }
    }
}
