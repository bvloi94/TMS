using System;
using System.Linq;
using System.Text;
using TMS.DAL;
using TMS.Models;
using TMS.Services;

namespace TMS.Utils
{
    public class GeneralUtil
    {
        UnitOfWork _unitOfWork = new UnitOfWork();
        TicketAttachmentService _ticketAttachmentService;

        public GeneralUtil()
        {
            _ticketAttachmentService = new TicketAttachmentService(_unitOfWork);
        }

        public static string GeneratePassword()
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            int length = 8;
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c == ' '))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string ConvertToFormatKeyword(string unformattedKeyword)
        {
            if (!string.IsNullOrWhiteSpace(unformattedKeyword))
            {
                string keyword = "";
                string[] keywordArr = unformattedKeyword.Trim().ToLower().Split(',');
                Array.Sort(keywordArr);
                string delimeter = "";
                foreach (string keywordItem in keywordArr)
                {
                    if (!string.IsNullOrWhiteSpace(keywordItem))
                    {
                        string keywordItemTmp = keywordItem.Trim().Replace(" ", String.Empty);
                        keyword += delimeter + '"' + keywordItemTmp + '"';
                        delimeter = ",";
                    }
                }
                return keyword;
            }
            return null;
        }

        public static string ConvertFormattedKeywordToView(string formattedKeyword)
        {
            if (!string.IsNullOrWhiteSpace(formattedKeyword))
            {
                return formattedKeyword.Replace("\"", "");
            }
            return "";
        }

        public static string GetTypeNameByType(int? type)
        {
            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case ConstantUtil.TicketType.Request:
                        return ConstantUtil.TicketTypeString.Request;
                    case ConstantUtil.TicketType.Problem:
                        return ConstantUtil.TicketTypeString.Problem;
                    case ConstantUtil.TicketType.Change:
                        return ConstantUtil.TicketTypeString.Change;
                }
            }
            return "-";
        }

        public static string GetModeNameByMode(int mode)
        {
            switch (mode)
            {
                case ConstantUtil.TicketMode.Email:
                    return ConstantUtil.TicketModeString.Email;
                case ConstantUtil.TicketMode.PhoneCall:
                    return ConstantUtil.TicketModeString.PhoneCall;
                case ConstantUtil.TicketMode.WebForm:
                    return ConstantUtil.TicketModeString.WebForm;
                case ConstantUtil.TicketMode.SocialNetwork:
                    return ConstantUtil.TicketModeString.SocialNetwork;
                case ConstantUtil.TicketMode.Forum:
                    return ConstantUtil.TicketModeString.Forum;
                case ConstantUtil.TicketMode.Other:
                    return ConstantUtil.TicketModeString.Other;
            }
            return "Unassigned";
        }

        public static string GetTicketHistoryTypeName(int type)
        {
            switch (type)
            {
                case ConstantUtil.TicketHistoryType.Created:
                    return "Created";
                case ConstantUtil.TicketHistoryType.Solved:
                    return "Solved";
                case ConstantUtil.TicketHistoryType.Cancelled:
                    return "Cancelled";
                case ConstantUtil.TicketHistoryType.Closed:
                    return "Closed";
                case ConstantUtil.TicketHistoryType.Merged:
                    return "Merged";
                case ConstantUtil.TicketHistoryType.Unapproved:
                    return "Unapproved";
                case ConstantUtil.TicketHistoryType.Updated:
                    return "Updated";
                case ConstantUtil.TicketHistoryType.Reassigned:
                    return "Reassigned";
            }
            return "Unassigned";
        }

        public static string ShowDateTime(DateTime dateTime)
        {
            DateTime currentDateTime = DateTime.Now;
            int minute = (int)currentDateTime.Subtract(dateTime).TotalMinutes;
            int hour = (int)(minute * 1.0) / 60;
            int day = (int)(hour * 1.0) / 24;

            if (day < 2)
            {
                if (day < 1)
                {
                    if (hour < 1)
                    {
                        if (minute < 1)
                        {
                            return "Just now";
                        }
                        else
                        {
                            return minute == 1 ? "A minute ago" : minute + " minutes ago";
                        }
                    }
                    else
                    {
                        return hour == 1 ? "An hour ago" : hour + " hours ago";
                    }
                }
                else
                {
                    return "Yesterday at " + dateTime.ToShortTimeString();
                }
            }
            else
            {
                return dateTime.ToString("MMM d, yyyy") + " at " + dateTime.ToShortTimeString();
            }
        }

        public static int GetNumberOfTags(string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return 0;
            }
            else
            {
                string[] tagArr = tags.Split(',');
                return tagArr.Count();
            }
        }

        public static string GetTicketStatusByID(int status)
        {
            switch (status)
            {
                case ConstantUtil.TicketStatus.Open:
                    return ConstantUtil.TicketStatusString.Open;
                case ConstantUtil.TicketStatus.Assigned:
                    return ConstantUtil.TicketStatusString.Assigned;
                case ConstantUtil.TicketStatus.Solved:
                    return ConstantUtil.TicketStatusString.Solved;
                case ConstantUtil.TicketStatus.Unapproved:
                    return ConstantUtil.TicketStatusString.Unapproved;
                case ConstantUtil.TicketStatus.Cancelled:
                    return ConstantUtil.TicketStatusString.Cancelled;
                case ConstantUtil.TicketStatus.Closed:
                    return ConstantUtil.TicketStatusString.Closed;
                default:
                    return "Unassigned";
            }
        }

        public static string GetUserInfo(AspNetUser user)
        {
            if (user != null)
            {
                return user.Fullname + " (" + user.Email + ")";
            }
            else
            {
                return "-";
            }
        }

        public static string GetMergedTicketInfo(Ticket mergedTicket)
        {
            if (mergedTicket != null)
            {
                return string.Format("{0} (<a href='/Ticket/TicketDetail/{1}'>#{2}</a>)", mergedTicket.Subject, mergedTicket.ID, mergedTicket.Code);
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetRequesterMergedTicketInfo(Ticket mergedTicket)
        {
            if (mergedTicket != null)
            {
                return string.Format("{0} (<a href='/Ticket/Detail/{1}'>#{2}</a>)", mergedTicket.Subject, mergedTicket.ID, mergedTicket.Code);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}