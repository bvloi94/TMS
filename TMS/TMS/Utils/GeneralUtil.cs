﻿using System;
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

        public static string ShowDateTime(DateTime dateTime)
        {
            DateTime currentDateTime = DateTime.Now;
            int day = currentDateTime.Day - dateTime.Day;
            int hour = currentDateTime.Hour - dateTime.Hour;
            int minute = currentDateTime.Minute - dateTime.Minute;

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
                        return hour == 1 ? hour + "An hour ago" : hour + " hours ago";
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
    }
}