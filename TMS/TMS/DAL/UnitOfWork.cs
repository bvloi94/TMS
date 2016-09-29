using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Models;

namespace TMS.DAL
{
    public class UnitOfWork : IDisposable
    {

        private TMSContext context = new TMSContext();
        private GenericRepository<Ticket> ticketRepository;
        private GenericRepository<KnowlegeBase> knowledgeBaseRepository;

        public GenericRepository<Ticket> TicketRepository
        {
            get
            {
                if (this.ticketRepository == null)
                {
                    this.ticketRepository = new GenericRepository<Ticket>(context);
                }
                return ticketRepository;
            }
        }

        public GenericRepository<KnowlegeBase> KnowledgeBaseRepository
        {
            get
            {
                if (this.knowledgeBaseRepository == null)
                {
                    this.knowledgeBaseRepository = new GenericRepository<KnowlegeBase>(context);
                }
                return knowledgeBaseRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}