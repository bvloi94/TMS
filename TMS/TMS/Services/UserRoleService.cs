using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class UserRoleService //: IUserService
    {
        private readonly UnitOfWork _unitOfWork;

        public UserRoleService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}