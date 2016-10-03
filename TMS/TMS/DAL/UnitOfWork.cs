using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Models;

namespace TMS.DAL
{
    public class UnitOfWork : IDisposable
    {

        private TMSEntities _context = new TMSEntities();
        private GenericRepository<Category> _categoryRepository;
        private GenericRepository<AspNetRole> _aspNetRoleRepository;
        private GenericRepository<AspNetUser> _aspNetUserRepository;
        private GenericRepository<AspNetUserClaim> _aspNetUserClaimRepository;
        private GenericRepository<AspNetUserLogin> _aspNetUserLoginRepository;
        private GenericRepository<BusinessRule> _businessRuleRepository;
        private GenericRepository<BusinessRuleCondition> _businessRuleConditionRepository;
        private GenericRepository<BusinessRuleNotification> _businessRuleNotificationRepository;
        private GenericRepository<BusinessRuleTrigger> _businessRuleTriggerRepository;
        private GenericRepository<Department> _departmentRepository;
        private GenericRepository<Impact> _impactRepository;
        private GenericRepository<Solution> _solutionRepository;
        private GenericRepository<SolutionAttachment> _solutionAttachmentRepository;
        private GenericRepository<Priority> _priorityRepository;
        private GenericRepository<PriorityMatrixItem> _priorityMatrixItemRepository;
        private GenericRepository<Ticket> _ticketRepository;
        private GenericRepository<TicketAttachment> _ticketAttachmentRepository;
        private GenericRepository<TicketHistory> _ticketHistoryRepository;
        private GenericRepository<Urgency> _urgencyRepository;

        public GenericRepository<Category> CategoryRepository
        {
            get
            {
                if (this._categoryRepository == null)
                {
                    this._categoryRepository = new GenericRepository<Category>(_context);
                }
                return _categoryRepository;
            }
        }

        public GenericRepository<AspNetRole> AspNetRoleRepository
        {
            get
            {
                if (this._aspNetRoleRepository == null)
                {
                    this._aspNetRoleRepository = new GenericRepository<AspNetRole>(_context);
                }
                return _aspNetRoleRepository;
            }
        }

        public GenericRepository<AspNetUser> AspNetUserRepository
        {
            get
            {
                if (this._aspNetUserRepository == null)
                {
                    this._aspNetUserRepository = new GenericRepository<AspNetUser>(_context);
                }
                return _aspNetUserRepository;
            }
        }

        public GenericRepository<AspNetUserClaim> AspNetUserClaimRepository
        {
            get
            {
                if (this._aspNetUserClaimRepository == null)
                {
                    this._aspNetUserClaimRepository = new GenericRepository<AspNetUserClaim>(_context);
                }
                return _aspNetUserClaimRepository;
            }
        }

        public GenericRepository<AspNetUserLogin> AspNetUserLoginRepository
        {
            get
            {
                if (this._aspNetUserLoginRepository == null)
                {
                    this._aspNetUserLoginRepository = new GenericRepository<AspNetUserLogin>(_context);
                }
                return _aspNetUserLoginRepository;
            }
        }

        public GenericRepository<BusinessRule> BusinessRuleRepository
        {
            get
            {
                if (this._businessRuleRepository == null)
                {
                    this._businessRuleRepository = new GenericRepository<BusinessRule>(_context);
                }
                return _businessRuleRepository;
            }
        }

        public GenericRepository<BusinessRuleCondition> BusinessRuleConditionRepository
        {
            get
            {
                if (this._businessRuleConditionRepository == null)
                {
                    this._businessRuleConditionRepository = new GenericRepository<BusinessRuleCondition>(_context);
                }
                return _businessRuleConditionRepository;
            }
        }

        public GenericRepository<BusinessRuleNotification> BusinessRuleNotificationRepository
        {
            get
            {
                if (this._businessRuleNotificationRepository == null)
                {
                    this._businessRuleNotificationRepository = new GenericRepository<BusinessRuleNotification>(_context);
                }
                return _businessRuleNotificationRepository;
            }
        }

        public GenericRepository<BusinessRuleTrigger> BusinessRuleTriggerRepository
        {
            get
            {
                if (this._businessRuleTriggerRepository == null)
                {
                    this._businessRuleTriggerRepository = new GenericRepository<BusinessRuleTrigger>(_context);
                }
                return _businessRuleTriggerRepository;
            }
        }

        public GenericRepository<Department> DepartmentRepository
        {
            get
            {
                if (this._departmentRepository == null)
                {
                    this._departmentRepository = new GenericRepository<Department>(_context);
                }
                return _departmentRepository;
            }
        }

        public GenericRepository<Impact> ImpactRepository
        {
            get
            {
                if (this._impactRepository == null)
                {
                    this._impactRepository = new GenericRepository<Impact>(_context);
                }
                return _impactRepository;
            }

        }

        public GenericRepository<Solution> SolutionRepository
        {
            get
            {
                if (this._solutionRepository == null)
                {
                    this._solutionRepository = new GenericRepository<Solution>(_context);
                }
                return _solutionRepository;
            }
        }

        public GenericRepository<SolutionAttachment> SolutionAttachmentRepository
        {
            get
            {
                if (this._solutionAttachmentRepository == null)
                {
                    this._solutionAttachmentRepository = new GenericRepository<SolutionAttachment>(_context);
                }
                return _solutionAttachmentRepository;
            }
        }

        public GenericRepository<Priority> PriorityRepository
        {
            get
            {
                if (this._priorityRepository == null)
                {
                    this._priorityRepository = new GenericRepository<Priority>(_context);
                }
                return _priorityRepository;
            }
        }

        public GenericRepository<PriorityMatrixItem> PriorityMatrixItemRepository
        {
            get
            {
                if (this._priorityMatrixItemRepository == null)
                {
                    this._priorityMatrixItemRepository = new GenericRepository<PriorityMatrixItem>(_context);
                }
                return _priorityMatrixItemRepository;
            }
        }

        public GenericRepository<Ticket> TicketRepository
        {
            get
            {
                if (this._ticketRepository == null)
                {
                    this._ticketRepository = new GenericRepository<Ticket>(_context);
                }
                return _ticketRepository;
            }
        }

        public GenericRepository<TicketAttachment> TicketAttachmentRepository
        {
            get
            {
                if (this._ticketAttachmentRepository == null)
                {
                    this._ticketAttachmentRepository = new GenericRepository<TicketAttachment>(_context);
                }
                return _ticketAttachmentRepository;
            }
        }

        public GenericRepository<TicketHistory> TicketHistoryRepository
        {
            get
            {
                if (this._ticketHistoryRepository == null)
                {
                    this._ticketHistoryRepository = new GenericRepository<TicketHistory>(_context);
                }
                return _ticketHistoryRepository;
            }
        }

        public GenericRepository<Urgency> UrgencyRepository
        {
            get
            {
                if (this._urgencyRepository == null)
                {
                    this._urgencyRepository = new GenericRepository<Urgency>(_context);
                }
                return _urgencyRepository;
            }
        }



        public void Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
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