using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Models;

namespace TMS.Class
{
    public class BusinessRuleConditionCustom
    {
        public BusinessRuleCondition BusinessRuleCondition { get; set; }
        public bool IsSatisfied { get; set; }
    }
}