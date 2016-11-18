using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class PriorityService
    {
        private UnitOfWork _unitOfWork;

        public PriorityService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Priority> GetAll()
        {
            return _unitOfWork.PriorityRepository.Get();
        }

        public bool IsDuplicateName(int? id, string name)
        {
            if (id == null)
            {
                return _unitOfWork.PriorityRepository.Get(p => p.Name.ToLower().Equals(name.ToLower())).Any();
            }
            else
            {
                return _unitOfWork.PriorityRepository.Get(p => p.ID != id && p.Name.ToLower().Equals(name.ToLower())).Any();
            }
        }

        public bool AddPriority(Priority priority)
        {
            _unitOfWork.PriorityRepository.Insert(priority);
            return _unitOfWork.Commit();
        }

        public Priority GetPriorityByID(int id)
        {
            return _unitOfWork.PriorityRepository.GetByID(id);
        }

        public bool UpdatePriority(Priority priority)
        {
            _unitOfWork.PriorityRepository.Update(priority);
            return _unitOfWork.Commit();
        }

        public bool IsInUse(Priority priority)
        {

            return _unitOfWork.TicketRepository.Get(m => m.PriorityID == priority.ID).Any()
                || _unitOfWork.BusinessRuleConditionRepository.Get(m => m.Criteria == ConstantUtil.BusinessRuleCriteria.Priority
                                                                      && m.Condition.HasValue && m.Condition.Value == priority.ID).Any()
                || (_unitOfWork.BusinessRuleTriggerRepository.Get(m => m.Action == ConstantUtil.BusinessRuleTrigger.SetPriorityAs).Where(m => m.Value.Split(',').Contains(priority.ID.ToString()))).Any();
        }

        public bool DeletePriority(Priority priority)
        {
            _unitOfWork.BeginTransaction();
            foreach (PriorityMatrixItem priorityMatrixItem in priority.PriorityMatrixItems.ToList())
            {
                _unitOfWork.PriorityMatrixItemRepository.Delete(priorityMatrixItem);
            }
            _unitOfWork.PriorityRepository.Delete(priority);
            return _unitOfWork.CommitTransaction();
        }
    }
}