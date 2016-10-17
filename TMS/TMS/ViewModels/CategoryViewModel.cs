using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class CategoryViewModel
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Level { get; set; }
        public int? ParentId { get; set; }
        public ICollection<CategoryViewModel> Categories { get; set; }
    }
}