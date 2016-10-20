﻿using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
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
            try
            {
                return _unitOfWork.AspNetUserRepository.GetByID(id);
            }
            catch
            {
                throw;
            }
        }

        public bool IsValidEmail(string email)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Email.Equals(email.ToLower()) && m.IsActive == true
                        && m.AspNetRoles.FirstOrDefault().Name.ToLower().Equals("requester")).Any();
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
            try
            {
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
        }

        public AspNetUser GetUserByEmail(string email)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Email.Equals(email.ToLower())).FirstOrDefault();
        }

        public bool IsDuplicatedEmail(string id, string email)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Id != id && m.Email.ToLower().Equals(email.ToLower())).Any();
        }

        public IEnumerable<AspNetUser> GetRequesters()
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "requester");
        }

        public IEnumerable<AspNetUser> GetHelpDesks()
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "helpdesk");
        }

        public IEnumerable<AspNetUser> GetTechnicians()
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "technician");
        }

        public IEnumerable<AspNetUser> GetTechnicianByPattern(string query, int? departmentId)
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "technician"
                                     && r.DepartmentID==departmentId
                                     && (query == null || r.Fullname.Contains(query)));
        }

        public IEnumerable<AspNetUser> GetAdmins()
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "admin");
        }

        public bool IsActive(string id)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Id == id && m.IsActive == true).Count() > 0;
        }

        public bool ToggleStatus(AspNetUser user)
        {
            bool? status = user.IsActive;
            bool isEnable;
            if (status == null || status == false)
            {
                user.IsActive = true;
                isEnable = true;
            }
            else
            {
                user.IsActive = false;
                isEnable = false;
            }
            try
            {
                _unitOfWork.AspNetUserRepository.Update(user);
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
            return isEnable;
        }

    }
}