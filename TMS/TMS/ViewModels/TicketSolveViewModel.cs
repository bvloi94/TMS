using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class TicketSolveViewModel
    {
        public int? ID { get; set; }
        public int? Status { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<DateTime> CreateTime { get; set; }
        public Nullable<DateTime> ModifiedTime { get; set; }
        [Required(ErrorMessage = "Solution cannot be empty!")]
        public string Solution { get; set; }
        public string SolvedBy { get; set; }
    }
}