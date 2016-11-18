using System.Collections.Generic;
using System.Linq;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class GroupService
    {
        private readonly UnitOfWork _unitOfWork;

        public GroupService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Group> GetAll()
        {
            return _unitOfWork.GroupRepository.Get();
        }

        public IEnumerable<Group> GetAllGroup()
        {
            return _unitOfWork.GroupRepository.Get();
        }

        public bool AddGroup(Group group)
        {
            _unitOfWork.GroupRepository.Insert(group);
            return _unitOfWork.Commit();
        }

        public Group GetGroupById(int id)
        {
            return _unitOfWork.GroupRepository.GetByID(id);
        }

        public bool EditGroup(Group group)
        {
            _unitOfWork.GroupRepository.Update(group);
            return _unitOfWork.Commit();
        }

        public bool RemoveGroup(int id)
        {
            Group group = _unitOfWork.GroupRepository.GetByID(id);
            _unitOfWork.GroupRepository.Delete(group);
            return _unitOfWork.Commit();
        }

        public bool DeleteGroup(Group group)
        {
            _unitOfWork.GroupRepository.Delete(group);
            return _unitOfWork.Commit();
        }

        public bool IsDuplicateName(int? id, string name)
        {
            if (id.HasValue)
            {
                return _unitOfWork.GroupRepository.Get(m => m.ID != id && m.Name.ToLower().Equals(name.ToLower())).Any();
            }
            else
            {
                return _unitOfWork.GroupRepository.Get(m => m.Name.ToLower().Equals(name.ToLower())).Any();
            }
        }

        public bool IsInUse(Group group)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.GroupID == group.ID).Any()
                || _unitOfWork.BusinessRuleConditionRepository.Get(
                    m => m.Criteria == ConstantUtil.BusinessRuleCriteria.Group && m.Condition.HasValue && m.Condition.Value == group.ID).Any();
        }
    }
}