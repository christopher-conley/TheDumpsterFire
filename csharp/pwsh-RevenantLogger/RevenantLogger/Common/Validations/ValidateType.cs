using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common {

    public class ValidateTypesAttribute : ValidateArgumentsAttribute {
        private readonly Type[] _allowedTypes;

        public ValidateTypesAttribute(params Type[] allowedTypes) {
            _allowedTypes = allowedTypes;
        }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics) {
            if (arguments == null) {
                throw new ValidationMetadataException("Value cannot be null");
            }

            PSObject psObj = arguments as PSObject;
            if (psObj == null) {
                throw new ValidationMetadataException("Value must be a PSObject");
            }

            // Get the base object's type
            Type baseType = psObj.BaseObject.GetType();

            if (!_allowedTypes.Any(t => t.IsAssignableFrom(baseType))) {
                string allowedTypeNames = string.Join(", ", _allowedTypes.Select(t => t.Name));
                throw new ValidationMetadataException(
                    $"Value must be one of the following types: {allowedTypeNames}. Got: {baseType.Name}");
            }
        }
    }
}
