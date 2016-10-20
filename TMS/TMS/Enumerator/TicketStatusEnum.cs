﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.Enumerator
{
    public enum TicketStatusEnum
    {
        New = 1,
        Assigned = 2,
        Solved = 3,
        Unapproved = 4,
        Canceled = 5,
        Closed = 6,
    }
}