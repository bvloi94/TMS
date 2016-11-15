using EAGetMail;
using log4net;
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
    public class CheckToCloseTicketJob : Job
    {
        private UnitOfWork _unitOfWork;
        private UserService _userService;
        private TicketService _ticketService;
        private ILog log = LogManager.GetLogger(typeof(CheckToCloseTicketJob));

        public CheckToCloseTicketJob()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
            _ticketService = new TicketService(_unitOfWork);
        }

        public override string GetName()
        {
            return this.GetType().Name;
        }

        public override void DoJob()
        {
            
            IEnumerable<Ticket> tickets = _ticketService.GetSolvedTickets();
            foreach (Ticket ticket in tickets)
            {
                if (ticket.SolvedDate.HasValue)
                {
                    if (DateTime.Now.Date > ticket.SolvedDate.Value.AddDays(ConstantUtil.DayToCloseTicket).Date)
                    {
                        ticket.Status = ConstantUtil.TicketStatus.Closed;
                        ticket.ModifiedTime = DateTime.Now;
                        try
                        {
                            _ticketService.UpdateTicket(ticket, null);
                            AspNetUser requester = _userService.GetActiveUserById(ticket.RequesterID);
                            if (requester != null)
                            {
                                EmailUtil.SendToRequesterWhenCloseTicket(ticket, requester);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error("Scheduler close ticket error", e);
                        }
                    }
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
            return (int) TimeSpan.FromDays(1).TotalMilliseconds;
        }
    }
}