using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TMS.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PathFormatAttribute : ValidationAttribute
    {
        public PathFormatAttribute() { }

        public override bool IsValid(object value)
        {
            string path = value as string;

            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith("-") || path.EndsWith("-"))
                {
                    return false;
                }
                // Path follow format a-z, 0-9 and separated by "-".
                Match pathFormat = Regex.Match(path.Trim(), "^[a-z0-9-]*$", RegexOptions.IgnoreCase);
                // False format path
                if (!pathFormat.Success)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
