using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class GroupViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Name is required!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name length is between 2 and 100 characters")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Categories { get; set; }
        public IEnumerable<GroupCategoryViewModel> GroupCategories { get; set; }
    }

    public class GroupCategoryViewModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
    }
}