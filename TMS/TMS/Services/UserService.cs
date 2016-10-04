using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class UserService //: IUserService
    {
        private readonly UnitOfWork _unitOfWork;

        public UserService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<AspNetUser> GetAll()
        {
            return _unitOfWork.AspNetUserRepository.Get();
        }

        public void AddUser(AspNetUser user)
        {
            _unitOfWork.AspNetUserRepository.Insert(user);
            _unitOfWork.Save();
        }

        public AspNetUser GetUserByUsername(string username)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.UserName == username).FirstOrDefault();
        }

        public AspNetUser GetUserById(string id)
        {
            return _unitOfWork.AspNetUserRepository.GetByID(id);
        }

        public void EditUser(AspNetUser user)
        {
            _unitOfWork.AspNetUserRepository.Update(user);
            _unitOfWork.Save();
        }

        public void RemoveUser(string id)
        {
            AspNetUser user = _unitOfWork.AspNetUserRepository.GetByID(id);
            user.IsActive = false;
            _unitOfWork.AspNetUserRepository.Update(user);
            _unitOfWork.Save();
        }

        public bool IsDuplicatedEmail(string id, string email)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Id != id && m.Email == email).Count() > 0;
        }

        public IEnumerable<AspNetUser> GetRequesters()
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.IsActive == true && r.AspNetRoles.FirstOrDefault().Name == "Requester");
        }

        public bool IsActive(string id)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Id == id && m.IsActive == true).Count() > 0;
        }
    }
}