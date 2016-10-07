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
                return _unitOfWork.UrgencyRepository.Get(p => p.Name == name).Any();
            }
            else
            {
                return _unitOfWork.UrgencyRepository.Get(p => p.ID != id && p.Name == name).Any();
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
    }
}