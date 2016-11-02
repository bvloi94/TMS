using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class BusinessRuleService 
    {
        private readonly UnitOfWork _unitOfWork;

        public BusinessRuleService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<BusinessRule> GetAll()
        {
            return _unitOfWork.BusinessRuleRepository.Get();
        }

    }
}