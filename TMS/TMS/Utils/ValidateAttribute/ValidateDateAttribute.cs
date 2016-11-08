using System;
using System.ComponentModel.DataAnnotations;

namespace TMS.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ValidateDateAttribute : ValidationAttribute
    {
        int _intervalYear;

        public ValidateDateAttribute(int intervalYear)
        {
            _intervalYear = intervalYear;
        }

        public override bool IsValid(object value)
        {
            DateTime? date = value as DateTime?;

            if (date.HasValue)
            {
                if (date > DateTime.Now.AddYears(_intervalYear))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
