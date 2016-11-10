using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class TriggerViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Mask { get; set; }

        public TriggerViewModel() { }

        public TriggerViewModel(int? Id, string name, string mask)
        {
            this.Id = Id;
            this.Name = name;
            this.Mask = mask;
        }
    }
}