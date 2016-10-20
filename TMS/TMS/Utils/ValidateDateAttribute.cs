﻿using System;
using System.ComponentModel.DataAnnotations;

namespace TMS.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ValidateDateAttribute : ValidationAttribute
    {
        private int IntervalYear { get; set; }

        public ValidateDateAttribute(int intervalYear)
        {
            IntervalYear = intervalYear;
        }

        public override bool IsValid(object value)
        {
            DateTime? date = value as DateTime?;

            if (date.HasValue)
            {
                if (date > DateTime.Now.AddYears(IntervalYear))
                {
                    return false;
                }
            }

            return true;
        }
    }
}