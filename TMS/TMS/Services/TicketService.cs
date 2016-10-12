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

        public Ticket GetTicketByID(int id)
        {
            return _unitOfWork.TicketRepository.GetByID(id);
        }

        public bool UpdateTicket(Ticket ticket)
        {
            _unitOfWork.TicketRepository.Update(ticket);
            return _unitOfWork.Save() > 0;
        }

        public void SolveTicket(Ticket ticket)
        {
            ticket.Status = 3; //Solved
            _unitOfWork.TicketRepository.Update(ticket);
            _unitOfWork.Save();
        }

        public IEnumerable<Ticket> GetTechnicianTickets(string id)
        {
            return _unitOfWork.TicketRepository.Get(m => m.TechnicianID == id);
        }
    }
}