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
            AspNetUser solver = _unitOfWork.AspNetUserRepository.GetByID(ticket.SolvedID);
            AspNetUser actedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);

            if (ticket != null)
            {
                string subject = ticket.Subject.Length > 120 ? ticket.Subject.Substring(0, 119) + "..." : ticket.Subject;
                switch (actionType)
                {
                    case ConstantUtil.NotificationActionType.RequesterNotiCreate:
                        return string.Format("Ticket #{0} was created: <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiCancel:
                        return string.Format("Ticket #{0} was cancelled: <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiIsMerged:
                        if (mergedTicket != null)
                        {
                            return string.Format("Ticket #{0} <b>\"{1}\"</b> was merged into ticket #{2} <b>\"{3}\"</b>.", ticket.Code, subject, mergedTicket.Code, mergedTicket.Subject);
                        }
                        break;
                    case ConstantUtil.NotificationActionType.RequesterNotiSolve:
                        return string.Format("Ticket #{0} was solved: <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiClose:
                        return string.Format("Ticket #{0} was closed: <b>\"{1}\"</b>.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.HelpDeskNotiCreate:
                        if (actedUser != null)
                        {
                            return string.Format("<b>{0}</b> created the ticket #{1}: <b>\"{2}\"</b>.", actedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        break;
                    case ConstantUtil.NotificationActionType.HelpDeskNotiUnapprove:
                        if (actedUser != null)
                        {
                            return string.Format("<b>Requester {0}</b> unapproved the ticket #{1}: <b>\"{2}\"</b>", actedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiAssign:
                        if (actedUser != null)
                        {
                            return string.Format("<b>{0}</b> assigned the ticket #{1} to you: <b>\"{2}\"</b>.", actedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiUnassign:
                        if (actedUser != null)
                        {
                            return string.Format("<b>{0}</b> unassigned the ticket #{1}: <b>\"{2}\"</b>.", actedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiReassign:
                        if (actedUser != null)
                        {
                            return string.Format("<b>{0}</b> reassigned the ticket #{1} to you: <b>\"{2}\"</b>.", ticket.Code, ticket.Subject, actedUser.Fullname);
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiCancel:
                        if (actedUser != null)
                        {
                            return string.Format("<b>{0}</b> cancelled the ticket #{1}: <b>\"{2}\"</b>.", actedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiIsMerged:
                        if (actedUser != null)
                        {
                            if (mergedTicket != null)
                            {
                                return string.Format("<b>{0}</b> merged ticket #{1} <b>\"{2}\"</b> into ticket #{3} <b>\"{4}\"</b>.", actedUser.Fullname, ticket.Code, subject, mergedTicket.Code, mergedTicket.Subject);
                            }
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiMerge:
                        if (actedUser != null)
                        {
                            return string.Format("<b>{0}</b> merged some tickets into ticket: #{1} <b>\"{2}\"</b>.", actedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiChangeDueByDate:
                        if (actedUser != null)
                        {
                            return string.Format("<b>{0}</b> changed due by date of ticket #{1}:  <b>\"{3}\"</b>.", actedUser.Fullname, ticket.Code, ticket.Subject);
                        }
                        break;
                }
            }
            return string.Empty;
        }
    }
}