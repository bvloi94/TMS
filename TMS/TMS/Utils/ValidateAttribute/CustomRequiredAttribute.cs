using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TMS.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CustomRequiredAttribute : RequiredAttribute
    {
        public CustomRequiredAttribute() { }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }

            string val = value.ToString();

            if (!string.IsNullOrWhiteSpace(val))
            {
                int tempVal;
                int? valInt = Int32.TryParse(val, out tempVal) ? tempVal : (int?)null;
                if (valInt.HasValue && valInt > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
