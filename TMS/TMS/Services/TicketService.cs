﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

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
    }
}