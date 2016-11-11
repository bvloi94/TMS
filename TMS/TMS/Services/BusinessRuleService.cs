using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class BusinessRuleService
    {
        private readonly UnitOfWork _unitOfWork;

        public BusinessRuleService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool UpdateBusinessRule(BusinessRule businessRule)
        {
            _unitOfWork.BusinessRuleRepository.Update(businessRule);
            return _unitOfWork.Commit();
        }


        public IEnumerable<BusinessRule> GetAll()
        {
            return _unitOfWork.BusinessRuleRepository.Get(a => (bool) a.IsActive);
        }

        public void RemoveAllRuleRelatedInfo(int businessRuleId)
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

        public int AddNew(BusinessRule businessRule)
        {
            businessRule.IsActive = true;
            _unitOfWork.BusinessRuleRepository.Insert(businessRule);
            _unitOfWork.Commit();
            return businessRule.ID;
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