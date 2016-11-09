using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class CategoryViewModel
    {
        public int? ID { get; set; }
        [Required(ErrorMessage = "Name is required!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ("Name length is between 2 and 100 characters!"))]
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Level { get; set; }
        public int? ParentId { get; set; }
        public ICollection<CategoryViewModel> Categories { get; set; }
    }
}