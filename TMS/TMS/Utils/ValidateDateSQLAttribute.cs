using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace TMS.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ValidateDateSQLAttribute : ValidationAttribute
    {

        public override bool IsValid(object value)
        {
            DateTime? date = value as DateTime?;

            if (date.HasValue)
            {
                if (date <= (DateTime) SqlDateTime.MinValue || date >= (DateTime) SqlDateTime.MaxValue)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
