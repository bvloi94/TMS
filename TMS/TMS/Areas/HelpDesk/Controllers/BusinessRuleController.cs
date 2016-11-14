using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Xsl;
using Microsoft.Ajax.Utilities;
using TMS.DAL;
using TMS.Enumerator;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Areas.HelpDesk.Controllers
{
    [CustomAuthorize(Roles = "Helpdesk")]
    public class BusinessRuleController : Controller
    {

        public BusinessRuleService _businessRuleService { get; set; }
        public UserService _userService { get; set; }
        public DepartmentService _departmentService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }

        private UnitOfWork unitOfWork = new UnitOfWork();

        public BusinessRuleController()
        {
            _businessRuleService = new BusinessRuleService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _departmentService = new DepartmentService(unitOfWork);
            _urgencyService = new UrgencyService(unitOfWork);
            _priorityService = new PriorityService(unitOfWork);
            _impactService = new ImpactService(unitOfWork);
            _categoryService = new CategoryService(unitOfWork);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var brId = Convert.ToInt32(id);
            BusinessRuleViewModel brModel = new BusinessRuleViewModel();
            BusinessRule br = _businessRuleService.GetById(brId);

            brModel.Id = br.ID;
            brModel.Name = br.Name;
            brModel.Description = br.Description;

            // Load all rules
            List<Rule> ruleList = new List<Rule>();
            var conditionList = _businessRuleService.GetAllBusinessRuleConditions(brId);
            foreach (var con in conditionList)
            {
                Rule rule = new Rule();
                rule.Id = con.ID.ToString();
                rule.Logic = con.Type;
                rule.LogicText = (con.Type == ConstantUtil.TypeOfBusinessRuleCondition.Or) ? "OR" : "AND";
                rule.Condition = Convert.ToInt32(con.Condition);
                rule.ConditionText = TMSUtils.GetDefaultCondition()[Convert.ToInt32(con.Condition) - 1].Name;
                rule.Criteria = Convert.ToInt32(con.Criteria);
                rule.CriteriaText = TMSUtils.GetDefaultCritetia()[Convert.ToInt32(con.Criteria) - 1].Name;
                rule.Value = con.Value;
                if (rule.Value == null)
                {
                    rule.ValueMask = "";
                }
                else
                    if (rule.Criteria == ConstantUtil.BusinessRuleCriteria.Subject || rule.Criteria == ConstantUtil.BusinessRuleCriteria.Description)
                {
                    rule.ValueMask = con.Value;
                }
                else
                {
                    string[] list = rule.Value.Split(',');
                    switch (rule.Criteria)
                    {
                        case ConstantUtil.BusinessRuleCriteria.RequesterName:
                            rule.ValueMask = _userService.GetUserById(list[0]).Fullname;
                            if (list.Length > 1)
                            {
                                for (int i = 1; i < list.Length; i++)
                                {
                                    rule.ValueMask += ", " + _userService.GetUserById(list[i]).Fullname;
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Department:
                            rule.ValueMask = _departmentService.GetDepartmentById(Convert.ToInt32(list[0])).Name;
                            if (list.Length > 1)
                            {
                                for (int i = 1; i < list.Length; i++)
                                {
                                    rule.ValueMask += ", " + _departmentService.GetDepartmentById(Convert.ToInt32(list[i])).Name;
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Priority:
                            rule.ValueMask = _priorityService.GetPriorityByID(Convert.ToInt32(list[0])).Name;
                            if (list.Length > 1)
                            {
                                for (int i = 1; i < list.Length; i++)
                                {
                                    rule.ValueMask += ", " + _priorityService.GetPriorityByID(Convert.ToInt32(list[i])).Name;
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Impact:
                            rule.ValueMask = _impactService.GetImpactById(Convert.ToInt32(list[0])).Name;
                            if (list.Length > 1)
                            {
                                for (int i = 1; i < list.Length; i++)
                                {
                                    rule.ValueMask += ", " + _impactService.GetImpactById(Convert.ToInt32(list[i])).Name;
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Urgency:
                            rule.ValueMask = _urgencyService.GetUrgencyByID(Convert.ToInt32(list[0])).Name;
                            if (list.Length > 1)
                            {
                                for (int i = 1; i < list.Length; i++)
                                {
                                    rule.ValueMask += ", " + _urgencyService.GetUrgencyByID(Convert.ToInt32(list[i])).Name;
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Category:
                            rule.ValueMask = _categoryService.GetCategoryById(Convert.ToInt32(list[0])).Name;
                            if (list.Length > 1)
                            {
                                for (int i = 1; i < list.Length; i++)
                                {
                                    rule.ValueMask += ", " + _categoryService.GetCategoryById(Convert.ToInt32(list[i])).Name;
                                }
                            }
                            break;
                    }
                }
                rule.ParentId = con.BusinessRuleConditionID.ToString();
                ruleList.Add(rule);
            }
            brModel.Rules = ruleList;

            //Load all action
            var actionList = new List<TriggerViewModel>();
            var brTriggers = _businessRuleService.GetAllBusinessRuleTrigger(brId);
            foreach (var no in brTriggers)
            {
                TriggerViewModel item = new TriggerViewModel();
                item.Id = no.Action;
                item.Name = no.Value;
                switch (no.Action)
                {
                    case ConstantUtil.BusinessRuleTrigger.AssignToTechnician:
                        item.Mask =
                            TMSUtils.GetDefaultActions()[ConstantUtil.BusinessRuleTrigger.AssignToTechnician - 1].Name;
                        item.Mask += " \"" + _userService.GetUserById(no.Value).Fullname + "\"";
                        break;
                    case ConstantUtil.BusinessRuleTrigger.ChangeStatusTo:
                        item.Mask = TMSUtils.GetDefaultActions()[ConstantUtil.BusinessRuleTrigger.ChangeStatusTo - 1].Name;
                        item.Mask += " \"" + TMSUtils.GetDefaultStatus()[Convert.ToInt32(no.Value) - 1].Name + "\"";
                        break;
                    case ConstantUtil.BusinessRuleTrigger.MoveToCategory:
                    case ConstantUtil.BusinessRuleTrigger.MoveToSubCategory:
                    case ConstantUtil.BusinessRuleTrigger.MoveToItem:
                        Category cate = _categoryService.GetCategoryById(Convert.ToInt32(no.Value));
                        var mask = cate.Name;
                        int level = (int)cate.CategoryLevel;
                        int parentId;
                        item.Mask = TMSUtils.GetDefaultActions()[level + 1].Name;
                        while (level > 1)
                        {
                            parentId = (int)cate.ParentID;
                            cate = _categoryService.GetCategoryById(parentId);
                            mask = cate.Name + " >> " + mask;
                            level--;
                        }
                        item.Mask += " \"" + mask + "\"";
                        break;
                    case ConstantUtil.BusinessRuleTrigger.PlaceInDepartment:
                        item.Mask = TMSUtils.GetDefaultActions()[ConstantUtil.BusinessRuleTrigger.PlaceInDepartment - 1].Name;
                        item.Mask += " \"" + _departmentService.GetDepartmentById(Convert.ToInt32(no.Value)).Name + "\"";
                        break;
                    case ConstantUtil.BusinessRuleTrigger.SetPriorityAs:
                        item.Mask = TMSUtils.GetDefaultActions()[ConstantUtil.BusinessRuleTrigger.SetPriorityAs - 1].Name;
                        item.Mask += " \"" + _priorityService.GetPriorityByID(Convert.ToInt32(no.Value)).Name + "\"";
                        break;
                }
                actionList.Add(item);
            }
            brModel.actionList = actionList;

            // Load all recievers
            var technicianList = new List<DropdownTechnicianViewModel>();
            var brNotifications = _businessRuleService.GetAllBusinessRuleNotifications(brId);
            foreach (var no in brNotifications)
            {
                DropdownTechnicianViewModel item = new DropdownTechnicianViewModel();
                item.Id = no.AspNetUser.Id;
                item.Name = no.AspNetUser.Fullname;
                technicianList.Add(item);
            }
            brModel.technicianList = technicianList;

            return View(brModel);
        }

        [HttpPost]
        public ActionResult Create(string name, string description, string rules, string actions, string technicians)
        {
            unitOfWork.BeginTransaction();

            // Create New BusinessRule
            BusinessRule businessRule = new BusinessRule();

            //validate TODO

            businessRule.Name = name.Trim();
            businessRule.Description = description;
            businessRule.IsActive = true;

            // Add new business rule to database
            int businessRuleId = _businessRuleService.AddNew(businessRule);

            //enable rule checkbox TODO

            // Get conditions

            List<BusinessRuleCondition> businessRuleConditions = new List<BusinessRuleCondition>();
            List<Rule> ruleList = new List<Rule>();
            var js = new JavaScriptSerializer();
            object[] ruleTree = (object[])js.DeserializeObject(rules);
            for (int i = 0; i < ruleTree.Length; i++)
            {

                Dictionary<string, object> rule = (Dictionary<string, object>)ruleTree[i];
                Rule tempRule = js.ConvertToType<Rule>(rule["data"]);
                tempRule.Id = (string)rule["id"];
                tempRule.ParentId = (string)rule["parent"];
                if (tempRule.Condition != 0 && tempRule.Criteria != 0 && tempRule.Value != null)
                {
                    ruleList.Add(tempRule);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msg = "The rule have not finish yet. Please fill in!"
                    });
                }
            }

            // Add condition to database
            AddConditionsToDB(0, 1, businessRuleId, null, "#", ruleList);

            // Add trigger 
            object[] actionSet = (object[])js.DeserializeObject(actions);
            for (int i = 0; i < actionSet.Length; i++)
            {
                Dictionary<string, object> action = (Dictionary<string, object>)actionSet[i];
                BusinessRuleTrigger trigger = new BusinessRuleTrigger();
                trigger.BusinessRuleID = businessRuleId;
                trigger.Action = Convert.ToInt32(action["id"]);
                trigger.Value = (string)action["value"];
                _businessRuleService.AddTrigger(trigger);
            }

            // Add technician notification
            var technicianList = (object[])js.DeserializeObject(technicians);
            if (technicianList != null)
            {
                for (int i = 0; i < technicianList.Length; i++)
                {
                    string techId = technicianList[i].ToString();
                    if (_userService.GetUserById(techId) != null)
                    {
                        BusinessRuleNotification brNotification = new BusinessRuleNotification();
                        brNotification.BusinessRuleID = businessRuleId;
                        brNotification.ReceiverID = techId;
                        _businessRuleService.AddNotificationReciever(brNotification);
                    }
                }
            }

            unitOfWork.CommitTransaction();
            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public ActionResult Update(int brId, string name, string description, string rules, string actions, string technicians)
        {
            unitOfWork.BeginTransaction();

            // Update new business rule to database
            int businessRuleId = brId;
            BusinessRule br = new BusinessRule();
            br.ID = brId;
            br.Name = name;
            br.Description = description;
            br.IsActive = true;

            _businessRuleService.UpdateBusinessRule(br);
            _businessRuleService.RemoveAllRuleRelatedInfo(brId);

            //enable rule checkbox TODO

            // Get conditions
            List<BusinessRuleCondition> businessRuleConditions = new List<BusinessRuleCondition>();
            List<Rule> ruleList = new List<Rule>();
            var js = new JavaScriptSerializer();
            object[] ruleTree = (object[])js.DeserializeObject(rules);
            for (int i = 0; i < ruleTree.Length; i++)
            {
                Dictionary<string, object> rule = (Dictionary<string, object>)ruleTree[i];
                Rule tempRule = js.ConvertToType<Rule>(rule["data"]);
                tempRule.Id = (string)rule["id"];
                tempRule.ParentId = (string)rule["parent"];
                ruleList.Add(tempRule);
            }

            // Add condition to database
            AddConditionsToDB(0, 1, businessRuleId, null, "#", ruleList);

            // Add trigger 
            object[] actionSet = (object[])js.DeserializeObject(actions);
            for (int i = 0; i < actionSet.Length; i++)
            {
                Dictionary<string, object> action = (Dictionary<string, object>)actionSet[i];
                BusinessRuleTrigger trigger = new BusinessRuleTrigger();
                trigger.BusinessRuleID = businessRuleId;
                trigger.Action = Convert.ToInt32(action["id"]);
                trigger.Value = (string)action["value"];
                _businessRuleService.AddTrigger(trigger);
            }

            // Add technician notification
            var technicianList = (object[])js.DeserializeObject(technicians);
            if (technicianList != null)
            {
                for (int i = 0; i < technicianList.Length; i++)
                {
                    string techId = technicianList[i].ToString();
                    if (_userService.GetUserById(techId) != null)
                    {
                        BusinessRuleNotification brNotification = new BusinessRuleNotification();
                        brNotification.BusinessRuleID = businessRuleId;
                        brNotification.ReceiverID = techId;
                        _businessRuleService.AddNotificationReciever(brNotification);
                    }
                }
            }

            unitOfWork.CommitTransaction();
            return Json(new
            {
                success = true,
            });
        }

        private void AddConditionsToDB(int index, int level, int businessRuleId, int? Id, string parentId, List<Rule> ruleList)
        {
            for (int i = index; i < ruleList.Count; i++)
            {
                Rule tempRule = ruleList[i];
                if (ruleList[i].ParentId == parentId)
                {
                    BusinessRuleCondition condition = new BusinessRuleCondition();
                    condition.BusinessRuleID = businessRuleId;
                    condition.Type = tempRule.Logic.HasValue ? tempRule.Logic : ConstantUtil.TypeOfBusinessRuleCondition.And;
                    condition.Criteria = tempRule.Criteria;
                    condition.Condition = tempRule.Condition;
                    condition.Value = tempRule.Value;
                    condition.BusinessRuleConditionID = Id;
                    condition.BusinessRuleConditionLevel = level;
                    AddConditionsToDB(index + 1, level + 1, businessRuleId, _businessRuleService.AddCondition(condition), tempRule.Id, ruleList);
                }
            }
        }


        [HttpPost]
        public ActionResult LoadAll(JqueryDatatableParameterViewModel param)
        {
            var search_key = Request["search[value]"];

            var queriedResult = _businessRuleService.GetAll();
            IEnumerable<BusinessRule> filteredListItems;

            if (!string.IsNullOrEmpty(search_key))
            {
                filteredListItems = queriedResult.Where(p => p.Name.ToLower().Contains(search_key.ToLower()));
            }
            else
            {
                filteredListItems = queriedResult;
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(param.order[0]["column"]);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Name);
                    break;
            }

            var result = filteredListItems.Skip(param.start).Take(param.length).ToList();
            var rules = new List<BusinessRuleViewModel>();
            int startNo = param.start;
            foreach (var item in result)
            {
                var s = new BusinessRuleViewModel();
                s.Id = item.ID;
                s.Name = item.Name;
                s.Description = item.Description;
                rules.Add(s);
            }
            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.draw = param.draw;
            rsModel.recordsTotal = queriedResult.Count();
            rsModel.recordsFiltered = filteredListItems.Count();
            rsModel.data = rules;
            return Json(rsModel, JsonRequestBehavior.AllowGet);
        }
    }
}