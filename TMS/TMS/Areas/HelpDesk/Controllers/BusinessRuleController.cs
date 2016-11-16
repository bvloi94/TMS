using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TMS.DAL;
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
            brModel.Enable = br.EnableRule.HasValue ? br.EnableRule.Value : false;

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
                            var requester = _userService.GetUserById(list[0]);
                            if (requester != null)
                            {
                                rule.ValueMask = requester.Fullname;
                                if (list.Length > 1)
                                {
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        rule.ValueMask += ", " + _userService.GetUserById(list[i]).Fullname;
                                    }
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
                        case ConstantUtil.BusinessRuleCriteria.Mode:
                            rule.ValueMask = TMSUtils.ConvertModeFromInt(Convert.ToInt32(list[0]));
                            if (list.Length > 1)
                            {
                                for (int i = 1; i < list.Length; i++)
                                {
                                    rule.ValueMask += ", " + TMSUtils.ConvertModeFromInt(Convert.ToInt32(list[i]));
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
        public ActionResult Create(BusinessRuleViewModel viewModel)
        {
            // Create New BusinessRule
            BusinessRule businessRule = new BusinessRule();
            businessRule.Name = viewModel.Name.Trim();
            businessRule.Description = viewModel.Description;
            businessRule.EnableRule = viewModel.Enable;
            businessRule.IsActive = true;

            // Add new business rule to database
            var result = _businessRuleService.AddNewBusinessRule(businessRule, viewModel.Conditions, viewModel.Actions, viewModel.Technicians);

            return Json(new
            {
                success = result,
            });
        }

        [HttpPost]
        public ActionResult Update(BusinessRuleViewModel viewModel)
        {
            var result = false;
            // Update new business rule to database
            if (viewModel.Id.HasValue)
            {
                BusinessRule businessRule = _businessRuleService.GetById((int)viewModel.Id);
                if (businessRule != null)
                {
                    businessRule.Name = viewModel.Name;
                    businessRule.Description = viewModel.Description;
                    businessRule.EnableRule = viewModel.Enable;
                    result = _businessRuleService.UpdateBusinessRule(businessRule, viewModel.Conditions, viewModel.Actions,
                        viewModel.Technicians);
                }
            }

            return Json(new
            {
                success = result
            });
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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser admin = _userService.GetUserById(id);
            if (admin != null)
            {
                ViewBag.LayoutName = admin.Fullname;
                ViewBag.LayoutAvatarURL = admin.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}