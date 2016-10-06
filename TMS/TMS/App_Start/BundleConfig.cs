using System.Web;
using System.Web.Optimization;

namespace TMS
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-2.2.3.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/app.js",
                "~/Scripts/main.js",
                "~/Scripts/jquery-ui.min.js",
                "~/Scripts/notify.min.js",
                "~/Content/Plugins/datetimepicker/jquery.datetimepicker.full.min.js",
                "~/Content/Plugins/bootstraptoggle/bootstrap-toggle.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/AdminLTE.min.css",
                      "~/Content/skins/_all-skins.min.css",
                      "~/Content/bootstrap.css.map",
                      "~/Content/font-awesome.min.css",
                      "~/Content/Site.css",
                      "~/Content/Plugins/datetimepicker/jquery.datetimepicker.css",
                      "~/Content/Plugins/bootstraptoggle/bootstrap-toggle.min.css"));
            // Datatable
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                "~/Content/Plugins/datatables/js/jquery.dataTables.min.js",
                "~/Content/Plugins/datatables/js/dataTables.bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/datatables").Include(
                      //"~/Content/Plugins/datatables/css/jquery.dataTables.min.css",
                      "~/Content/Plugins/datatables/css/dataTables.bootstrap.min.css"));
            // Login CSS
            bundles.Add(new StyleBundle("~/Content/login_css").Include(
                      "~/Content/login.css"));
        }
    }
}
