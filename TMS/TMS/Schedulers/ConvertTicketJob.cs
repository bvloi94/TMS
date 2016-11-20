using EAGetMail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;

namespace TMS.Schedulers
{
    public class ConvertTicketJob : Job
    {
        private UnitOfWork _unitOfWork;
        private UserService _userService;
        private TicketService _ticketService;
        private UrgencyService _urgencyService;
        private ImpactService _impactService;

        public ConvertTicketJob()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
            _ticketService = new TicketService(_unitOfWork);
            _urgencyService = new UrgencyService(_unitOfWork);
            _impactService = new ImpactService(_unitOfWork);
        }

        public override string GetName()
        {
            return this.GetType().Name;
        }

        public override void DoJob()
        {
            List<Mail> mailList = EmailUtil.GetUnreadMailsUsingEAGetMail();
            foreach (Mail mail in mailList)
            {
                string requesterEmail = mail.From.Address;
                if (_userService.IsValidEmail(requesterEmail))
                {
                    string subject = mail.Subject.Replace("(Trial Version)", "");
                    string description = mail.TextBody;
                    Urgency urgency = _urgencyService.GetSystemUrgency();
                    Impact impact = _impactService.GetSystemImpact();
                    Ticket ticket = new Ticket
                    {
                        Subject = subject,
                        Description = description == null ? string.Empty : description.Trim(),
                        Status = ConstantUtil.TicketStatus.Open,
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                        ScheduleStartDate = DateTime.Now,
                        DueByDate = DateTime.Now.AddHours(urgency.Duration),
                        ScheduleEndDate = DateTime.Now.AddDays(ConstantUtil.DayToCloseTicket),
                        UrgencyID = urgency.ID,
                        ImpactID = impact.ID,
                        PriorityID = _ticketService.GetPriorityId(impact.ID, DateTime.Now.AddHours(urgency.Duration)),
                        CreatedID = _userService.GetUserByEmail(requesterEmail).Id,
                        RequesterID = _userService.GetUserByEmail(requesterEmail).Id,
                        Mode = ConstantUtil.TicketMode.Email,
                        TicketKeywords = _ticketService.GetTicketKeywords(subject)
                    };

                    if (mail.Attachments.Length > 0)
                    {
                        string attachmentDirectory = HostingEnvironment.MapPath(@"~/Uploads/Attachments");
                        if (!Directory.Exists(attachmentDirectory))
                        {
                            Directory.CreateDirectory(attachmentDirectory);
                        }
                        foreach (EAGetMail.Attachment att in mail.Attachments)
                        {
                            string fileName = att.Name.Replace(Path.GetFileNameWithoutExtension(att.Name), Guid.NewGuid().ToString());
                            string attName = String.Format("{0}\\{1}", attachmentDirectory, fileName);
                            att.SaveAs(attName, true);
                            TicketAttachment ticketAttachment = new TicketAttachment();
                            ticketAttachment.Path = "/Uploads/Attachments/" + fileName;
                            ticketAttachment.Filename = att.Name;
                            ticketAttachment.Type = ConstantUtil.TicketAttachmentType.Description;
                            ticket.TicketAttachments.Add(ticketAttachment);
                        }
                    }
                    _ticketService.AddTicket(ticket);
                }
            }
        }

        /// <summary>
        /// Determines this job is repeatable.
        /// </summary>
        /// <returns>Returns true because this job is repeatable.</returns>
        public override bool IsRepeatable()
        {
            return true;
        }

        /// <summary>
        /// Determines that this job is to be executed again after
        /// 1 sec.
        /// </summary>
        /// <returns>1 sec, which is the interval this job is to be
        /// executed repeatadly.</returns>
        public override int GetRepetitionIntervalTime()
        {
            return (int) TimeSpan.FromMinutes(5).TotalMilliseconds;
        }
    }
}