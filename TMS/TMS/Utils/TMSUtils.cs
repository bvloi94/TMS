using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;

namespace TMS.Utils
{
    public class TMSUtils
    {
        public static string ConvertModeFromInt(int mode)
        {
            switch (mode)
            {
                case ConstantUtil.TicketMode.PhoneCall:
                    return ConstantUtil.TicketModeString.PhoneCall;
                case ConstantUtil.TicketMode.WebForm:
                    return ConstantUtil.TicketModeString.WebForm;
                case ConstantUtil.TicketMode.Email:
                    return ConstantUtil.TicketModeString.Email;
                case ConstantUtil.TicketMode.SocialNetwork:
                    return ConstantUtil.TicketModeString.SocialNetwork;
                case ConstantUtil.TicketMode.Forum:
                    return ConstantUtil.TicketModeString.Forum;
                case ConstantUtil.TicketMode.Other:
                    return ConstantUtil.TicketModeString.Other;
                default:
                    return "-";
            }
        }

        public static string ConvertTypeFromInt(int? type)
        {
            switch (type)
            {
                case ConstantUtil.TicketType.Request:
                    return ConstantUtil.TicketTypeString.Request;
                case ConstantUtil.TicketType.Problem:
                    return ConstantUtil.TicketTypeString.Problem;
                case ConstantUtil.TicketType.Change:
                    return ConstantUtil.TicketTypeString.Change;
                default:
                    return "-";
            }
        }

        public static string ConvertStatusFromInt(int? status)
        {
            switch (status)
            {
                case ConstantUtil.TicketStatus.New:
                    return "New";
                case ConstantUtil.TicketStatus.Assigned:
                    return "Assigned";
                case ConstantUtil.TicketStatus.Solved:
                    return "Solved";
                case ConstantUtil.TicketStatus.Unapproved:
                    return "Unapproved";
                case ConstantUtil.TicketStatus.Cancelled:
                    return "Cancelled";
                case ConstantUtil.TicketStatus.Closed:
                    return "Closed";
                default:
                    return "-";
            }
        }

        public static string GetMinimizedAttachmentName(string fileName)
        {
            if (fileName.Length > ConstantUtil.Attachment.NameLength)
            {
                string ext = fileName.Split('.').Last();
                return fileName.Substring(0, ConstantUtil.Attachment.NameLength - ConstantUtil.Attachment.NameReplace.Length - ext.Length) + ConstantUtil.Attachment.NameReplace + ext;
            }
            return fileName;
        }

        public static List<string> GetFieldsOfClass(object pObject)
        {
            List<string> propertyList = new List<string>();
            if (pObject != null)
            {
                foreach (var prop in pObject.GetType().GetFields())
                {
                    propertyList.Add(prop.Name);
                }
            }
            return propertyList;
        }

    }
}