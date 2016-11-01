using System;

namespace TMS.Utils
{
    public class SentenceUtil
    {
        public static int Compute(string s, string t)
        {
            if (s.Length == 0 || t.Length == 0)
            {
                return 0;
            }

            if (s.Length >= t.Length)
            {
                t = GeneralUtil.RemoveSpecialCharacters(t);
                s = GeneralUtil.RemoveSpecialCharacters(s);
                string[] tArr = t.Split(' ');
                int similarWords = 0;
                int totalWords = 0;
                foreach (string tItem in tArr)
                {
                    if (!string.IsNullOrWhiteSpace(tItem))
                    {
                        if (s.Contains(tItem.Trim()))
                        {
                            similarWords++;
                        }
                    }
                }
                string[] sArr = s.Split(' ');
                foreach (string sItem in sArr)
                {
                    if (!string.IsNullOrWhiteSpace(sItem))
                    {
                        totalWords++;
                    }
                }
                return Convert.ToInt32(((float)similarWords / totalWords) * 100);
            }
            else
            {
                return Compute(t, s);
            }
        }
    }
}