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

        private UnitOfWork unitOfWork = new UnitOfWork();

        public BusinessRuleController()
        {
            _businessRuleService = new BusinessRuleService(unitOfWork);
            _userService = new UserService(unitOfWork);
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
                rule.LogicText = (con.Type == 2) ? "OR" : "AND";
                rule.Condition = Convert.ToInt32(con.Condition);
                rule.ConditionText = TMSUtils.GetDefaultCondition()[Convert.ToInt32(con.Condition)].Name;
                // rule.Criteria = 1; TODO
                //rule.CriteriaText = TMSUtils.GetDefaultCritetia()[Convert.ToInt32(con.Criteria)].Name;
                rule.Value = con.Value;
                rule.ParentId = con.BusinessRuleConditionID.ToString();
                ruleList.Add(rule);
            }
            brModel.Rules = ruleList;

            //Load all action
            var actionList = new List<DropDownViewModel>();
            var brTriggers = _businessRuleService.GetAllBusinessRuleTrigger(brId);
            foreach (var no in brTriggers)
            {
                DropDownViewModel item = new DropDownViewModel();
                item.Id = no.Action;
                item.Name = no.Value.ToString(); //TODO
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
                // trigger.Value = (string) action["value"]; // TODO
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

            // Add new business rule to database
            int businessRuleId = brId;
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
                // trigger.Value = (string) action["value"]; // TODO
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
                    condition.Type = tempRule.Logic.HasValue ? tempRule.Logic : 1;
                    condition.Criteria = tempRule.Criteria;
                    condition.Condition = tempRule.Condition;
                    condition.Value = tempRule.Value;
                    condition.BusinessRuleConditionID = Id;
                    condition.BusinessRuleConditionLevel = level;
                    AddConditionsToDB(index+1, level+1, businessRuleId, _businessRuleService.AddCondition(condition), tempRule.Id, ruleList);
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