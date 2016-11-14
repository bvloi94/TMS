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

        public Notification GetNotificationById(int id)
        {
            return _unitOfWork.NotificationRepository.GetByID(id);
        }

        public IEnumerable<Notification> GetUserNotifications(string id)
        {
            return _unitOfWork.NotificationRepository.Get().Where(m => m.BeNotifiedID == id);
        }

        public bool EditNotification(Notification notification)
        {
            _unitOfWork.NotificationRepository.Update(notification);
            return _unitOfWork.Commit();
        }

        public string GetNotificationContent(int ticketID, int actionType, string actID)
        {
            Ticket ticket = _unitOfWork.TicketRepository.GetByID(ticketID);
            Ticket mergedTicket;
            AspNetUser mergedUser;
            if (ticket != null)
            {
                switch (actionType)
                {
                    case ConstantUtil.NotificationActionType.RequesterNotiCreate:
                        return string.Format("Ticket #{0} ({1}) was created successfully.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiCancel:
                        return string.Format("Ticket #{0} ({1}) was cancelled.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiIsMerged:
                        mergedTicket = _unitOfWork.TicketRepository.GetByID(ticket.MergedID);
                        if (mergedTicket != null)
                        {
                            return string.Format("Ticket #{0} ({1}) was merged into ticket #{2}.", ticket.Code, ticket.Subject, mergedTicket.Code);
                        }
                        else
                        {
                            return string.Format("Ticket #{0} ({1}) was merged.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.RequesterNotiSolve:
                        return string.Format("Ticket #{0} ({1}) was solved.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.RequesterNotiClose:
                        return string.Format("Ticket #{0} ({1}) was closed.", ticket.Code, ticket.Subject);
                    case ConstantUtil.NotificationActionType.HelpDeskNotiCreate:
                        AspNetUser createdUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (createdUser != null)
                        {
                            return string.Format("Ticket #{0} ({1}) was created by {2}.", ticket.Code, ticket.Subject, createdUser.Fullname);
                        }
                        else
                        {
                            return string.Format("Ticket #{0} ({1}) was created.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.HelpDeskNotiUnapprove:
                        AspNetUser unapprovedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (unapprovedUser != null)
                        {
                            return string.Format("Ticket #{0} ({1}) was unapproved by {2}.", ticket.Code, ticket.Subject, unapprovedUser.Fullname);
                        }
                        else
                        {
                            return string.Format("Ticket #{0} ({1}) was unapproved.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiAssign:
                        AspNetUser assigedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (assigedUser != null)
                        {
                            return string.Format("Ticket #{0} ({1}) was assigned by {2}.", ticket.Code, ticket.Subject, assigedUser.Fullname);
                        }
                        else
                        {
                            return string.Format("Ticket #{0} ({1}) was assigned.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiUnassign:
                        AspNetUser unassigedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (unassigedUser != null)
                        {
                            return string.Format("Ticket #{0} ({1}) was unassigned by {2}.", ticket.Code, ticket.Subject, unassigedUser.Fullname);
                        }
                        else
                        {
                            return string.Format("Ticket #{0} ({1}) was unassigned.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiReassign:
                        AspNetUser reassigedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (reassigedUser != null)
                        {
                            return string.Format("Ticket #{0} ({1}) was reassigned by {2}.", ticket.Code, ticket.Subject, reassigedUser.Fullname);
                        }
                        else
                        {
                            return string.Format("Ticket #{0} ({1}) was reassigned.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiCancel:
                        AspNetUser cancelledUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (cancelledUser != null)
                        {
                            return string.Format("Ticket #{0} ({1}) was cancelled by {2}.", ticket.Code, ticket.Subject, cancelledUser.Fullname);
                        }
                        else
                        {
                            return string.Format("Ticket #{0} ({1}) was cancelled.", ticket.Code, ticket.Subject);
                        }
                    case ConstantUtil.NotificationActionType.TechnicianNotiIsMerged:
                        mergedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (mergedUser != null)
                        {
                            if (ticket.MergedID.HasValue)
                            {
                                mergedTicket = _unitOfWork.TicketRepository.GetByID(ticket.MergedID);
                                return string.Format("Ticket #{0} ({1}) was merged into ticket #{2} ({3}) by {4}.", ticket.Code, ticket.Subject, mergedTicket.Code, mergedTicket.Subject, mergedUser.Fullname);
                            }
                        }
                        else
                        {
                            if (ticket.MergedID.HasValue)
                            {
                                mergedTicket = _unitOfWork.TicketRepository.GetByID(ticket.MergedID);
                                return string.Format("Ticket #{0} ({1}) was merged into ticket #{2} ({3}).", ticket.Code, ticket.Subject, mergedTicket.Code, mergedTicket.Subject);
                            }
                        }
                        break;
                    case ConstantUtil.NotificationActionType.TechnicianNotiMerge:
                        mergedUser = _unitOfWork.AspNetUserRepository.GetByID(actID);
                        if (mergedUser != null)
                        {
                            return string.Format("Ticket #{0} ({1}) was merged by {2}.", ticket.Code, ticket.Subject, mergedUser.Fullname);
                        }
                        else
                        {
                            return string.Format("Ticket #{0} ({1}) was merged.", ticket.Code, ticket.Subject);
                        }
                }
            }
            return string.Empty;
        }
    }
}