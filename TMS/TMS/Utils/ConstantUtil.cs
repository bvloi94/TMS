using System;
using System.Text;

namespace TMS.Utils
{
    public class ConstantUtil
    {
        public static readonly string DateTimeFormat = "dd/MM/yyyy HH:mm";
        public static readonly string DateTimeFormat2 = "MMMM dd, yyyy HH:mm";
        public static readonly string DateTimeFormat3 = "MMM d yyyy, HH:mm";
        public static readonly string DateFormat = "dd/MM/yyyy";
        public const int DayToCloseTicket = 3;

        public class UserRole
        {
            public static readonly int Admin = 1;
            public static readonly int HelpDesk = 2;
            public static readonly int Requester = 3;
            public static readonly int Technician = 4;
            public static readonly int Manager = 5;
        }

        public class UserRoleString
        {
            public static readonly string Admin = "Admin";
            public static readonly string HelpDesk = "Helpdesk";
            public static readonly string Requester = "Requester";
            public static readonly string Technician = "Technician";
            public static readonly string Manager = "Manager";
        }

        public class TicketType
        {
            public const int Request = 1;
            public const int Problem = 2;
            public const int Change = 3;
        }

        public class TicketTypeValue
        {
            public const int Request = 1;
            public const int Problem = 2;
            public const int Change = 3;
            public const int PendingRequest = 4;
            public const int PendingProblem = 5;
            public const int PendingChange = 6;
        }

        public class TicketTypeString
        {
            public static readonly string Request = "Request";
            public static readonly string Problem = "Problem";
            public static readonly string Change = "Change";
        }

        public class TicketMode
        {
            public const int PhoneCall = 1;
            public const int WebForm = 2;
            public const int Email = 3;
            public const int SocialNetwork = 4;
            public const int Forum = 5;
            public const int Other = 6;
        }

        public class TicketModeString
        {
            public static readonly string PhoneCall = "Phone Call";
            public static readonly string WebForm = "Web Form";
            public static readonly string Email = "Email";
            public static readonly string SocialNetwork = "Social Network";
            public static readonly string Forum = "Forum";
            public static readonly string Other = "Other";
        }

        public class TicketStatus
        {
            public const int Open = 1;
            public const int Assigned = 2;
            public const int Solved = 3;
            public const int Unapproved = 4;
            public const int Cancelled = 5;
            public const int Closed = 6;
        }

        public class TicketStatusString
        {
            public const string Open = "Open";
            public const string Assigned = "Assigned";
            public const string Solved = "Solved";
            public const string Unapproved = "Unapproved";
            public const string Cancelled = "Cancelled";
            public const string Closed = "Closed";
        }

        public class ContactEmailInfo
        {
            public static readonly string MailAddress = "tms.g4.hotro@gmail.com";
            public static readonly string Password = "mdx4kISu";
        }

        public class TypeOfBusinessRuleCondition
        {
            public const int And = 1;
            public const int Or = 2;
        }

        public class CriteriaOfBusinessRuleCondition
        {
            public const int Subject = 1;
            public const int Description = 2;
            public const int RequesterName = 3;
            public const int Group = 4;
            public const int Priority = 5;
            public const int Impact = 6;
            public const int Urgency = 7;
            public const int Category = 8;
            public const int Mode = 9;
        }

        public class BusinessRuleCriteria
        {
            public const int Subject = 1;
            public const int Description = 2;
            public const int RequesterName = 3;
            public const int Group = 4;
            public const int Priority = 5;
            public const int Impact = 6;
            public const int Urgency = 7;
            public const int Category = 8;
            public const int Mode = 9;
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
            public const int Item = 3;
        }

        public class TicketAttachmentType
        {
            public const bool Description = true;
            public const bool Solution = false;
        }

        public class CommonError
        {
            public const string DBExceptionError = "Some error occured! Please try again later!";
            public const string UnavailableTicket = "This ticket is not available!";
            public const string UnavailableTechnician = "This technician is not available!";
            public const string UnavailableUser = "This user is not available!";
            public const string InvalidTicket = "This ticket is invalid!";

        }

        public class TimeOption
        {
            public const int Today = 1;
            public const int FourDaysAgo = 2;
            public const int ThisWeek = 3;
            public const int ThisMonth = 4;
            public const int ThisYear = 5;
        }

        public class TicketCodeTemplate
        {
            public const int Length = 8;
            public const string FirstLetter = "TK";
            public const string CharacterTemplate = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            public const string NumberTemplate = "0123456789";
        }

        public class Attachment
        {
            public const int NameLength = 30;
            public const string NameReplace = "....";
        }

        public class TicketHistoryType
        {
            public const int Created = 1;
            public const int Updated = 2;
            public const int Solved = 3;
            public const int Unapproved = 4;
            public const int Cancelled = 5;
            public const int Closed = 6;
            public const int Merged = 7;
            public const int Reassigned = 8;
        }

        public class BusinessRuleTrigger
        {
            public const int AssignToTechnician = 1;
            public const int PlaceInGroup = 2;
            public const int MoveToCategory = 3;
            public const int MoveToSubCategory = 4;
            public const int MoveToItem = 5;
            //public const int SetPriorityAs = 6;
        }

        public class NotificationActionType
        {
            public const int RequesterNotiCreate = 1;
            public const int RequesterNotiCancel = 2;
            public const int RequesterNotiSolve = 3;
            public const int RequesterNotiClose = 4;
            public const int RequesterNotiIsMerged = 5;
            public const int HelpDeskNotiCreate = 6;
            public const int HelpDeskNotiUnapprove = 7;
            public const int TechnicianNotiAssign = 8;
            public const int TechnicianNotiUnassign = 9;
            public const int TechnicianNotiReassign = 10;
            public const int TechnicianNotiCancel = 11;
            public const int TechnicianNotiMerge = 12;
            public const int TechnicianNotiIsMerged = 13;
            public const int TechnicianNotiChangeDueByDate = 14;
        }
    }
}