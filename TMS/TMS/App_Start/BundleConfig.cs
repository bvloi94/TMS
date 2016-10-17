﻿using System.Web;
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
                      "~/Content/Plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.all.min.js",
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/app.js",
                "~/Scripts/main.js",
                "~/Scripts/jquery-ui.min.js",
                "~/Content/js/jquery.noty.packaged.js",
                "~/Scripts/notify.min.js",
                "~/Content/Plugins/datetimepicker/jquery.datetimepicker.full.min.js",
                "~/Content/Plugins/bootstraptoggle/bootstrap-toggle.min.js",
                "~/Scripts/globalize.js",
                "~/Scripts/cldr.js",
                "~/Scripts/jquery.cookie.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/AdminLTE.min.css",
                      "~/Content/skins/_all-skins.min.css",
                      "~/Content/bootstrap.css.map",
                      "~/Content/font-awesome.min.css",
                      "~/Content/Site.css",
                      "~/Content/Plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css",
                      "~/Content/Plugins/datetimepicker/jquery.datetimepicker.css",
                      "~/Content/Plugins/bootstraptoggle/bootstrap-toggle.min.css"));
            // Datatable
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                "~/Content/Plugins/datatables/js/jquery.dataTables.min.js",
                "~/Content/Plugins/datatables/js/dataTables.bootstrap.js",
                "~/Content/Plugins/datatables/js/dataTables.responsive.min.js",
                "~/Content/Plugins/datatables/js/responsive.bootstrap.min.js"));

            bundles.Add(new StyleBundle("~/Content/datatables").Include(
                      //"~/Content/Plugins/datatables/css/jquery.dataTables.min.css",
                      "~/Content/Plugins/datatables/css/dataTables.bootstrap.min.css",
                      "~/Content/Plugins/datatables/css/responsive.dataTables.min.css"));
            // Login CSS
            bundles.Add(new StyleBundle("~/Content/login_css").Include(
                      "~/Content/login.css"));

            bundles.Add(new ScriptBundle("~/bundles/helpdesk-manage-ticket").Include(
               "~/Scripts/helpdesk-manage-ticket.js"));
        }
    }
}
