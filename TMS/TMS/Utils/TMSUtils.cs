using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Microsoft.Ajax.Utilities;
using TMS.ViewModels;

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
                case ConstantUtil.TicketStatus.Open:
                    return "Open";
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

        //public static List<CriteriaViewModel> GetDefaultCritetia()
        //{
        //    List<CriteriaViewModel> criteriaList = new List<CriteriaViewModel>();
        //    FieldInfo[] fields = (new ConstantUtil.BusinessRuleCriteria()).GetType().GetFields();
        //    foreach (var field in fields)
        //    {
        //        CriteriaViewModel crit = new CriteriaViewModel();
        //        crit.Id = (int)field.GetValue(null);
        //        crit.Name = AddSpacesToSentence(field.Name);
        //        criteriaList.Add(crit);
        //    }
        //    return criteriaList;
        //}

        //public static List<ConditionViewModel> GetDefaultCondition()
        //{
        //    List<ConditionViewModel> conditionList = new List<ConditionViewModel>();
        //    FieldInfo[] fields = (new ConstantUtil.ConditionOfBusinessRuleCondition()).GetType().GetFields();
        //    foreach (var field in fields)
        //    {
        //        ConditionViewModel con = new ConditionViewModel();
        //        con.Id = (int)field.GetValue(null);
        //        con.Name = AddSpacesToSentence(field.Name).ToLower();
        //        conditionList.Add(con);
        //    }
        //    return conditionList;
        //}

        public static string AddSpacesToSentence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static List<DropDownViewModel> GetDefaultCritetia()
        {
            List<DropDownViewModel> list = new List<DropDownViewModel>();
            list.Add((new DropDownViewModel("1", "Subject")));
            list.Add((new DropDownViewModel("2", "Description")));
            list.Add((new DropDownViewModel("3", "Requester Name")));
            list.Add((new DropDownViewModel("4", "Department")));
            list.Add((new DropDownViewModel("5", "Priority")));
            list.Add((new DropDownViewModel("6", "Impact")));
            list.Add((new DropDownViewModel("7", "Urgency")));
            list.Add((new DropDownViewModel("8", "Category")));
            list.Add((new DropDownViewModel("9", "Mode")));
            return list;
        }

        public static List<DropDownViewModel> GetDefaultCondition()
        {
            List<DropDownViewModel> list = new List<DropDownViewModel>();
            list.Add((new DropDownViewModel("1", "is")));
            list.Add((new DropDownViewModel("2", "is not")));
            list.Add((new DropDownViewModel("3", "begins with")));
            list.Add((new DropDownViewModel("4", "ends with")));
            list.Add((new DropDownViewModel("5", "contains")));
            list.Add((new DropDownViewModel("6", "does not contain")));
            return list;
        }

        public static List<DropDownViewModel> GetDefaultStatus()
        {
            List<DropDownViewModel> status = new List<DropDownViewModel>();
            status.Add((new DropDownViewModel("1", "Open")));
            status.Add((new DropDownViewModel("2", "Assigned")));
            status.Add((new DropDownViewModel("3", "Solved")));
            status.Add((new DropDownViewModel("4", "Unapproved")));
            status.Add((new DropDownViewModel("5", "Cancelled")));
            status.Add((new DropDownViewModel("6", "Closed")));
            return status;
        }

        public static List<DropDownViewModel> GetDefaultMode()
        {
            List<DropDownViewModel> status = new List<DropDownViewModel>();
            status.Add((new DropDownViewModel(ConstantUtil.TicketMode.PhoneCall.ToString(), ConstantUtil.TicketModeString.PhoneCall)));
            status.Add((new DropDownViewModel(ConstantUtil.TicketMode.WebForm.ToString(), ConstantUtil.TicketModeString.WebForm)));
            status.Add((new DropDownViewModel(ConstantUtil.TicketMode.Email.ToString(), ConstantUtil.TicketModeString.Email)));
            status.Add((new DropDownViewModel(ConstantUtil.TicketMode.SocialNetwork.ToString(), ConstantUtil.TicketModeString.SocialNetwork)));
            status.Add((new DropDownViewModel(ConstantUtil.TicketMode.Forum.ToString(), ConstantUtil.TicketModeString.Forum)));
            status.Add((new DropDownViewModel(ConstantUtil.TicketMode.Other.ToString(), ConstantUtil.TicketModeString.Other)));
            return status;
        }

        public static List<DropDownViewModel> GetDefaultActions()
        {
            List<DropDownViewModel> actions = new List<DropDownViewModel>();
            actions.Add((new DropDownViewModel("1", "Assign to Technician")));
            actions.Add((new DropDownViewModel("2", "Move to Category")));
            actions.Add((new DropDownViewModel("3", "Move to Sub Category")));
            actions.Add((new DropDownViewModel("4", "Move to Item")));
            actions.Add((new DropDownViewModel("5", "Set Priority as")));
            return actions;
        }

        public static int StrToIntDef(string s, int defaultNumber)
        {
            int num;
            if (int.TryParse(s, out num))
                return num;
            return defaultNumber;
        }
    }
}