using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class NotificationService
    {
        private readonly UnitOfWork _unitOfWork;

        public NotificationService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Notification> GetAll()
        {
            return _unitOfWork.NotificationRepository.Get();
        }

        public Notification GetNotificationById(int id)
        {
            return _unitOfWork.NotificationRepository.GetByID(id);
        }

        public IEnumerable<Notification> GetUserNotifications(string id)
        {
            return _unitOfWork.NotificationRepository.Get().Where(m => m.BeNotifiedID == id);
        }

        public bool EditNotification(Notification notification)
        {
            _unitOfWork.NotificationRepository.Update(notification);
            return _unitOfWork.Commit();
        }
    }
}