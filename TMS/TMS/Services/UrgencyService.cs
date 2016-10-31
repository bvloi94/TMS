using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class UrgencyService
    {
        private readonly UnitOfWork _unitOfWork;

        public UrgencyService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Urgency> GetAll()
        {
            return _unitOfWork.UrgencyRepository.Get();
        }

        public bool IsDuplicatedName(int? id, string name)
        {
            if (id == null)
            {
                return _unitOfWork.UrgencyRepository.Get(p => p.Name.ToLower().Equals(name.ToLower())).Any();
            }
            else
            {
                return _unitOfWork.UrgencyRepository.Get(p => p.ID != id && p.Name.ToLower().Equals(name.ToLower())).Any();
            }
        }

        public void AddUrgency(Urgency urgency)
        {
            try
            {
                _unitOfWork.UrgencyRepository.Insert(urgency);
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
        }

        public Urgency GetUrgencyByID(int id)
        {
            return _unitOfWork.UrgencyRepository.GetByID(id);
        }

        public void UpdateUrgency(Urgency urgency)
        {
            try
            {
                _unitOfWork.UrgencyRepository.Update(urgency);
                _unitOfWork.Save();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public bool IsInUse(Urgency urgency)
        {
            return _unitOfWork.TicketRepository.Get(m => m.UrgencyID == urgency.ID).Any();
        }

        public void DeleteUrgency(Urgency urgency)
        {
            try
            {
                foreach (PriorityMatrixItem priorityMatrixItem in urgency.PriorityMatrixItems.ToList())
                {
                    _unitOfWork.PriorityMatrixItemRepository.Delete(priorityMatrixItem);
                }
                _unitOfWork.UrgencyRepository.Delete(urgency);
                _unitOfWork.Save();
            }
            catch (Exception)
            {
                throw;
            }
           
        }
    }
}