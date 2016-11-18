using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class ImpactViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name length is between 2 and 100 characters")]
        public string Name { get; set; }
        [StringLength(255, ErrorMessage = "Description length is less than 255 characters")]
        public string Description { get; set; }
        public bool IsSystem { get; set; }
    }

    public class UrgencyViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name length is between 2 and 100 characters")]
        public string Name { get; set; }
        [StringLength(255, ErrorMessage = "Description length is less than 255 characters")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Duration is required!")]
        public int Duration { get; set; }
        public string DurationOption { get; set; }
        public bool IsSystem { get; set; }
    }

    public class PriorityViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name length is between 2 and 100 characters")]
        public string Name { get; set; }
        [StringLength(255, ErrorMessage = "Description length is less than 255 characters")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Color is required!")]
        public string Color { get; set; }
        [Required(ErrorMessage = "Level is required!")]
        public int Level { get; set; }
        public bool IsSystem { get; set; }
    }
}