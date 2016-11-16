using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TMS.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DueByDateValidateAttribute : ValidationAttribute
    {
        int _intervalDay;
        string _dateTimeProperty;

        public DueByDateValidateAttribute(int intervalDay, string dateTimeProperty)
        {
            _intervalDay = intervalDay;
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
                    if (date.Value.Date > compareToDate.Value.AddDays(-_intervalDay).Date)
                    {
                        return new ValidationResult(ErrorMessage);
                    }
                }
                return null;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("Due By Date must less than {0} days than Schedule End Date", _intervalDay.ToString());
        }
    }
}
