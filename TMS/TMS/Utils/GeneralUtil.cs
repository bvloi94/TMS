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
    }
}