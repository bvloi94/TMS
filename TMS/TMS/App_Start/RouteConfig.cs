using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TMS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "FAQ_Detail",
                url: "FAQ/Detail/{path}",
                defaults: new { controller = "FAQ", action = "Detail", path = UrlParameter.Optional },
                namespaces: new[] { "TMS.Controllers" }
            );

            routes.MapRoute(
                name: "FAQ_Category",
                url: "FAQ/Category/{category}",
                defaults: new { controller = "FAQ", action = "Category", category = UrlParameter.Optional },
                namespaces: new[] { "TMS.Controllers" }
            );

            routes.MapRoute(
                name: "FAQ_Tags",
                url: "FAQ/Tags/{tag}",
                defaults: new { controller = "FAQ", action = "Tags", tag = UrlParameter.Optional },
                namespaces: new[] { "TMS.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "TMS.Controllers" }
            );
        }
    }
}
