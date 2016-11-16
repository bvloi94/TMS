using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Services
{
    public class BusinessRuleService
    {
        private readonly UnitOfWork _unitOfWork;

        public BusinessRuleService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IEnumerable<BusinessRule> GetAll()
        {
            return _unitOfWork.BusinessRuleRepository.Get(a => (bool)a.IsActive);
        }

        public void RemoveAllBusinessRuleRelatedInfo(int businessRuleId)
        {
            var brConditions = this.GetAllBusinessRuleConditions(businessRuleId);
            foreach (var con in brConditions)
            {
                _unitOfWork.BusinessRuleConditionRepository.Delete(con.ID);

            }
            var brActions = this.GetAllBusinessRuleTrigger(businessRuleId);
            foreach (var act in brActions)
            {
                _unitOfWork.BusinessRuleTriggerRepository.Delete(act.ID);
            }
            var brNotifications = this.GetAllBusinessRuleNotifications(businessRuleId);
            foreach (var act in brNotifications)
            {
                _unitOfWork.BusinessRuleNotificationRepository.Delete(act.ID);

            }
        }

        public BusinessRule GetById(int id)
        {

            return _unitOfWork.BusinessRuleRepository.GetByID(id);
        }

        public bool AddNewBusinessRule(BusinessRule businessRule, string conditions, string actions, string technicians)
        {
            _unitOfWork.BeginTransaction();
            _unitOfWork.BusinessRuleRepository.Insert(businessRule);
            _unitOfWork.Commit();
            AddAllBusinessRuleRelatedInfo(businessRule.ID, conditions, actions, technicians);
            return _unitOfWork.CommitTransaction();
        }

        public bool UpdateBusinessRule(BusinessRule businessRule, string conditions, string actions, string technicians)
        {
            _unitOfWork.BeginTransaction();
            _unitOfWork.BusinessRuleRepository.Update(businessRule);
            RemoveAllBusinessRuleRelatedInfo(businessRule.ID);
            AddAllBusinessRuleRelatedInfo(businessRule.ID, conditions, actions, technicians);
            return _unitOfWork.CommitTransaction();
        }

        private void AddAllBusinessRuleRelatedInfo(int id, string conditions, string actions, string technicians)
        {
            List<BusinessRuleCondition> businessRuleConditions = new List<BusinessRuleCondition>();
            List<Rule> ruleList = new List<Rule>();
            var js = new JavaScriptSerializer();
            object[] ruleTree = (object[])js.DeserializeObject(conditions);
            if (ruleTree != null)
            {
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
                }
            }

            // Add condition to database
            AddConditionsToDB(0, 1, id, null, "#", ruleList);

            // Add trigger 
            object[] actionSet = (object[])js.DeserializeObject(actions);
            if (actionSet != null)
            {
                for (int i = 0; i < actionSet.Length; i++)
                {
                    Dictionary<string, object> action = (Dictionary<string, object>)actionSet[i];
                    BusinessRuleTrigger trigger = new BusinessRuleTrigger();
                    trigger.BusinessRuleID = id;
                    trigger.Action = Convert.ToInt32(action["id"]);
                    trigger.Value = (string)action["value"];
                    AddTrigger(trigger);
                }
            }

            // Add technician notification
            var technicianList = (object[])js.DeserializeObject(technicians);
            if (technicianList != null)
            {
                for (int i = 0; i < technicianList.Length; i++)
                {
                    string techId = technicianList[i].ToString();
                    if (_unitOfWork.AspNetUserRepository.GetByID(techId) != null)
                    {
                        BusinessRuleNotification brNotification = new BusinessRuleNotification();
                        brNotification.BusinessRuleID = id;
                        brNotification.ReceiverID = techId;
                        _unitOfWork.BusinessRuleNotificationRepository.Insert(brNotification);
                        _unitOfWork.Commit();
                    }
                }
            }
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
                    _unitOfWork.BusinessRuleConditionRepository.Insert(condition);
                    _unitOfWork.Commit();
                    AddConditionsToDB(index + 1, level + 1, businessRuleId, condition.ID, tempRule.Id, ruleList);
                }
            }
        }

        public int AddCondition(BusinessRuleCondition condition)
        {
            _unitOfWork.BusinessRuleConditionRepository.Insert(condition);
            _unitOfWork.Commit();
            return condition.ID;
        }

        public IEnumerable<BusinessRuleCondition> GetAllBusinessRuleConditions(int businessRuleId)
        {
            IEnumerable<BusinessRuleCondition> cons = _unitOfWork.BusinessRuleConditionRepository.Get();
            return cons.Where(a => a.BusinessRuleID == businessRuleId);
        }

        public void AddNotificationReciever(BusinessRuleNotification businessRuleNotification)
        {
            _unitOfWork.BusinessRuleNotificationRepository.Insert(businessRuleNotification);
            _unitOfWork.Commit();
        }

        public IEnumerable<BusinessRuleNotification> GetAllBusinessRuleNotifications(int businessRuleId)
        {
            return _unitOfWork.BusinessRuleNotificationRepository.Get(a => a.BusinessRuleID == businessRuleId);
        }


        public void AddTrigger(BusinessRuleTrigger trigger)
        {
            _unitOfWork.BusinessRuleTriggerRepository.Insert(trigger);
            _unitOfWork.Commit();
        }

        public IEnumerable<BusinessRuleTrigger> GetAllBusinessRuleTrigger(int businessRuleId)
        {
            return _unitOfWork.BusinessRuleTriggerRepository.Get(a => a.BusinessRuleID == businessRuleId);
        }
    }
}