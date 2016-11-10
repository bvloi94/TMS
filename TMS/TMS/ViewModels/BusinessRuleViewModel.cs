using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class BusinessRuleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public bool EnableRules { get; set; }
        public List<Rule> Rules { get; set; }
        public string Actions { get; set; }
        public string Technicians { get; set; }
        public List<DropdownTechnicianViewModel> technicianList { get; set; }
        public List<TriggerViewModel> actionList { get; set; }
    }

    public class Node
    {
        public string id { get; set; }
        public List<Rule> data { get; set; }
        public string parent { get; set; }
    }

    public class Rule
    {
        public string Id { get; set; }
        public int? Logic { get; set; }
        public string LogicText { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public int Condition { get; set; }
        public string ConditionText { get; set; }
        public string Value { get; set; }
        public string ValueMask { get; set; }
        public string ParentId { get; set; }
    }
}