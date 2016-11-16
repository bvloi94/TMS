using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class DropDownViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public DropDownViewModel() { }

        public DropDownViewModel(string Id, string name)
        {
            this.Id = Id;
            this.Name = name;
        }
    }
}