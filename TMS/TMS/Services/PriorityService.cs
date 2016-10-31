﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class PriorityService
    {
        private UnitOfWork _unitOfWork;

        public PriorityService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Priority> GetAll()
        {
            return _unitOfWork.PriorityRepository.Get();
        }

        public bool IsDuplicateName(int? id, string name)
        {
            if (id == null)
            {
                return _unitOfWork.PriorityRepository.Get(p => p.Name.ToLower().Equals(name.ToLower())).Any();
            }
            else
            {
                return _unitOfWork.PriorityRepository.Get(p => p.ID != id && p.Name.ToLower().Equals(name.ToLower())).Any();
            }
        }

        public void AddPriority(Priority priority)
        {
            try
            {
                _unitOfWork.PriorityRepository.Insert(priority);
                _unitOfWork.Save();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public Priority GetPriorityByID(int id)
        {
            return _unitOfWork.PriorityRepository.GetByID(id);
        }

        public void UpdatePriority(Priority priority)
        {
            try
            {
                _unitOfWork.PriorityRepository.Update(priority);
                _unitOfWork.Save();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool IsInUse(Priority priority)
        {
            return _unitOfWork.TicketRepository.Get(m => m.PriorityID == priority.ID).Any();
        }

        public void DeletePriority(Priority priority)
        {
            try
            {
                foreach (PriorityMatrixItem priorityMatrixItem in priority.PriorityMatrixItems.ToList())
                {
                    _unitOfWork.PriorityMatrixItemRepository.Delete(priorityMatrixItem);
                }
                _unitOfWork.PriorityRepository.Delete(priority);
                _unitOfWork.Save();
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}