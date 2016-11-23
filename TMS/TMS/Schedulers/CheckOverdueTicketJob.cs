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
    public class CheckOverdueTicketJob : Job
    {
        private UnitOfWork _unitOfWork;
        private UserService _userService;
        private TicketService _ticketService;

        public CheckOverdueTicketJob()
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
            IEnumerable<Ticket> tickets = _ticketService.GetOverdueTickets().Where(m => (int)(DateTime.Now.Date - m.DueByDate.Date).TotalDays > 0);
            IEnumerable<AspNetUser> helpdesks = _userService.GetHelpDesks().Where(m => m.IsActive == true);
            foreach (Ticket ticket in tickets)
            {
                EmailUtil.SendToHelpdesksWhenTicketIsOverdue(ticket, helpdesks);
                if (!string.IsNullOrWhiteSpace(ticket.TechnicianID))
                {
                    AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
                    if (technician != null)
                    {
                        EmailUtil.SendToTechnicianWhenTicketIsOverdue(ticket, technician);
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
            return (int)TimeSpan.FromDays(1).TotalMilliseconds;
        }
    }
}