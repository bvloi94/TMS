using System;
using System.Text;

namespace TMS.Utils
{
    public class ConstantUtil
    {
        public class UserRole
        {
            public static readonly int Admin = 1;
            public static readonly int HelpDesk = 2;
            public static readonly int Requester = 3;
            public static readonly int Technician = 4;
        }

        public class UserRoleString
        {
            public static readonly string Admin = "Admin";
            public static readonly string HelpDesk = "Helpdesk";
            public static readonly string Requester = "Requester";
            public static readonly string Technician = "Technician";
        }

        public class TicketType
        {
            public static readonly int Request = 1;
            public static readonly int Problem = 2;
            public static readonly int Change = 3;
        }

        public class TicketTypeString
        {
            public static readonly string Request = "Request";
            public static readonly string Problem = "Problem";
            public static readonly string Change = "Change";
        }

        public class TicketMode
        {
            public static readonly int PhoneCall = 1;
            public static readonly int WebForm = 2;
            public static readonly int Email = 3;
        }

        public class TicketModeString
        {
            public static readonly string PhoneCall = "Phone Call";
            public static readonly string WebForm = "Web Form";
            public static readonly string Email = "Email";
        }

        public class TicketStatus
        {
            public static readonly int New = 1;
            public static readonly int Assigned = 2;
            public static readonly int Solved = 3;
            public static readonly int Unapproved = 4;
            public static readonly int Cancelled = 5;
            public static readonly int Closed = 6;
        }
        
        public class ContactEmailInfo
        {
            public static readonly string MailAddress = "huytcd16@gmail.com";
            public static readonly string Password = "huydaivuong";
        }

        public class TypeOfBusinessRuleCondition
        {
            public const int And = 1;
            public const int Or = 2;
        }

        public class CriteriaOfBusinessRuleCondition
        {
            public const string Subject = "Subject";
            public const string Description = "Description";
            public const string RequesterName = "Requester Name";
            public const string Department = "Department";
            public const string Priority = "Priority";
            public const string Impact = "Impact";
            public const string Urgency = "Urgency";
            public const string Category = "Category";
            public const string Mode = "Mode";
        }

        public class ConditionOfBusinessRuleCondition
        {
            public const int Is = 1;
            public const int IsNot = 2;
            public const int BeginsWith = 3;
            public const int EndsWith = 4;
            public const int Contains = 5;
            public const int DoesNotContain = 6;
        }

        public class CategoryLevel
        {
            public const int Category = 1;
            public const int SubCategory = 2;
        }
    }
}