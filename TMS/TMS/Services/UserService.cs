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

        public bool AddUser(AspNetUser user)
        {
            _unitOfWork.AspNetUserRepository.Insert(user);
            return _unitOfWork.Commit();
        }

        public AspNetUser GetUserByUsername(string username)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.UserName == username).FirstOrDefault();
        }

        public AspNetUser GetUserById(string id)
        {
            return _unitOfWork.AspNetUserRepository.GetByID(id);
        }

        public AspNetUser GetActiveUserById(string id)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Id.Equals(id) && m.IsActive == true).FirstOrDefault();
        }

        public bool IsValidEmail(string email)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Email.Equals(email.ToLower()) && m.IsActive == true
                                                             &&
                                                             m.AspNetRoles.FirstOrDefault()
                                                                 .Name.ToLower()
                                                                 .Equals("requester")).Any();
        }

        public bool EditUser(AspNetUser user)
        {
            _unitOfWork.AspNetUserRepository.Update(user);
            return _unitOfWork.Commit();
        }

        public bool RemoveUser(string id)
        {
            AspNetUser user = _unitOfWork.AspNetUserRepository.GetByID(id);
            user.IsActive = false;
            _unitOfWork.AspNetUserRepository.Update(user);
            return _unitOfWork.Commit();
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

        public IEnumerable<AspNetUser> SearchRequesters(string query)
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "requester"
                                                        && (query == null || query == "" || r.Fullname.ToLower().Contains(query.ToLower())));
        }

        public IEnumerable<AspNetUser> GetHelpDesks()
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "helpdesk");
        }

        public IEnumerable<AspNetUser> GetTechnicians()
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "technician");
        }

        public IEnumerable<AspNetUser> GetTechnicianByPattern(string query, int? groupId)
        {
            return _unitOfWork.AspNetUserRepository.Get(
                    r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "technician"
                         && (!groupId.HasValue || groupId == 0 || r.GroupID == groupId)
                         && (query == null || r.Fullname.Contains(query)));
        }

        public IEnumerable<AspNetUser> GetHelpDeskByPattern(string query, int? groupId)
        {
            return _unitOfWork.AspNetUserRepository.Get(
                    r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "helpdesk"
                         && (!groupId.HasValue || groupId == 0 || r.GroupID == groupId)
                         && (query == null || r.Fullname.Contains(query)));
        }

        public IEnumerable<AspNetUser> GetAdmins(string currentUserId)
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower() == "admin" && r.Id != currentUserId);
        }

        public IEnumerable<AspNetUser> GetManagers()
        {
            return _unitOfWork.AspNetUserRepository.Get(r => r.AspNetRoles.FirstOrDefault().Name.ToLower().Equals(ConstantUtil.UserRoleString.Manager.ToLower()));
        }

        public bool IsActive(string id)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.Id == id && m.IsActive == true).Any();
        }

        public bool ToggleStatus(AspNetUser user)
        {
            bool? status = user.IsActive;
            if (status == null || status == false)
            {
                user.IsActive = true;
            }
            else
            {
                user.IsActive = false;
            }

            _unitOfWork.AspNetUserRepository.Update(user);
            return _unitOfWork.Commit();
        }

        public IEnumerable<AspNetUser> GetActiveTechnicians()
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.AspNetRoles.FirstOrDefault().Name.ToLower().Equals("technician")
                && m.IsActive == true);
        }
    }
}