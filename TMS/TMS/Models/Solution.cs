//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TMS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Solution
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Solution()
        {
            this.SolutionAttachments = new HashSet<SolutionAttachment>();
            this.SolutionKeywords = new HashSet<SolutionKeyword>();
        }
    
        public int ID { get; set; }
        public string Subject { get; set; }
        public string ContentText { get; set; }
        public int CategoryID { get; set; }
        public string Path { get; set; }
        public System.DateTime CreatedTime { get; set; }
        public System.DateTime ModifiedTime { get; set; }
        public bool IsPublish { get; set; }
    
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SolutionAttachment> SolutionAttachments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SolutionKeyword> SolutionKeywords { get; set; }
    }
}
