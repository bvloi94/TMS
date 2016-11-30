using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using LumiSoft.Net.Mime.vCard;
using Org.BouncyCastle.Crypto.Engines;
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
        public GroupService _groupService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }

        private UnitOfWork unitOfWork = new UnitOfWork();

        public BusinessRuleController()
        {
            _businessRuleService = new BusinessRuleService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _groupService = new GroupService(unitOfWork);
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
            BusinessRule br = _businessRuleService.GetById((int)id);
            if (br == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BusinessRuleViewModel brModel = new BusinessRuleViewModel();
            brModel.Id = br.ID;
            brModel.Name = br.Name;
            brModel.Description = br.Description;
            brModel.Enable = br.EnableRule ?? false;

            // Load all rules
            List<Rule> ruleList = new List<Rule>();
            var conditionList = _businessRuleService.GetAllBusinessRuleConditions(br.ID);
            foreach (var con in conditionList)
            {
                Rule rule = new Rule();
                rule.Id = con.ID.ToString();
                rule.Logic = con.Type;
                rule.Criteria = con.Criteria ?? 0;
                rule.Condition = con.Condition ?? 0;
                rule.Value = con.Value;
                if (rule.Criteria == 0 || rule.Condition == 0 || rule.Value == null)
                {
                    continue;
                }
                rule.LogicText = (con.Type == ConstantUtil.TypeOfBusinessRuleCondition.Or) ? "OR" : "AND";
                rule.CriteriaText = rule.Criteria > 0 ? TMSUtils.GetDefaultCritetia()[rule.Criteria - 1].Name : "";
                rule.ConditionText = rule.Condition > 0 ? TMSUtils.GetDefaultCondition()[rule.Condition - 1].Name : "";
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
                        case ConstantUtil.BusinessRuleCriteria.Group:
                            var group = _groupService.GetGroupById(TMSUtils.StrToIntDef(list[0], 0));
                            if (group != null)
                            {
                                rule.ValueMask = group.Name;
                                if (list.Length > 1)
                                {
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        var nextGroup = _groupService.GetGroupById(TMSUtils.StrToIntDef(list[i], 0));
                                        if (nextGroup != null)
                                        {
                                            {
                                                rule.ValueMask += ", " + nextGroup.Name;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Priority:
                            var pri = _priorityService.GetPriorityByID(TMSUtils.StrToIntDef(list[0], 0));
                            if (pri != null)
                            {
                                rule.ValueMask = pri.Name;
                                if (list.Length > 1)
                                {
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        var nextPri = _priorityService.GetPriorityByID(TMSUtils.StrToIntDef(list[i], 0));
                                        if (nextPri != null)
                                        {
                                            rule.ValueMask += ", " + nextPri.Name;
                                        }
                                    }
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Impact:
                            var impact = _impactService.GetImpactById(TMSUtils.StrToIntDef(list[0], 0));
                            if (impact != null)
                            {
                                rule.ValueMask = impact.Name;
                                if (list.Length > 1)
                                {
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        var nextImpact = _impactService.GetImpactById(TMSUtils.StrToIntDef(list[i], 0));
                                        if (nextImpact != null)
                                        {
                                            rule.ValueMask += ", " + nextImpact.Name;
                                        }
                                    }
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Urgency:
                            var urgency = _urgencyService.GetUrgencyByID(TMSUtils.StrToIntDef(list[0], 0));
                            if (urgency != null)
                            {
                                rule.ValueMask = urgency.Name;
                                if (list.Length > 1)
                                {
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        var nextUrg = _urgencyService.GetUrgencyByID(TMSUtils.StrToIntDef(list[i], 0));
                                        if (nextUrg != null)
                                        {
                                            rule.ValueMask += ", " + nextUrg.Name;

                                        }
                                    }
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Category:
                            var cate = _categoryService.GetCategoryById(TMSUtils.StrToIntDef(list[0], 0));
                            if (cate != null)
                            {
                                rule.ValueMask = cate.Name;
                                if (list.Length > 1)
                                {
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        var nextCate = _categoryService.GetCategoryById(TMSUtils.StrToIntDef(list[i], 0));
                                        if (nextCate != null)
                                        {
                                            rule.ValueMask += ", " + nextCate.Name;
                                        }
                                    }
                                }
                            }
                            break;
                        case ConstantUtil.BusinessRuleCriteria.Mode:
                            var mode = TMSUtils.ConvertModeFromInt(TMSUtils.StrToIntDef(list[0], 0));
                            if (mode != "-")
                            {
                                rule.ValueMask = mode;
                                if (list.Length > 1)
                                {
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        var nextMode = TMSUtils.ConvertModeFromInt(TMSUtils.StrToIntDef(list[i], 0));
                                        if (nextMode != "-")
                                        {
                                            rule.ValueMask += ", " + nextMode;
                                        }
                                    }
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
            var brTriggers = _businessRuleService.GetAllBusinessRuleTrigger(br.ID);
            foreach (var no in brTriggers)
            {
                TriggerViewModel item = new TriggerViewModel();
                item.Id = no.Action;
                item.Name = no.Value;
                item.Mask = "";
                switch (no.Action)
                {
                    case ConstantUtil.BusinessRuleTrigger.AssignToTechnician:
                        var user = _userService.GetUserById(no.Value);
                        if (user != null)
                        {
                            item.Mask =
                                TMSUtils.GetDefaultActions()[ConstantUtil.BusinessRuleTrigger.AssignToTechnician - 1].Name;
                            item.Mask += " \"" + user.Fullname + "\"";
                        }
                        break;
                    case ConstantUtil.BusinessRuleTrigger.SetTypeAs:
                        int type = TMSUtils.StrToIntDef(no.Value, 0);
                        if (type > 0)
                        {
                            item.Mask =
                                TMSUtils.GetDefaultActions()[ConstantUtil.BusinessRuleTrigger.SetTypeAs - 1].Name;
                            item.Mask += " \"" + TMSUtils.GetDefaultTypes()[type-1].Name + "\"";

                        }
                        break;
                    case ConstantUtil.BusinessRuleTrigger.MoveToCategory:
                    case ConstantUtil.BusinessRuleTrigger.MoveToSubCategory:
                    case ConstantUtil.BusinessRuleTrigger.MoveToItem:
                        Category cate = _categoryService.GetCategoryById(TMSUtils.StrToIntDef(no.Value, 0));
                        if (cate != null)
                        {
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
                        }
                        break;
                    case ConstantUtil.BusinessRuleTrigger.PlaceInGroup:
                        var group = _groupService.GetGroupById(TMSUtils.StrToIntDef(no.Value, 0));
                        if (group != null)
                        {
                            item.Mask = TMSUtils.GetDefaultActions()[ConstantUtil.BusinessRuleTrigger.PlaceInGroup - 1].Name;
                            item.Mask += " \"" + group.Name + "\"";
                        }
                        break;
                }
                if (item.Mask != "") actionList.Add(item);
            }
            brModel.actionList = actionList;

            // Load all recievers
            var helpdeskList = new List<DropdownTechnicianViewModel>();
            var brNotifications = _businessRuleService.GetAllBusinessRuleNotifications(br.ID);
            foreach (var no in brNotifications)
            {
                DropdownTechnicianViewModel item = new DropdownTechnicianViewModel();
                item.Id = no.AspNetUser.Id;
                item.Name = no.AspNetUser.Fullname;
                helpdeskList.Add(item);
            }
            brModel.helpdeskList = helpdeskList;

            return View(brModel);
        }

        [HttpPost]
        public ActionResult Create(BusinessRuleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = error.ErrorMessage
                        });
                    }
                }
            }
            // Create New BusinessRule
            BusinessRule businessRule = new BusinessRule();
            businessRule.Name = viewModel.Name.Trim();
            businessRule.Description = viewModel.Description;
            businessRule.EnableRule = viewModel.Enable;
            businessRule.IsActive = true;

            // Add new business rule to database
            var result = _businessRuleService.AddNewBusinessRule(businessRule, viewModel.Conditions, viewModel.Actions, viewModel.HelpDesks);

            return Json(new
            {
                success = result,
            });
        }

        [HttpPost]
        public ActionResult Update(BusinessRuleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = error.ErrorMessage
                        });
                    }
                }
            }
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
                        viewModel.HelpDesks);
                }
            }

            return Json(new
            {
                success = result
            });
        }

        public ActionResult Remove(int? id)
        {
            var br = _businessRuleService.GetById(id ?? -1);
            var msg = "Some error occured. Please try again later!";
            bool result = false;
            if (br == null)
            {
                result = true;
                msg = "The business rule was no longer existed!";
            }
            else
            {
                result = _businessRuleService.Remove(br.ID);
                if (result)
                {
                    msg = "Business rule was removed successfully!";
                }
            }
            return Json(new
            {
                success = result,
                message = msg
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
            var sortColumnIndex = TMSUtils.StrToIntDef(param.order[0]["column"], 0);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 0:
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
                s.IsActive = item.IsActive ?? false;
                rules.Add(s);
            }
            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.draw = param.draw;
            rsModel.recordsTotal = queriedResult.Count();
            rsModel.recordsFiltered = filteredListItems.Count();
            rsModel.data = rules;
            return Json(rsModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeStatus(int? id)
        {

            BusinessRule businessRule = _businessRuleService.GetById(id ?? 0);
            if (businessRule != null)
            {
                bool? wasActive = businessRule.IsActive;
                bool changeStatusResult = _businessRuleService.ChangeStatus(businessRule);
                var message = "";
                if (changeStatusResult)
                {
                    if (wasActive.HasValue && wasActive == true)
                    {
                        message = "Disable business rule successfully!";
                    }
                    else
                    {
                        message = "Enable business rule successfully!";
                    }
                    return Json(new
                    {
                        success = true,
                        message = message
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.UnavailableUser
                });
            }
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