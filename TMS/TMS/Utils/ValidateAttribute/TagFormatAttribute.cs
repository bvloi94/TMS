using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TMS.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TagFormatAttribute : ValidationAttribute
    {
        public TagFormatAttribute() { }

        public override bool IsValid(object value)
        {
            string unformattedString = value as string;

            if (!string.IsNullOrEmpty(unformattedString))
            {
                // Keyword follow format a-z, 0-9 and separated by comas (",").
                Match match = Regex.Match(unformattedString.Trim(), "^[a-z0-9-, ]*$", RegexOptions.IgnoreCase);
                // Keyword is not match format.
                if (!match.Success)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
