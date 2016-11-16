using System;
using System.ComponentModel.DataAnnotations;

namespace TMS.Utils
{
    public enum CompareToOperation
    {
        EqualTo,
        LessThan,
        GreaterThan
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class CompareValidateDateAttribute : ValidationAttribute
    {
        CompareToOperation _compareToOperation;
        string _dateTimeProperty;
        public CompareValidateDateAttribute(CompareToOperation compareToOperation, string dateTimeProperty)
        {
            _compareToOperation = compareToOperation;
            _dateTimeProperty = dateTimeProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(_dateTimeProperty);
            if (propertyInfo == null)
            {
                return new ValidationResult(String.Format("Unknown property: {0}.", _dateTimeProperty));
            }
            else
            {
                DateTime? date = value as DateTime?;
                DateTime? compareToDate = propertyInfo.GetValue(validationContext.ObjectInstance, null) as DateTime?;

                if (date.HasValue && compareToDate.HasValue)
                {
                    switch (_compareToOperation)
                    {
                        case CompareToOperation.GreaterThan:
                            if (date.Value <= compareToDate.Value)
                            {
                                return new ValidationResult(ErrorMessage);
                            }
                            break;
                        case CompareToOperation.EqualTo:
                            if (date.Value.Date != compareToDate.Value.Date)
                            {
                                return new ValidationResult(ErrorMessage);
                            }
                            break;
                        case CompareToOperation.LessThan:
                            if (date.Value >= compareToDate.Value)
                            {
                                return new ValidationResult(ErrorMessage);
                            }
                            break;
                    }
                }
                return ValidationResult.Success;
            }
        }

        private object _typeId = new object();
        public override object TypeId
        {
            get
            {
                return this._typeId;
            }
        }
    }
}
