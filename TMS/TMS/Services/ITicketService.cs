﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Models;

namespace TMS.Services
{
    interface ITicketService
    {
        IEnumerable<Ticket> GetAll();
    }
}
