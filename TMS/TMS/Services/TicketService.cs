using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Class;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class TicketService //: ITicketService
    {
        private readonly UnitOfWork _unitOfWork;

        public TicketService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Ticket> GetAll()
        {
            return _unitOfWork.TicketRepository.Get();
        }

        public Ticket GetTicketByID(int id)
        {
            return _unitOfWork.TicketRepository.GetByID(id);
        }

        public Ticket ParseTicket(Ticket ticket)
        {
            Ticket handlingTicket = ticket;
            //Handle automation job
            bool isSatisfied = false;
            IEnumerable<BusinessRule> businessRules = _unitOfWork.BusinessRuleRepository.Get(m => m.IsActive == true);
            foreach (BusinessRule businessRule in businessRules)
            {
                ICollection<BusinessRuleConditionCustom> businessRuleConditionCustomList = new List<BusinessRuleConditionCustom>();
                IEnumerable<BusinessRuleCondition> conditions = businessRule.BusinessRuleConditions;
                foreach (BusinessRuleCondition condition in conditions)
                {
                    BusinessRuleConditionCustom businessRuleConditionCustom = new BusinessRuleConditionCustom
                    {
                        BusinessRuleCondition = condition,
                        IsSatisfied = IsSatisfiedWithCondition(handlingTicket, condition)
                    };
                    businessRuleConditionCustomList.Add(businessRuleConditionCustom);
                }
                isSatisfied = IsSatisfiedWithMultipleConditions(handlingTicket, businessRuleConditionCustomList);
                if (isSatisfied)
                {
                    IEnumerable<BusinessRuleTrigger> triggers = businessRule.BusinessRuleTriggers;
                    foreach (BusinessRuleTrigger trigger in triggers)
                    {

                    }
                }
            }
            //end handle automation job
            return handlingTicket;
        }

        public IEnumerable<Ticket> GetSolvedTickets()
        {
            return _unitOfWork.TicketRepository.Get(m => m.Status == ConstantUtil.TicketStatus.Solved);
        }

        private bool IsSatisfiedWithMultipleConditions(Ticket handlingTicket, ICollection<BusinessRuleConditionCustom> businessRuleConditionCustomList)
        {
            int highestLevel = businessRuleConditionCustomList.Aggregate((i1, i2) => i1.BusinessRuleCondition.BusinessRuleConditionLevel > i2.BusinessRuleCondition.BusinessRuleConditionLevel ? i1 : i2).BusinessRuleCondition.BusinessRuleConditionLevel.Value;
            bool isSatisfied = false;
            while (highestLevel > 0)
            {
                ICollection<BusinessRuleConditionCustom> items = businessRuleConditionCustomList
                    .GroupBy(m => m.BusinessRuleCondition.BusinessRuleConditionID)
                    .Select(m => m.First())
                    .Where(m => m.BusinessRuleCondition.BusinessRuleConditionLevel == highestLevel).ToList();
                foreach (BusinessRuleConditionCustom item in items)
                {
                    BusinessRuleConditionCustom parentItem = businessRuleConditionCustomList.Where(m => m.BusinessRuleCondition.ID == item.BusinessRuleCondition.BusinessRuleConditionID).FirstOrDefault();
                    bool isSatisfiedOfRelevantItems = true;
                    if (parentItem != null)
                    {
                        isSatisfiedOfRelevantItems = parentItem.IsSatisfied;
                    }
                    IEnumerable<BusinessRuleConditionCustom> relevantItems = businessRuleConditionCustomList
                        .Where(m => m.BusinessRuleCondition.BusinessRuleConditionLevel == item.BusinessRuleCondition.BusinessRuleConditionLevel
                        && m.BusinessRuleCondition.BusinessRuleConditionID == item.BusinessRuleCondition.BusinessRuleConditionID)
                        .OrderBy(m => m.BusinessRuleCondition.ID);
                    foreach (BusinessRuleConditionCustom relevantItem in relevantItems)
                    {
                        switch (relevantItem.BusinessRuleCondition.Type)
                        {
                            case ConstantUtil.TypeOfBusinessRuleCondition.Or:
                                isSatisfiedOfRelevantItems = isSatisfiedOfRelevantItems || relevantItem.IsSatisfied;
                                break;
                            case ConstantUtil.TypeOfBusinessRuleCondition.And:
                            default:
                                isSatisfiedOfRelevantItems = isSatisfiedOfRelevantItems && relevantItem.IsSatisfied;
                                break;
                        }
                        businessRuleConditionCustomList.Remove(relevantItem);
                    }
                    if (parentItem != null)
                    {
                        parentItem.IsSatisfied = isSatisfiedOfRelevantItems;
                    }
                    else
                    {
                        isSatisfied = isSatisfiedOfRelevantItems;
                    }
                }
                highestLevel--;
            }
            return isSatisfied;
        }

        private bool IsSatisfiedWithCondition(Ticket handlingTicket, BusinessRuleCondition businessRuleCondition)
        {
            bool isSatisfied = false;
            switch (businessRuleCondition.Criteria)
            {
                case ConstantUtil.CriteriaOfBusinessRuleCondition.Subject:
                    switch (businessRuleCondition.Condition)
                    {
                        case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                            if (handlingTicket.Subject.Equals(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                            if (!handlingTicket.Subject.Equals(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.BeginsWith:
                            if (handlingTicket.Subject.StartsWith(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.EndsWith:
                            if (handlingTicket.Subject.EndsWith(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.Contains:
                            if (handlingTicket.Subject.Contains(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.DoesNotContain:
                            if (!handlingTicket.Subject.Contains(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                    }
                    break;
                case ConstantUtil.CriteriaOfBusinessRuleCondition.Description:
                    switch (businessRuleCondition.Condition)
                    {
                        case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                            if (handlingTicket.Description.Equals(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                            if (!handlingTicket.Description.Equals(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.BeginsWith:
                            if (handlingTicket.Description.StartsWith(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.EndsWith:
                            if (handlingTicket.Description.EndsWith(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.Contains:
                            if (handlingTicket.Description.Contains(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.DoesNotContain:
                            if (!handlingTicket.Description.Contains(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                    }
                    break;
                case ConstantUtil.CriteriaOfBusinessRuleCondition.RequesterName:
                    AspNetUser requester = _unitOfWork.AspNetUserRepository.GetByID(handlingTicket.RequesterID);
                    if (requester != null)
                    {
                        string requesterName = requester.Fullname;
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                if (requesterName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                if (!requesterName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.CriteriaOfBusinessRuleCondition.Department:
                    AspNetUser technician = _unitOfWork.AspNetUserRepository.GetByID(handlingTicket.TechnicianID);
                    if (technician != null)
                    {
                        Department department = technician.Department;
                        if (department != null)
                        {
                            string departmentName = department.Name;
                            switch (businessRuleCondition.Condition)
                            {
                                case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                    if (departmentName.Equals(businessRuleCondition.Value))
                                    {
                                        isSatisfied = true;
                                    }
                                    break;
                                case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                    if (!departmentName.Equals(businessRuleCondition.Value))
                                    {
                                        isSatisfied = true;
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case ConstantUtil.CriteriaOfBusinessRuleCondition.Priority:
                    Priority priority = handlingTicket.Priority;
                    if (priority != null)
                    {
                        string priorityName = priority.Name;
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                if (priorityName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                if (!priorityName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.CriteriaOfBusinessRuleCondition.Impact:
                    Impact impact = handlingTicket.Impact;
                    if (impact != null)
                    {
                        string impactName = impact.Name;
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                if (impactName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                if (!impactName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.CriteriaOfBusinessRuleCondition.Urgency:
                    Urgency urgency = handlingTicket.Urgency;
                    if (urgency != null)
                    {
                        string urgencyName = urgency.Name;
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                if (urgencyName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                if (!urgencyName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.CriteriaOfBusinessRuleCondition.Mode:
                    string mode = ConstantUtil.TicketModeString.WebForm;
                    if (handlingTicket.Mode == ConstantUtil.TicketMode.Email)
                    {
                        mode = ConstantUtil.TicketModeString.Email;
                    }
                    else if (handlingTicket.Mode == ConstantUtil.TicketMode.PhoneCall)
                    {
                        mode = ConstantUtil.TicketModeString.PhoneCall;
                    }
                    switch (businessRuleCondition.Condition)
                    {
                        case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                            if (mode.Equals(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                            if (!mode.Equals(businessRuleCondition.Value))
                            {
                                isSatisfied = true;
                            }
                            break;
                    }
                    break;
                case ConstantUtil.CriteriaOfBusinessRuleCondition.Category:
                    Category category = handlingTicket.Category;
                    if (category != null)
                    {
                        string categoryName = category.Name;
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                if (categoryName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                if (!categoryName.Equals(businessRuleCondition.Value))
                                {
                                    isSatisfied = true;
                                }
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
            return isSatisfied;
        }

        public void AddTicket(Ticket ticket)
        {
            try
            {
                _unitOfWork.TicketRepository.Insert(ticket);
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
        }

        public bool UpdateTicket(Ticket ticket)
        {
            _unitOfWork.TicketRepository.Update(ticket);
            return _unitOfWork.Save() > 0;
        }

        public void CancelTicket(Ticket ticket)
        {
            ticket.Status = ConstantUtil.TicketStatus.Cancelled;
            ticket.ModifiedTime = DateTime.Now;
            try
            {
                _unitOfWork.TicketRepository.Update(ticket);
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
        }

        public void SolveTicket(Ticket ticket)
        {
            ticket.Status = ConstantUtil.TicketStatus.Solved; //Solved
            _unitOfWork.TicketRepository.Update(ticket);
            _unitOfWork.Save();
        }

        public IEnumerable<Ticket> GetTechnicianTickets(string id)
        {
            return _unitOfWork.TicketRepository.Get(m => m.TechnicianID == id);
        }

        public IEnumerable<Ticket> GetRequesterTickets(string id)
        {
            return _unitOfWork.TicketRepository.Get(m => m.RequesterID == id);
        }

        public void CloseTicket(Ticket ticket)
        {
            ticket.Status = ConstantUtil.TicketStatus.Closed;
            ticket.ModifiedTime = DateTime.Now;
            try
            {
                _unitOfWork.TicketRepository.Update(ticket);
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Ticket> GetPendingTickets()
        {
            return _unitOfWork.TicketRepository.Get(m => m.Status != ConstantUtil.TicketStatus.Cancelled
                && m.Status != ConstantUtil.TicketStatus.Closed);
        }
    }
}