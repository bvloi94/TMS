﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class SolutionService
    {
        private UnitOfWork _unitOfWork;

        public SolutionService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Solution> GetAllSolutions()
        {
            return _unitOfWork.SolutionRepository.Get();
        }

        public void AddSolution(Solution solution)
        {
            try

            {
                _unitOfWork.SolutionRepository.Insert(solution);
                _unitOfWork.Commit();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool IsDuplicateSubject(int? id, string subject)
        {
            if (id == null)
            {
                return _unitOfWork.SolutionRepository.Get(m => m.Subject.ToLower().Equals(subject.ToLower())).Any();
            }
            else
            {
                return _unitOfWork.SolutionRepository.Get(m => m.ID != id && m.Subject.ToLower().Equals(subject.ToLower())).Any();
            }

        }

        public Solution GetSolutionById(int? id)
        {
            if (id.HasValue)
            {
                return _unitOfWork.SolutionRepository.GetByID(id);
            }
            else
            {
                return null;
            }
        }

        public void EditSolution(Solution solution)
        {
            _unitOfWork.SolutionRepository.Update(solution);
            _unitOfWork.Commit();
        }

        public IEnumerable<Solution> GetSolutionsByCategory(int id)
        {
            return _unitOfWork.SolutionRepository.Get(m => m.CategoryID == id);
        }

        public bool IsduplicatePath(int? id, string path)
        {
            if (id == null)
            {
                return _unitOfWork.SolutionRepository.Get(m => m.Path.ToLower().Equals(path.ToLower())).Any();
            }
            else
            {
                return _unitOfWork.SolutionRepository.Get(m => m.ID != id && m.Path.ToLower().Equals(path.ToLower())).Any();
            }

        }

        public IEnumerable<Solution> SearchSolutions(string searchtxt)
        {
            return _unitOfWork.SolutionRepository.Get(s => s.Subject.ToLower().Contains(searchtxt.ToLower()));
        }

        public Solution GetSolutionById(int id)
        {
            return _unitOfWork.SolutionRepository.GetByID(id);
        }

        public Solution GetSolutionByPath(string path)
        {
            return _unitOfWork.SolutionRepository.Get(m => m.Path.Equals(path)).FirstOrDefault();
        }
    }
}