using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class TicketHistoryService
    {
        private readonly UnitOfWork _unitOfWork;

        public TicketHistoryService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<TicketHistory> GetHistoryTicketsByTicketID(int ticketId)
        {
            return _unitOfWork.TicketHistoryRepository.Get(m => m.TicketID == ticketId);
        }
    }
}