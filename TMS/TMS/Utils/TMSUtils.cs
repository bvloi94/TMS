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

    }
}