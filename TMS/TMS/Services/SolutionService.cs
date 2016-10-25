﻿using System;
using System.Collections.Generic;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    internal class SolutionService
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
                _unitOfWork.Save();
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}