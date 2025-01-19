using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common {

    public class ValidateStringAttribute : ValidateArgumentsAttribute {
        public int MinLength { get; }
        public int MaxLength { get; }
        public bool AllowWhitespace { get; }
        public bool AllowNull { get; }

        public ValidateStringAttribute(int minLength = 1, int maxLength = int.MaxValue, bool allowWhitespace = true, bool allowNull = true) {
            MinLength = minLength;
            MaxLength = maxLength;
            AllowWhitespace = allowWhitespace;
            AllowNull = allowNull;
        }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics) {

            if (!AllowNull && arguments == null) {
                throw new ValidationMetadataException("Value cannot be null");
            }
            PSObject? psObj = arguments as PSObject;
            if (psObj == null) {
                // If it's not a PSObject, check directly if it's a string

                if (arguments is string directString) {
                    ValidateString(directString);
                    return;
                }
                throw new ValidationMetadataException($"Value must be a PSObject or string but was {arguments.GetType().Name}");
            }

            if (psObj.BaseObject is FileInfo) {
                ValidateString(((FileInfo)psObj.BaseObject).FullName);
                return;
            }

            if (psObj.BaseObject is DirectoryInfo) {
                ValidateString(((DirectoryInfo)psObj.BaseObject).FullName);
                return;
            }

            string? str = psObj.BaseObject as string;
            if ((psObj.BaseObject is string) && !AllowNull && str == null) {
                throw new ValidationMetadataException($"PSObject must contain a string but contained {psObj.BaseObject.GetType().Name}");
            }

            ValidateString(str);
        }

        private void ValidateString(string? str) {

            if (!AllowNull && str == null) {
                throw new ValidationMetadataException("Value cannot be null");
            }

            if (str is not string) {
                throw new ValidationMetadataException($"Value must be a string but was {str.GetType().Name}");
            }

            if (!AllowWhitespace && string.IsNullOrWhiteSpace(str)) {
                throw new ValidationMetadataException("String cannot be whitespace");
            }

            if (str.Length < MinLength) {
                throw new ValidationMetadataException($"String must be at least {MinLength} characters long");
            }

            if (str.Length > MaxLength) {
                throw new ValidationMetadataException($"String cannot be longer than {MaxLength} characters");
            }
        }
    }
}
