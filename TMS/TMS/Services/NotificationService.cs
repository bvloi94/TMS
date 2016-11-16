using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class NotificationService
    {
        private readonly UnitOfWork _unitOfWork;

        public NotificationService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Notification> GetAll()
        {
            return _unitOfWork.NotificationRepository.Get();
        }

        public IEnumerable<Notification> GetHelpDeskNotifications()
        {
            return _unitOfWork.NotificationRepository.Get(m => m.IsForHelpDesk == true);
        }

        public Notification GetNotificationById(int id)
        {
            return _unitOfWork.NotificationRepository.GetByID(id);
        }

        public IEnumerable<Notification> GetUserNotifications(string id)
        {
            return _unitOfWork.NotificationRepository.Get(m => m.BeNotifiedID == id);
        }

        public bool EditNotification(Notification notification)
        {
            _unitOfWork.NotificationRepository.Update(notification);
            return _unitOfWork.Commit();
        }

        public string GetNotificationContent(int ticketID, int actionType, string actID)
        {
            Ticket ticket = _unitOfWork.TicketRepository.GetByID(ticketID);
            Ticket mergedTicket = _unitOfWork.TicketRepository.GetByID(ticket.MergedID);
            AspNetUser creater = _unitOfWork.AspNetUserRepository.GetByID(ticket.CreatedID);
            AspNetUser solver = _unitOfWork.AspNetUserRepository.GetByID(ticket.SolveID);
            AspNetUser actedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);

            if (ticket != null)
            {
                string subject = ticket.Subject.Length > 120 ? ticket.Subject.Substring(0, 119) + "..." : ticket.Subject;
                switch (actionType)
                {
                    
                    case ConstantUtil.NotificationActionType.RequesterNotiCreate:
                        return string.Format("You created a ticket: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiCancel:
                        return string.Format("One of your ticket was cancelled: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiIsMerged:
                        mergedTicket = _unitOfWork.TicketRepository.GetByID(ticket.MergedID);
                        if (mergedTicket != null)
                        {
                            return string.Format("Ticket #{0} <b>\"{1}\"</b> was merged into ticket #{2}.", ticket.Code, subject, mergedTicket.Code);
                        }
                        else
                        {
                            return string.Format("A ticket was merged: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.RequesterNotiSolve:
                        return string.Format("<b>{0}</b> solved a ticket: #{1} <b>\"{2}\"</b>.", solver.Fullname, ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiClose:
                        return string.Format("A ticket was closed: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.HelpDeskNotiCreate:
                        AspNetUser createdUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (createdUser != null)
                        {
                            return string.Format("<b>{0}</b> created a ticket: #{1} <b>\"{2}\"</b>.", createdUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        else
                        {
                            return string.Format("A ticket was created: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.HelpDeskNotiUnapprove:
                        AspNetUser unapprovedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (unapprovedUser != null)
                        {
                            return string.Format("<b>Requester {0}</b> unapproved a ticket: #{1} <b>\"{2}\"</b>", unapprovedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        else
                        {
                            return string.Format("A ticket was unapproved: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiAssign:
                        AspNetUser assigedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (assigedUser != null)
                        {
                            return string.Format("<b>{0}</b> assigned a ticket to you: #{1} <b>\"{2}\"</b>.", assigedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        else
                        {
                            return string.Format("A ticket was assigned: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiUnassign:
                        AspNetUser unassigedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (unassigedUser != null)
                        {
                            return string.Format("<b>{0}</b> unassigned a ticket: #{1} <b>\"{2}\"</b>.", unassigedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        else
                        {
                            return string.Format("A ticket was unassigned: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiReassign:
                        AspNetUser reassigedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (reassigedUser != null)
                        {
                            return string.Format("<b>{0}</b> reassigned a ticket to you: #{1} <b>\"{2}\"</b>.", ticket.Code, ticket.Subject, reassigedUser.Fullname);
                        }
                        else
                        {
                            return string.Format("A ticket was reassigned: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiCancel:
                        AspNetUser cancelledUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (cancelledUser != null)
                        {
                            return string.Format("<b>{0}</b> cancelled a ticket: #{1} <b>\"{2}\"</b>.", cancelledUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        else
                        {
                            return string.Format("A ticket was cancelled: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiIsMerged:
                        if (actedUser != null)
                        {
                            if (ticket.MergedID.HasValue)
                            {
                                mergedTicket = _unitOfWork.TicketRepository.GetByID(ticket.MergedID);
                                string ticketSubject = ticket.Subject.Length > 50 ? ticket.Subject.Substring(0, 49) + "..." : ticket.Subject;
                                string mergedTicketSubject = ticket.Subject.Length > 50 ? ticket.Subject.Substring(0, 49) + "..." : ticket.Subject;
                                return string.Format("<b>{0}</b> merged ticket #{1} <b>\"{2}\"</b> into ticket #{3} <b>\"{4}\"</b>.", actedUser.Fullname, ticket.Code, subject, mergedTicket.Code, mergedTicket.Subject);
                            }
                        }
                        else
                        {
                            if (ticket.MergedID.HasValue)
                            {
                                mergedTicket = _unitOfWork.TicketRepository.GetByID(ticket.MergedID);
                                return string.Format("Ticket #{0} <b>\"{1}\"</b> was merged into ticket #{2} <b>\"{3}\"</b>.", ticket.Code, subject, mergedTicket.Code, mergedTicket.Subject);
                            }
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiMerge:
                        if (actedUser != null)
                        {
                            return string.Format("<b>{0}</b> merge 2 ticket: #{1} <b>\"{2}\"</b>.", actedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        else
                        {
                            return string.Format("A ticket was merged: #{0} <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                        }
                }
            }
            return string.Empty;
        }
    }
}