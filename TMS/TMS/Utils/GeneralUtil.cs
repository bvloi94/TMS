using System;
using System.Text;

namespace TMS.Utils
{
    public class GeneralUtil
    {
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
            return "Unassigned";
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
                default:
                    return ConstantUtil.TicketModeString.WebForm;
            }
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
                default:
                    return "Updated";
            }
        }
    }
}