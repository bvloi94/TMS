﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class RequesterTicketViewModel
    {
        public int? ID { get; set; }
        public string Status { get; set; }
        [Required(ErrorMessage = "Subject cannot be empty!")]
        public string Subject { get; set; }
        public string Description { get; set; }
        public string AttachmentURL { get; set; }
        public string CreatedBy { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public Nullable<DateTime> CreateTime { get; set; }
        public Nullable<DateTime> ModifiedTime { get; set; }
        public string Solution { get; set; }
        public string SolvedBy { get; set; }
        public string UnapproveReason { get; set; }
    }
}