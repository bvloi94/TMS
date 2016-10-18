using System.Diagnostics.CodeAnalysis;

namespace TMS.ViewModels
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class JqueryDatatableResultViewModel
    {
        /// <summary>
        /// An unaltered copy of sEcho sent from the 
        /// client side. This parameter will change 
        /// with each draw (it is basically a draw 
        /// count) - so it is important that this is 
        /// implemented. Note that it strongly 
        /// recommended for security reasons that 
        /// you 'cast' this parameter to an integer 
        /// in order to prevent Cross Site Scripting 
        /// (XSS) attacks.
        /// </summary>
        public int draw { get; set; }
        /// <summary>
        /// Total records, before filtering (i.e. the 
        /// total number of records in the database)
        /// </summary>
        public int recordsTotal { get; set; }
        /// <summary>
        /// Total records, after filtering (i.e. the 
        /// total number of records after filtering 
        /// has been applied - not just the number 
        /// of records being returned in this result 
        /// set)
        /// </summary>
        public int recordsFiltered { get; set; }
        /// <summary>
        /// The data in a 2D array. Note that you can 
        /// change the name of this parameter with 
        /// sAjaxDataProp.
        /// </summary>
        public object data { get; set; }
    }
}