using System.Diagnostics.CodeAnalysis;

namespace TMS.ViewModels
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class JqueryDatatableParameterViewModel
    {
        /// <summary>
        /// Information for DataTables to use for rendering.
        /// </summary>
        public string sEcho { get; set; }
        /// <summary>
        /// Display start point in the current data set.
        /// </summary>
        public int iDisplayStart { get; set; }
        /// <summary>
        /// Number of records that the table can display 
        /// in the current draw. It is expected that the 
        /// number of records returned will be equal to 
        /// this number, unless the server has fewer 
        /// records to return.
        /// </summary>
        public int iDisplayLength { get; set; }
        /// <summary>
        /// Number of columns being displayed (useful 
        /// for getting individual column search info)
        /// </summary>
        public int iColumns { get; set; }
        /// <summary>
        /// Global search field
        /// </summary>
        public string sSearch { get; set; }
        /// <summary>
        /// Direction to be sorted for column 0 - "desc" or "asc".
        /// </summary>
        public string sSortDir_0 { get; set; }
        /// <summary>
        /// Column being sorted on (you will need to decode this 
        /// number for your database)
        /// </summary>
        public int iSortCol_0 { get; set; }
        /// <summary>
        /// Number of columns to sort on
        /// </summary>
        public int iSortingCols { get; set; }
    }
}