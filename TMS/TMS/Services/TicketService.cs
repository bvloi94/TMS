﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.Ajax.Utilities;
using TMS.Class;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;
using TMS.ViewModels;

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

            if (handlingTicket != null)
            {

                //Handle automation job
                bool isSatisfied = false;
                IEnumerable<BusinessRule> businessRules = _unitOfWork.BusinessRuleRepository.Get(m => m.IsActive == true);
                foreach (BusinessRule businessRule in businessRules)
                {
                    IEnumerable<BusinessRuleTrigger> triggers = businessRule.BusinessRuleTriggers;
                    var enable = false;
                    if (businessRule.EnableRule != null)
                        enable = (bool)businessRule.EnableRule;
                    bool triggerEnable = true;
                    if (enable)
                    {
                        List<BusinessRuleConditionCustom> businessRuleConditionCustomList =
                            new List<BusinessRuleConditionCustom>();
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
                        if (!isSatisfied)
                        {
                            triggerEnable = false;
                        }
                    }
                    if (triggerEnable)
                    {
                        foreach (BusinessRuleTrigger trigger in triggers)
                        {
                            switch (trigger.Action)
                            {
                                case ConstantUtil.BusinessRuleTrigger.AssignToTechnician:
                                    var technician = _unitOfWork.AspNetUserRepository.GetByID(trigger.Value);
                                    if (technician != null && string.IsNullOrEmpty(handlingTicket.TechnicianID))
                                    {
                                        handlingTicket.TechnicianID = technician.Id;
                                    }
                                    break;
                                case ConstantUtil.BusinessRuleTrigger.MoveToCategory:
                                case ConstantUtil.BusinessRuleTrigger.MoveToSubCategory:
                                case ConstantUtil.BusinessRuleTrigger.MoveToItem:
                                    int categoryId = TMSUtils.StrToIntDef(trigger.Value, 0);
                                    if (categoryId > 0 && !handlingTicket.CategoryID.HasValue)
                                    {
                                        handlingTicket.CategoryID = categoryId;
                                    }
                                    break;
                                case ConstantUtil.BusinessRuleTrigger.SetPriorityAs:
                                    int priorityId = TMSUtils.StrToIntDef(trigger.Value, 0);
                                    if (priorityId > 0 && !handlingTicket.PriorityID.HasValue)
                                    {
                                        handlingTicket.PriorityID = priorityId;
                                    }
                                    break;
                            }
                        }
                    }

                    IEnumerable<BusinessRuleNotification> notifications = businessRule.BusinessRuleNotifications;
                    foreach (var noty in notifications)
                    {
                        var technician = _unitOfWork.AspNetUserRepository.GetByID(noty.ReceiverID);
                        if (technician != null)
                        {
                            Thread thread =
                                new Thread(() => EmailUtil.SendToTechnicianWhenBusinessRuleIsApplied(ticket, technician));
                            thread.Start();
                        }
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
            string[] values;
            switch (businessRuleCondition.Criteria)
            {
                case ConstantUtil.BusinessRuleCriteria.Subject:
                    if (!string.IsNullOrWhiteSpace(businessRuleCondition.Value))
                    {
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                if (handlingTicket.Subject.ToLower().Equals(businessRuleCondition.Value.ToLower()))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                if (!handlingTicket.Subject.ToLower().Equals(businessRuleCondition.Value.ToLower()))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.BeginsWith:
                                if (handlingTicket.Subject.ToLower().StartsWith(businessRuleCondition.Value.ToLower()))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.EndsWith:
                                if (handlingTicket.Subject.ToLower().EndsWith(businessRuleCondition.Value.ToLower()))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Contains:
                                if (handlingTicket.Subject.ToLower().Contains(businessRuleCondition.Value.ToLower()))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.DoesNotContain:
                                if (!handlingTicket.Subject.ToLower().Contains(businessRuleCondition.Value.ToLower()))
                                {
                                    return true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.BusinessRuleCriteria.Description:
                    if (!string.IsNullOrWhiteSpace(handlingTicket.Description))
                    {
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                if (handlingTicket.Description.Equals(businessRuleCondition.Value))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                if (!handlingTicket.Description.Equals(businessRuleCondition.Value))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.BeginsWith:
                                if (handlingTicket.Description.StartsWith(businessRuleCondition.Value))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.EndsWith:
                                if (handlingTicket.Description.EndsWith(businessRuleCondition.Value))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Contains:
                                if (handlingTicket.Description.Contains(businessRuleCondition.Value))
                                {
                                    return true;
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.DoesNotContain:
                                if (!handlingTicket.Description.Contains(businessRuleCondition.Value))
                                {
                                    return true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.BusinessRuleCriteria.RequesterName:
                    AspNetUser requester = _unitOfWork.AspNetUserRepository.GetByID(handlingTicket.RequesterID);
                    if (requester != null)
                    {
                        string requesterId = requester.Id;
                        values = businessRuleCondition.Value.Split(',');
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                foreach (string value in values)
                                {
                                    if (requesterId.Equals(value))
                                    {
                                        return true;
                                    }
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                bool result = true;
                                foreach (string value in values)
                                {
                                    if (requesterId.Equals(value))
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                                if (result)
                                {
                                    return true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.BusinessRuleCriteria.Group:
                    AspNetUser technician = _unitOfWork.AspNetUserRepository.GetByID(handlingTicket.TechnicianID);
                    if (technician != null)
                    {
                        Group group = technician.Group;
                        if (group != null)
                        {
                            int groupId = group.ID;
                            values = businessRuleCondition.Value.Split(',');
                            switch (businessRuleCondition.Condition)
                            {
                                case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                    foreach (string value in values)
                                    {
                                        int intVal = 0;
                                        Int32.TryParse(value, out intVal);
                                        if (groupId == intVal)
                                        {
                                            return true;
                                        }
                                    }
                                    break;
                                case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                    bool result = true;
                                    foreach (string value in values)
                                    {
                                        int intVal = 0;
                                        Int32.TryParse(value, out intVal);
                                        if (groupId == intVal)
                                        {
                                            result = false;
                                            break;
                                        }
                                    }
                                    if (result)
                                    {
                                        return true;
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case ConstantUtil.BusinessRuleCriteria.Priority:
                    Priority priority = handlingTicket.Priority;
                    if (priority != null)
                    {
                        int priorityId = priority.ID;
                        values = businessRuleCondition.Value.Split(',');
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                foreach (string value in values)
                                {
                                    int intVal = 0;
                                    Int32.TryParse(value, out intVal);
                                    if (priorityId == intVal)
                                    {
                                        return true;
                                    }
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                bool result = true;
                                foreach (string value in values)
                                {
                                    int intVal = 0;
                                    Int32.TryParse(value, out intVal);
                                    if (priorityId == intVal)
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                                if (result)
                                {
                                    return true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.BusinessRuleCriteria.Impact:
                    Impact impact = handlingTicket.Impact;
                    if (impact != null)
                    {
                        int impactId = impact.ID;
                        values = businessRuleCondition.Value.Split(',');
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                foreach (string value in values)
                                {
                                    int intVal = 0;
                                    Int32.TryParse(value, out intVal);
                                    if (impactId == intVal)
                                    {
                                        return true;
                                    }
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                bool result = true;
                                foreach (string value in values)
                                {
                                    int intVal = 0;
                                    Int32.TryParse(value, out intVal);
                                    if (impactId == intVal)
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                                if (result)
                                {
                                    return true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.BusinessRuleCriteria.Urgency:
                    Urgency urgency = handlingTicket.Urgency;
                    if (urgency != null)
                    {
                        int urgencyId = urgency.ID;
                        values = businessRuleCondition.Value.Split(',');
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                foreach (string value in values)
                                {
                                    int intVal = 0;
                                    Int32.TryParse(value, out intVal);
                                    if (urgencyId == intVal)
                                    {
                                        return true;
                                    }
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                bool result = true;
                                foreach (string value in values)
                                {
                                    int intVal = 0;
                                    Int32.TryParse(value, out intVal);
                                    if (urgencyId == intVal)
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                                if (result)
                                {
                                    return true;
                                }
                                break;
                        }
                    }
                    break;
                case ConstantUtil.BusinessRuleCriteria.Mode:
                    values = businessRuleCondition.Value.Split(',');
                    switch (businessRuleCondition.Condition)
                    {
                        case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                            foreach (string value in values)
                            {
                                int intVal = 0;
                                Int32.TryParse(value, out intVal);
                                if (handlingTicket.Mode == intVal)
                                {
                                    return true;
                                }
                            }
                            break;
                        case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                            bool result = true;
                            foreach (string value in values)
                            {
                                int intVal = 0;
                                Int32.TryParse(value, out intVal);
                                if (handlingTicket.Mode == intVal)
                                {
                                    result = false;
                                    break;
                                }
                            }
                            if (result)
                            {
                                return true;
                            }
                            break;
                    }
                    break;
                case ConstantUtil.BusinessRuleCriteria.Category:
                    Category category = handlingTicket.Category;
                    if (category != null)
                    {
                        int categoryId = category.ID;
                        values = businessRuleCondition.Value.Split(',');
                        switch (businessRuleCondition.Condition)
                        {
                            case ConstantUtil.ConditionOfBusinessRuleCondition.Is:
                                foreach (string value in values)
                                {
                                    int intVal = 0;
                                    Int32.TryParse(value, out intVal);
                                    if (categoryId == intVal)
                                    {
                                        return true;
                                    }
                                }
                                break;
                            case ConstantUtil.ConditionOfBusinessRuleCondition.IsNot:
                                bool result = true;
                                foreach (string value in values)
                                {
                                    int intVal = 0;
                                    Int32.TryParse(value, out intVal);
                                    if (categoryId == intVal)
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                                if (result)
                                {
                                    return true;
                                }
                                break;
                        }
                    }
                    break;
            }
            return false;
        }

        public IEnumerable<Ticket> GetRecentTickets(int timeOption)
        {
            IEnumerable<Ticket> tickets = _unitOfWork.TicketRepository.Get();
            IEnumerable<Ticket> recentTickets = tickets;
            switch (timeOption)
            {
                case ConstantUtil.TimeOption.Today:
                    recentTickets = recentTickets.Where(m => m.CreatedTime.Date >= DateTime.Now.Date);
                    break;
                case ConstantUtil.TimeOption.FourDaysAgo:
                    recentTickets = recentTickets.Where(m => m.CreatedTime.Date >= DateTime.Now.AddDays(-4).Date);
                    break;
                case ConstantUtil.TimeOption.ThisWeek:
                    int intervalWeek = 0;
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        intervalWeek = 6;
                    }
                    else
                    {
                        intervalWeek = (int)DateTime.Now.DayOfWeek - 1;
                    }
                    recentTickets = recentTickets.Where(m => m.CreatedTime.Date >= DateTime.Now.Date.AddDays(-intervalWeek).Date
                        && m.CreatedTime.Date <= DateTime.Now.Date);
                    break;
                case ConstantUtil.TimeOption.ThisMonth:
                    recentTickets = recentTickets.Where(m => m.CreatedTime.Year == DateTime.Now.Year && m.CreatedTime.Month == DateTime.Now.Month);
                    break;
                case ConstantUtil.TimeOption.ThisYear:
                    recentTickets = recentTickets.Where(m => m.CreatedTime.Year == DateTime.Now.Year);
                    break;
            }
            return recentTickets;
        }

        //send notification to requester
        //send notification to helpdesk if requester create ticket 
        //send notification to technician if ticket is assigned when creating
        public bool AddTicket(Ticket ticket)
        {
            ticket = ParseTicket(ticket);
            string ticketCode = GenerateTicketCode();
            if (!string.IsNullOrWhiteSpace(ticketCode))
            {
                ticket.Code = ticketCode;

                //send notification to requester
                Notification requesterNoti = new Notification();
                requesterNoti.IsForHelpDesk = false;
                requesterNoti.BeNotifiedID = ticket.RequesterID;
                requesterNoti.ActionType = ConstantUtil.NotificationActionType.RequesterNotiCreate;
                requesterNoti.NotifiedTime = DateTime.Now;
                ticket.Notifications.Add(requesterNoti);

                //send notification to technician if ticket is assigned when creating
                if (ticket.Status == ConstantUtil.TicketStatus.Assigned && ticket.AssignedByID != null)
                {
                    AspNetUser ticketAssigner = _unitOfWork.AspNetUserRepository.GetByID(ticket.AssignedByID);
                    if (ticketAssigner != null)
                    {
                        Notification technicianNoti = new Notification();
                        technicianNoti.IsForHelpDesk = false;
                        technicianNoti.BeNotifiedID = ticket.TechnicianID;
                        technicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiAssign;
                        technicianNoti.ActID = ticket.AssignedByID;
                        technicianNoti.NotifiedTime = DateTime.Now;
                        ticket.Notifications.Add(technicianNoti);
                    }
                }

                //send notification to helpdesk if requester create ticket 
                if (ticket.CreatedID != null)
                {
                    AspNetUser ticketCreator = _unitOfWork.AspNetUserRepository.GetByID(ticket.CreatedID);
                    if (ticketCreator != null)
                    {
                        string role = ticketCreator.AspNetRoles.FirstOrDefault().Name.ToLower();
                        if (role.Equals("requester"))
                        {
                            Notification helpdeskNoti = new Notification();
                            helpdeskNoti.IsForHelpDesk = true;
                            helpdeskNoti.ActionType = ConstantUtil.NotificationActionType.HelpDeskNotiCreate;
                            helpdeskNoti.ActID = ticket.RequesterID;
                            helpdeskNoti.NotifiedTime = DateTime.Now;
                            ticket.Notifications.Add(helpdeskNoti);
                        }
                    }
                }
                //end notification

                //start ticket history
                TicketHistory ticketHistory = new TicketHistory();
                ticketHistory.ActID = ticket.CreatedID;
                ticketHistory.ActedTime = DateTime.Now;
                ticketHistory.Type = ConstantUtil.TicketHistoryType.Created;
                ticketHistory.Action = "OPERATION: CREATE";
                ticket.TicketHistories.Add(ticketHistory);
                //end ticket history
                _unitOfWork.TicketRepository.Insert(ticket);
                return _unitOfWork.Commit();
            }
            else
            {
                return false;
            }
        }

        //send notification to technician when assigned
        //send notification to technician when unassigned
        public bool UpdateTicket(Ticket ticket, string actId)
        {
            Ticket oldTicket = _unitOfWork.TicketRepository.DbSet.AsNoTracking().Where(m => m.ID == ticket.ID).FirstOrDefault();
            if (oldTicket != null)
            {
                _unitOfWork.BeginTransaction();

                //send notification to technician when assigned
                if (oldTicket.Status == ConstantUtil.TicketStatus.Open && ticket.Status == ConstantUtil.TicketStatus.Assigned)
                {
                    AspNetUser ticketAssigner = _unitOfWork.AspNetUserRepository.GetByID(actId);
                    if (ticketAssigner != null)
                    {
                        Notification technicianNoti = new Notification();
                        technicianNoti.IsForHelpDesk = false;
                        technicianNoti.TicketID = ticket.ID;
                        technicianNoti.BeNotifiedID = ticket.TechnicianID;
                        technicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiAssign;
                        technicianNoti.ActID = actId;
                        technicianNoti.NotifiedTime = DateTime.Now;
                        _unitOfWork.NotificationRepository.Insert(technicianNoti);
                    }
                }
                //send notification to technician when unassigned
                else if (oldTicket.Status == ConstantUtil.TicketStatus.Assigned && ticket.Status == ConstantUtil.TicketStatus.Open)
                {
                    AspNetUser ticketUnassigner = _unitOfWork.AspNetUserRepository.GetByID(actId);
                    if (ticketUnassigner != null)
                    {
                        Notification technicianNoti = new Notification();
                        technicianNoti.IsForHelpDesk = false;
                        technicianNoti.TicketID = ticket.ID;
                        technicianNoti.BeNotifiedID = oldTicket.TechnicianID;
                        technicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiUnassign;
                        technicianNoti.ActID = actId;
                        technicianNoti.NotifiedTime = DateTime.Now;
                        _unitOfWork.NotificationRepository.Insert(technicianNoti);
                    }
                }
                else if (oldTicket.Status == ConstantUtil.TicketStatus.Assigned && ticket.Status == ConstantUtil.TicketStatus.Assigned)
                {
                    if (oldTicket.TechnicianID != ticket.TechnicianID)
                    {
                        AspNetUser ticketUnassigner = _unitOfWork.AspNetUserRepository.GetByID(actId);
                        if (ticketUnassigner != null)
                        {
                            //send notification to old technician
                            Notification oldTechnicianNoti = new Notification();
                            oldTechnicianNoti.IsForHelpDesk = false;
                            oldTechnicianNoti.TicketID = ticket.ID;
                            oldTechnicianNoti.BeNotifiedID = oldTicket.TechnicianID;
                            oldTechnicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiUnassign;
                            oldTechnicianNoti.ActID = actId;
                            oldTechnicianNoti.NotifiedTime = DateTime.Now;
                            _unitOfWork.NotificationRepository.Insert(oldTechnicianNoti);
                            //send notification to new technician
                            Notification newTechnicianNoti = new Notification();
                            newTechnicianNoti.IsForHelpDesk = false;
                            newTechnicianNoti.TicketID = ticket.ID;
                            newTechnicianNoti.BeNotifiedID = ticket.TechnicianID;
                            newTechnicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiAssign;
                            newTechnicianNoti.ActID = actId;
                            newTechnicianNoti.NotifiedTime = DateTime.Now;
                            _unitOfWork.NotificationRepository.Insert(newTechnicianNoti);
                        }
                    }
                    if (ticket.DueByDate.HasValue && oldTicket.DueByDate != ticket.DueByDate)
                    {
                        Notification newTechnicianNoti = new Notification();
                        newTechnicianNoti.IsForHelpDesk = false;
                        newTechnicianNoti.TicketID = ticket.ID;
                        newTechnicianNoti.BeNotifiedID = ticket.TechnicianID;
                        newTechnicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiChangeDueByDate;
                        newTechnicianNoti.ActID = actId;
                        newTechnicianNoti.NotifiedTime = DateTime.Now;
                        _unitOfWork.NotificationRepository.Insert(newTechnicianNoti);
                    }
                }
                //end notification

                TicketHistory ticketHistory = GetUpdatedTicketHistory(oldTicket, ticket, actId);
                if (ticketHistory != null)
                {
                    _unitOfWork.TicketHistoryRepository.Insert(ticketHistory);
                }
                _unitOfWork.TicketRepository.Update(ticket);
                return _unitOfWork.CommitTransaction();
            }
            else
            {
                return false;
            }
        }

        //send notification to helpdesk when requester unapprove ticket
        public bool ApproveTicket(Ticket ticket, string actId)
        {
            ticket.Status = ConstantUtil.TicketStatus.Unapproved;
            ticket.ModifiedTime = DateTime.Now;
            _unitOfWork.BeginTransaction();

            //send notification to helpdesk when requester unapprove ticket
            AspNetUser ticketUnapprovedUser = _unitOfWork.AspNetUserRepository.GetByID(actId);
            if (ticketUnapprovedUser != null)
            {
                Notification helpdeskNoti = new Notification();
                helpdeskNoti.IsForHelpDesk = true;
                helpdeskNoti.TicketID = ticket.ID;
                helpdeskNoti.ActionType = ConstantUtil.NotificationActionType.HelpDeskNotiUnapprove;
                helpdeskNoti.ActID = actId;
                //helpdeskNoti.NotificationContent = string.Format("Ticket '{0}'[#{1}] was unapproved by {2}", ticket.Subject, ticket.Code, ticketUnapprovedUser.Fullname);
                helpdeskNoti.NotifiedTime = DateTime.Now;
                _unitOfWork.NotificationRepository.Insert(helpdeskNoti);
            }
            //end notification

            //start ticket history
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.TicketID = ticket.ID;
            ticketHistory.ActID = actId;
            ticketHistory.ActedTime = DateTime.Now;
            ticketHistory.Type = ConstantUtil.TicketHistoryType.Unapproved;
            ticketHistory.Action = "OPERATION: APPROVE";
            _unitOfWork.TicketHistoryRepository.Insert(ticketHistory);
            //end ticket history
            _unitOfWork.TicketRepository.Update(ticket);
            return _unitOfWork.CommitTransaction();
        }

        //send notification to requester
        //send notification to technician if ticket is assigned to him
        public bool CancelTicket(Ticket ticket, string actId)
        {
            _unitOfWork.BeginTransaction();
            int status = ticket.Status;
            ticket.Status = ConstantUtil.TicketStatus.Cancelled;
            ticket.ModifiedTime = DateTime.Now;

            //send notification to requester
            Notification requesterNoti = new Notification();
            requesterNoti.IsForHelpDesk = false;
            requesterNoti.TicketID = ticket.ID;
            requesterNoti.BeNotifiedID = ticket.RequesterID;
            requesterNoti.ActionType = ConstantUtil.NotificationActionType.RequesterNotiCancel;
            //requesterNoti.NotificationContent = string.Format("Ticket '{0}'[#{1}] was cancelled.", ticket.Subject, ticket.Code);
            requesterNoti.NotifiedTime = DateTime.Now;
            _unitOfWork.NotificationRepository.Insert(requesterNoti);

            //send notification to technician if ticket is assigned to him
            AspNetUser ticketCancelledUser = _unitOfWork.AspNetUserRepository.GetByID(actId);
            if (ticketCancelledUser != null)
            {
                if (status == ConstantUtil.TicketStatus.Assigned)
                {
                    Notification technicianNoti = new Notification();
                    technicianNoti.IsForHelpDesk = false;
                    technicianNoti.TicketID = ticket.ID;
                    technicianNoti.BeNotifiedID = ticket.TechnicianID;
                    technicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiCancel;
                    technicianNoti.ActID = actId;
                    //technicianNoti.NotificationContent = string.Format("Ticket '{0}'[#{1}] was cancelled by {2}.", ticket.Subject, ticket.Code, ticketCancelledUser.Fullname);
                    technicianNoti.NotifiedTime = DateTime.Now;
                    _unitOfWork.NotificationRepository.Insert(technicianNoti);
                }
            }
            //end notification

            //start ticket history
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.TicketID = ticket.ID;
            ticketHistory.Type = ConstantUtil.TicketHistoryType.Cancelled;
            ticketHistory.ActID = actId;
            ticketHistory.Action = "OPERATION: CANCEL";
            ticketHistory.ActedTime = DateTime.Now;
            //end ticket history

            _unitOfWork.TicketHistoryRepository.Insert(ticketHistory);

            _unitOfWork.TicketRepository.Update(ticket);
            return _unitOfWork.CommitTransaction();
        }

        //send notification to requester
        public bool SolveTicket(Ticket ticket, string actId)
        {
            ticket.Status = ConstantUtil.TicketStatus.Solved;
            ticket.ModifiedTime = DateTime.Now;
            _unitOfWork.BeginTransaction();

            //send notification to requester
            Notification requesterNoti = new Notification();
            requesterNoti.IsForHelpDesk = false;
            requesterNoti.TicketID = ticket.ID;
            requesterNoti.BeNotifiedID = ticket.RequesterID;
            requesterNoti.ActionType = ConstantUtil.NotificationActionType.RequesterNotiSolve;
            //requesterNoti.NotificationContent = string.Format("Ticket '{0}'[#{1}] was solved.", ticket.Subject, ticket.Code);
            requesterNoti.NotifiedTime = DateTime.Now;
            _unitOfWork.NotificationRepository.Insert(requesterNoti);
            //end notification
            //start ticket history
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.TicketID = ticket.ID;
            ticketHistory.Type = ConstantUtil.TicketHistoryType.Solved;
            ticketHistory.ActID = actId;
            ticketHistory.Action = "OPERATION: SOLVE";
            ticketHistory.ActedTime = DateTime.Now;
            //end ticket history
            _unitOfWork.TicketHistoryRepository.Insert(ticketHistory);
            _unitOfWork.TicketRepository.Update(ticket);
            return _unitOfWork.CommitTransaction();
        }

        public IEnumerable<Ticket> GetTechnicianTickets(string id)
        {
            return _unitOfWork.TicketRepository.Get(m => m.TechnicianID == id);
        }

        public IEnumerable<Ticket> GetRequesterTickets(string id)
        {
            return _unitOfWork.TicketRepository.Get(m => m.RequesterID == id);
        }

        public string GenerateTicketCode()
        {
            bool duplicated = true;
            string sample = ConstantUtil.TicketCodeTemplate.NumberTemplate;
            Random rnd = new Random();
            int size;
            int num;
            string code = "";
            while (duplicated)
            {
                size = ConstantUtil.TicketCodeTemplate.Length;
                code = ConstantUtil.TicketCodeTemplate.FirstLetter;
                for (int i = 0; i < size - ConstantUtil.TicketCodeTemplate.FirstLetter.Length; i++)
                {
                    num = rnd.Next(0, sample.Length);
                    code += sample[num];
                }
                duplicated = _unitOfWork.TicketRepository.Get(m => m.Code == code).Any();
            }
            return code;
        }

        //send notification to requester
        public bool CloseTicket(Ticket ticket, string actId)
        {
            ticket.Status = ConstantUtil.TicketStatus.Closed;
            ticket.ModifiedTime = DateTime.Now;
            _unitOfWork.BeginTransaction();

            //send notification to requester
            Notification requesterNoti = new Notification();
            requesterNoti.IsForHelpDesk = false;
            requesterNoti.TicketID = ticket.ID;
            requesterNoti.BeNotifiedID = ticket.RequesterID;
            requesterNoti.ActionType = ConstantUtil.NotificationActionType.RequesterNotiClose;
            //requesterNoti.NotificationContent = string.Format("Ticket '{0}'[#{1}] was closed.", ticket.Subject, ticket.Code);
            requesterNoti.NotifiedTime = DateTime.Now;
            _unitOfWork.NotificationRepository.Insert(requesterNoti);
            //end notification

            //start ticket history
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.TicketID = ticket.ID;
            ticketHistory.Type = ConstantUtil.TicketHistoryType.Closed;
            ticketHistory.ActID = actId;
            ticketHistory.Action = "OPERATION: CLOSE";
            ticketHistory.ActedTime = DateTime.Now;
            //end ticket history
            _unitOfWork.TicketHistoryRepository.Insert(ticketHistory);
            _unitOfWork.TicketRepository.Update(ticket);
            return _unitOfWork.CommitTransaction();
        }

        //send notification to requester
        public bool ReassignTicket(Ticket ticket, string actId)
        {
            ticket.Status = ConstantUtil.TicketStatus.Assigned;
            ticket.ModifiedTime = DateTime.Now;
            _unitOfWork.BeginTransaction();

            AspNetUser reassignUser = _unitOfWork.AspNetUserRepository.GetByID(actId);
            if (reassignUser != null)
            {
                //send notification to technician
                Notification technicianNoti = new Notification();
                technicianNoti.IsForHelpDesk = false;
                technicianNoti.TicketID = ticket.ID;
                technicianNoti.BeNotifiedID = ticket.TechnicianID;
                technicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiReassign;
                technicianNoti.ActID = actId;
                //technicianNoti.NotificationContent = string.Format("Ticket '{0}'[#{1}] was reassigned by {2}.", ticket.Subject, ticket.Code, reassignUser.Fullname);
                technicianNoti.NotifiedTime = DateTime.Now;
                _unitOfWork.NotificationRepository.Insert(technicianNoti);
                //end notification
            }

            //start ticket history
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.TicketID = ticket.ID;
            ticketHistory.Type = ConstantUtil.TicketHistoryType.Reassigned;
            ticketHistory.ActID = actId;
            ticketHistory.Action = "OPERATION: REASSIGN";
            ticketHistory.ActedTime = DateTime.Now;
            //end ticket history
            _unitOfWork.TicketHistoryRepository.Insert(ticketHistory);
            _unitOfWork.TicketRepository.Update(ticket);
            return _unitOfWork.CommitTransaction();
        }

        //send notification to technician
        //send notification to requester
        public bool MergeTicket(List<Ticket> tickets, string actId)
        {
            UserService _userService = new UserService(_unitOfWork);
            tickets.Sort((x, y) => DateTime.Compare(x.CreatedTime, y.CreatedTime));
            Ticket newTicket = tickets[0];
            _unitOfWork.BeginTransaction();
            for (int i = 1; i < tickets.Count; i++)
            {
                Ticket oldTicket = tickets[i];
                if (!string.IsNullOrWhiteSpace(newTicket.Description))
                {
                    newTicket.Description += string.Format("\n\n[Merged from ticket '{0}' (#{1})]:\n", oldTicket.Subject, oldTicket.Code) + oldTicket.Description;
                }
                else
                {
                    newTicket.Description = string.Format("[Merged from ticket '{0}' (#{1})]:\n", oldTicket.Subject, oldTicket.Code) + oldTicket.Description;
                }
                oldTicket.ModifiedTime = DateTime.Now;
                int status = oldTicket.Status;
                oldTicket.Status = ConstantUtil.TicketStatus.Cancelled;
                oldTicket.MergedID = newTicket.ID;
                //start ticket history
                TicketHistory oldTicketHistory = new TicketHistory();
                oldTicketHistory.TicketID = oldTicket.ID;
                oldTicketHistory.Type = ConstantUtil.TicketHistoryType.Merged;
                oldTicketHistory.ActID = actId;
                oldTicketHistory.Action = string.Format("Merged into ticket '{0}'[#{1}]", newTicket.Subject, newTicket.Code);
                oldTicketHistory.ActedTime = DateTime.Now;
                foreach (TicketAttachment ticketAttachment in oldTicket.TicketAttachments.ToList())
                {
                    if (ticketAttachment.Type == ConstantUtil.TicketAttachmentType.Description)
                    {
                        TicketAttachment newTicketAttachment = new TicketAttachment();
                        newTicketAttachment.TicketID = newTicket.ID;
                        newTicketAttachment.Filename = ticketAttachment.Filename;
                        newTicketAttachment.Path = ticketAttachment.Path;
                        newTicketAttachment.Type = ticketAttachment.Type;
                        _unitOfWork.TicketAttachmentRepository.Insert(newTicketAttachment);
                    }
                }
                //end ticket history
                _unitOfWork.TicketHistoryRepository.Insert(oldTicketHistory);
                _unitOfWork.TicketRepository.Update(oldTicket);

                if (status == ConstantUtil.TicketStatus.Assigned)
                {
                    AspNetUser mergedUser = _userService.GetUserById(actId);
                    if (mergedUser != null)
                    {
                        Notification technicianNoti = new Notification();
                        technicianNoti.IsForHelpDesk = false;
                        technicianNoti.TicketID = oldTicket.ID;
                        technicianNoti.BeNotifiedID = oldTicket.TechnicianID;
                        technicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiIsMerged;
                        technicianNoti.ActID = actId;
                        //technicianNoti.NotificationContent = string.Format("Ticket #{0} was merged into ticket #{1} by {2}.", oldTicket.Code, newTicket.Code, mergedUser.Fullname);
                        technicianNoti.NotifiedTime = DateTime.Now;
                        _unitOfWork.NotificationRepository.Insert(technicianNoti);
                    }

                    AspNetUser technician = _userService.GetUserById(oldTicket.TechnicianID);
                    Thread thread = new Thread(() => EmailUtil.SendToTechnicianWhenCancelTicket(oldTicket, technician));
                    thread.Start();
                }

                Notification requesterNoti = new Notification();
                requesterNoti.IsForHelpDesk = false;
                requesterNoti.TicketID = oldTicket.ID;
                requesterNoti.BeNotifiedID = oldTicket.RequesterID;
                requesterNoti.ActionType = ConstantUtil.NotificationActionType.RequesterNotiIsMerged;
                //requesterNoti.NotificationContent = string.Format("Ticket #{0} was merged into ticket #{1}.", oldTicket.Code, newTicket.Code);
                requesterNoti.NotifiedTime = DateTime.Now;
                _unitOfWork.NotificationRepository.Insert(requesterNoti);
            }

            if (newTicket.Status == ConstantUtil.TicketStatus.Assigned)
            {
                AspNetUser mergedUser = _userService.GetUserById(actId);
                if (mergedUser != null)
                {
                    Notification technicianNoti = new Notification();
                    technicianNoti.IsForHelpDesk = false;
                    technicianNoti.TicketID = newTicket.ID;
                    technicianNoti.BeNotifiedID = newTicket.TechnicianID;
                    technicianNoti.ActionType = ConstantUtil.NotificationActionType.TechnicianNotiMerge;
                    technicianNoti.ActID = actId;
                    //technicianNoti.NotificationContent = string.Format("Ticket #{0} was merged by {1}.", newTicket.Code, mergedUser.Fullname);
                    technicianNoti.NotifiedTime = DateTime.Now;
                    _unitOfWork.NotificationRepository.Insert(technicianNoti);
                }
            }
            //start ticket history
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.TicketID = newTicket.ID;
            ticketHistory.Type = ConstantUtil.TicketHistoryType.Merged;
            ticketHistory.ActID = actId;
            ticketHistory.Action = "OPERATION: MERGE";
            ticketHistory.ActedTime = DateTime.Now;
            //end ticket history
            newTicket.ModifiedTime = DateTime.Now;
            _unitOfWork.TicketHistoryRepository.Insert(ticketHistory);
            _unitOfWork.TicketRepository.Update(newTicket);
            return _unitOfWork.CommitTransaction();
        }

        public IEnumerable<Ticket> GetPendingTickets()
        {
            return _unitOfWork.TicketRepository.Get(m => m.Status != ConstantUtil.TicketStatus.Cancelled
                && m.Status != ConstantUtil.TicketStatus.Closed);
        }

        public IEnumerable<Ticket> GetOlderTickets()
        {
            return _unitOfWork.TicketRepository.Get(m => m.Status == ConstantUtil.TicketStatus.Solved
                || m.Status == ConstantUtil.TicketStatus.Closed);
        }

        public IEnumerable<FrequentlyAskedTicketViewModel> GetFrequentlyAskedSubjects(IEnumerable<Ticket> tickets)
        {
            //IEnumerable<FrequentlyAskedTicketViewModel> results = tickets.Where(m => m.Tags != null && m.Tags.Trim() != string.Empty)
            //    .GroupBy(m => m.Tags).Select(m => new FrequentlyAskedTicketViewModel
            //    {
            //        Tags = m.Key,
            //        Count = m.Count()
            //    }).OrderByDescending(m => m.Count);

            tickets = tickets.OrderByDescending(m => GeneralUtil.GetNumberOfTags(m.Tags));
            List<FrequentlyAskedTicketViewModel> result = new List<FrequentlyAskedTicketViewModel>();
            List<Ticket> temp = tickets.ToList();
            foreach (Ticket compareTicket in temp)
            {
                int count = 0;
                if (!string.IsNullOrWhiteSpace(compareTicket.Tags))
                {
                    string[] tagArr = compareTicket.Tags.Split(',');
                    int numOfTags = tagArr.Count();
                    if (result.Where(m => m.Tags == compareTicket.Tags).Any())
                    {
                        continue;
                    }
                    foreach (Ticket remainingTicket in temp)
                    {
                        int matchTag = 0;
                        //string itemTags = string.IsNullOrWhiteSpace(remainingTicket.Tags) ? "" : remainingTicket.Tags;
                        //int itemNumOfTags = GeneralUtil.GetNumberOfTags(itemTags);
                        //if (itemNumOfTags >= numOfTags && compareTicket.Tags != remainingTicket.Tags)
                        //{
                        //    continue;
                        //}
                        if (!string.IsNullOrWhiteSpace(remainingTicket.Tags))
                        {
                            foreach (string tag in tagArr)
                            {
                                if (remainingTicket.Tags.Contains(tag))
                                {
                                    matchTag++;
                                }
                            }
                        }
                        if (numOfTags <= 3)
                        {
                            if (matchTag == numOfTags)
                            {
                                count++;
                            }
                        }
                        else if (3 < numOfTags && numOfTags <= 5)
                        {
                            if (matchTag >= numOfTags - 1 && matchTag <= numOfTags + 1)
                            {
                                count++;
                            }
                        }
                        else
                        {
                            if (matchTag >= numOfTags - 2 && matchTag <= numOfTags + 2)
                            {
                                count++;
                            }
                        }
                    }
                    FrequentlyAskedTicketViewModel frequentlyAskedTicket = new FrequentlyAskedTicketViewModel();
                    frequentlyAskedTicket.Tags = compareTicket.Tags;
                    frequentlyAskedTicket.Count = count;
                    result.Add(frequentlyAskedTicket);
                }
            }

            return result;
        }

        public int? GetPriority(int? impactId, int? urgencyId, int? priorityId)
        {
            if (priorityId.HasValue)
            {
                return priorityId;
            }
            else
            {
                if (impactId.HasValue && urgencyId.HasValue)
                {
                    PriorityMatrixItem item = _unitOfWork.PriorityMatrixItemRepository.Get(m => m.ImpactID == impactId.Value
                        && m.UrgencyID == urgencyId.Value).FirstOrDefault();
                    if (item != null)
                    {
                        return item.PriorityID;
                    }
                }
            }
            return null;
        }

        public IEnumerable<Ticket> GetMergedTickets(int id)
        {
            return _unitOfWork.TicketRepository.Get(m => m.MergedID == id);
        }

        public TicketHistory GetUpdatedTicketHistory(Ticket oldTicket, Ticket newTicket, string actId)
        {
            TicketHistory result = new TicketHistory();
            result.TicketID = oldTicket.ID;
            result.ActID = actId;
            result.ActedTime = DateTime.Now;
            result.Type = ConstantUtil.TicketHistoryType.Updated;

            StringBuilder sb = new StringBuilder();
            //status
            if (oldTicket.Status != newTicket.Status)
            {
                sb.Append(string.Format(@"<p>Status changed from <b>{0}</b> to <b>{1}</b></p>", GeneralUtil.GetTicketStatusByID(oldTicket.Status), GeneralUtil.GetTicketStatusByID(newTicket.Status)));
            }
            //impact
            if (oldTicket.ImpactID != null || newTicket.ImpactID != null)
            {
                if (oldTicket.ImpactID != newTicket.ImpactID)
                {
                    Impact oldImpact = _unitOfWork.ImpactRepository.GetByID(oldTicket.ImpactID);
                    Impact newImpact = _unitOfWork.ImpactRepository.GetByID(newTicket.ImpactID);
                    string oldImpactName = "Unassigned";
                    string newImpactName = "Unassigned";
                    if (oldImpact != null)
                    {
                        oldImpactName = oldImpact.Name;
                    }
                    if (newImpact != null)
                    {
                        newImpactName = newImpact.Name;
                    }
                    sb.Append(string.Format(@"<p>Impact changed from <b>{0}</b> to <b>{1}</b></p>", oldImpactName, newImpactName));
                }
            }
            //impact detail
            if (oldTicket.ImpactDetail != null || newTicket.ImpactDetail != null)
            {
                if (oldTicket.ImpactDetail != newTicket.ImpactDetail)
                {
                    string oldImpactDetail = "None";
                    string newImpactDetail = "None";
                    if (!string.IsNullOrWhiteSpace(oldTicket.ImpactDetail))
                    {
                        oldImpactDetail = oldTicket.ImpactDetail;
                    }
                    if (!string.IsNullOrWhiteSpace(newTicket.ImpactDetail))
                    {
                        newImpactDetail = newTicket.ImpactDetail;
                    }
                    sb.Append(string.Format("<p>Impact Detail changed from <b>{0}</b> to <b>{1}</b></p>", oldImpactDetail, newImpactDetail));
                }
            }
            //urgency
            if (oldTicket.UrgencyID != null || newTicket.UrgencyID != null)
            {
                if (oldTicket.UrgencyID != newTicket.UrgencyID)
                {
                    Urgency oldUrgency = _unitOfWork.UrgencyRepository.GetByID(oldTicket.UrgencyID);
                    Urgency newUrgency = _unitOfWork.UrgencyRepository.GetByID(newTicket.UrgencyID);
                    string oldUrgencyName = "Unassigned";
                    string newUrgencyName = "Unassigned";
                    if (oldUrgency != null)
                    {
                        oldUrgencyName = oldUrgency.Name;
                    }
                    if (newUrgency != null)
                    {
                        newUrgencyName = newUrgency.Name;
                    }
                    sb.Append(string.Format("<p>Urgency changed from <b>{0}</b> to <b>{1}</b></p>", oldUrgencyName, newUrgencyName));
                }
            }
            //priority
            if (oldTicket.PriorityID != null || newTicket.PriorityID != null)
            {
                if (oldTicket.PriorityID != newTicket.PriorityID)
                {
                    Priority oldPriority = _unitOfWork.PriorityRepository.GetByID(oldTicket.PriorityID);
                    Priority newPriority = _unitOfWork.PriorityRepository.GetByID(newTicket.PriorityID);
                    string oldPriorityName = "Unassigned";
                    string newPriorityName = "Unassigned";
                    if (oldPriority != null)
                    {
                        oldPriorityName = oldPriority.Name;
                    }
                    if (newPriority != null)
                    {
                        newPriorityName = newPriority.Name;
                    }
                    sb.Append(string.Format("<p>Priority changed from <b>{0}</b> to <b>{1}</b></p>", oldPriorityName, newPriorityName));
                }
            }
            //category
            if (oldTicket.CategoryID != null || newTicket.CategoryID != null)
            {
                if (oldTicket.CategoryID != newTicket.CategoryID)
                {
                    Category oldCategory = _unitOfWork.CategoryRepository.GetByID(oldTicket.CategoryID);
                    Category newCategory = _unitOfWork.CategoryRepository.GetByID(newTicket.CategoryID);
                    string oldCategoryName = "Unassigned";
                    string newCategoryName = "Unassigned";
                    CategoryService _categoryService = new CategoryService(_unitOfWork);
                    if (oldCategory != null)
                    {
                        oldCategoryName = _categoryService.GetCategoryPath(oldCategory);
                    }
                    if (newCategory != null)
                    {
                        newCategoryName = _categoryService.GetCategoryPath(newCategory);
                    }
                    sb.Append(string.Format("<p>Category changed from <b>{0}</b> to <b>{1}</b></p>", oldCategoryName, newCategoryName));
                }
            }
            //type
            if (oldTicket.Type != null || newTicket.Type != null)
            {
                if (oldTicket.Type != newTicket.Type)
                {
                    string oldTypeName = GeneralUtil.GetTypeNameByType(oldTicket.Type);
                    string newTypeName = GeneralUtil.GetTypeNameByType(newTicket.Type);
                    sb.Append(string.Format("<p>Type changed from <b>{0}</b> to <b>{1}</b></p>", oldTypeName, newTypeName));
                }
            }
            //mode
            if (oldTicket.Mode != newTicket.Mode)
            {
                string oldModeName = GeneralUtil.GetModeNameByMode(oldTicket.Mode);
                string newModeName = GeneralUtil.GetModeNameByMode(newTicket.Mode);
                sb.Append(string.Format("<p>Mode changed from <b>{0}</b> to <b>{1}</b></p>", oldModeName, newModeName));
            }
            //schedule start date
            if (oldTicket.ScheduleStartDate != null || newTicket.ScheduleStartDate != null)
            {
                if (oldTicket.ScheduleStartDate != newTicket.ScheduleStartDate)
                {
                    string oldDate = "Unassigned";
                    string newDate = "Unassigned";
                    if (oldTicket.ScheduleStartDate != null)
                    {
                        oldDate = oldTicket.ScheduleStartDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    if (newTicket.ScheduleStartDate != null)
                    {
                        newDate = newTicket.ScheduleStartDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    sb.Append(string.Format(@"<p>Schedule Start Date changed from <b>{0}</b> to <b>{1}</b></p>", oldDate, newDate));
                }
            }
            //schedule end date
            if (oldTicket.ScheduleEndDate != null || newTicket.ScheduleEndDate != null)
            {
                if (oldTicket.ScheduleEndDate != newTicket.ScheduleEndDate)
                {
                    string oldDate = "Unassigned";
                    string newDate = "Unassigned";
                    if (oldTicket.ScheduleEndDate != null)
                    {
                        oldDate = oldTicket.ScheduleEndDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    if (newTicket.ScheduleEndDate != null)
                    {
                        newDate = newTicket.ScheduleEndDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    sb.Append(string.Format(@"<p>Schedule End Date changed from <b>{0}</b> to <b>{1}</b></p>", oldDate, newDate));
                }
            }
            //actual start date
            if (oldTicket.ActualStartDate != null || newTicket.ActualStartDate != null)
            {
                if (oldTicket.ActualStartDate != newTicket.ActualStartDate)
                {
                    string oldDate = "Unassigned";
                    string newDate = "Unassigned";
                    if (oldTicket.ActualStartDate != null)
                    {
                        oldDate = oldTicket.ActualStartDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    if (newTicket.ActualStartDate != null)
                    {
                        newDate = newTicket.ActualStartDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    sb.Append(string.Format(@"<p>Actual Start Date changed from <b>{0}</b> to <b>{1}</b></p>", oldDate, newDate));
                }
            }
            //actual end date
            if (oldTicket.ActualEndDate != null || newTicket.ActualEndDate != null)
            {
                if (oldTicket.ActualEndDate != newTicket.ActualEndDate)
                {
                    string oldDate = "Unassigned";
                    string newDate = "Unassigned";
                    if (oldTicket.ActualEndDate != null)
                    {
                        oldDate = oldTicket.ActualEndDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    if (newTicket.ActualEndDate != null)
                    {
                        newDate = newTicket.ActualEndDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    sb.Append(string.Format(@"<p>Actual End Date changed from <b>{0}</b> to <b>{1}</b></p>", oldDate, newDate));
                }
            }
            //due by date
            if (oldTicket.DueByDate != null || newTicket.DueByDate != null)
            {
                if (oldTicket.DueByDate != newTicket.DueByDate)
                {
                    string oldDate = "Unassigned";
                    string newDate = "Unassigned";
                    if (oldTicket.DueByDate != null)
                    {
                        oldDate = oldTicket.DueByDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    if (newTicket.DueByDate != null)
                    {
                        newDate = newTicket.DueByDate.Value.ToString(ConstantUtil.DateTimeFormat);
                    }
                    sb.Append(string.Format(@"<p>Due By Date changed from <b>{0}</b> to <b>{1}</b></p>", oldDate, newDate));
                }
            }
            //technician
            if (oldTicket.TechnicianID != null || newTicket.TechnicianID != null)
            {
                if (oldTicket.TechnicianID != newTicket.TechnicianID)
                {
                    string oldTechnicianString = "Unassigned";
                    string newTechnicianString = "Unassigned";
                    if (oldTicket.TechnicianID != null)
                    {
                        AspNetUser oldTechnician = _unitOfWork.AspNetUserRepository.GetByID(oldTicket.TechnicianID);
                        if (oldTechnician != null)
                        {
                            oldTechnicianString = oldTechnician.Fullname;
                        }
                    }
                    if (newTicket.TechnicianID != null)
                    {
                        AspNetUser newTechnician = _unitOfWork.AspNetUserRepository.GetByID(newTicket.TechnicianID);
                        if (newTechnician != null)
                        {
                            newTechnicianString = newTechnician.Fullname;
                        }
                    }
                    sb.Append(string.Format(@"<p>Technician changed from <b>{0}</b> to <b>{1}</b></p>", oldTechnicianString, newTechnicianString));
                }
            }
            result.Action = sb.ToString();
            if (string.IsNullOrWhiteSpace(result.Action))
            {
                return null;
            }
            return result;
        }
    }
}