using System;
using System.Management.Automation;

namespace PSPhlebotomist.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateTypeAttribute : ValidateArgumentsAttribute
    {
        private readonly Type[] _validTypes;

        public ValidateTypeAttribute(params Type[] validTypes)
        {
            if (validTypes == null || validTypes.Length == 0)
            {
                throw new ArgumentException("At least one valid type must be specified.", nameof(validTypes));
            }

            _validTypes = validTypes;
        }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            if (arguments == null)
            {
                return; // Let other validators handle null if needed
            }

            Type argumentType = arguments.GetType();

            if (!_validTypes.Contains(argumentType))
            {
                string validTypeNames = string.Join(", ", _validTypes.Select(t => t.Name));
                throw new ValidationMetadataException(
                    $"The argument is of type '{argumentType.Name}'. Valid types are: {validTypeNames}.");
            }
        }

        public override string ToString()
        {
            string typeList = string.Join(", ", _validTypes.Select(t => t.Name));
            return $"[ValidateType({typeList})]";
        }
    }
}
