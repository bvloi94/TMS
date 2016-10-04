using System.Web.Mvc;

namespace TMS.Areas.HelpDesk
{
    public class HelpDeskAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "HelpDesk";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HelpDesk_default",
                "HelpDesk/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "TMS.Areas.HelpDesk.Controllers" }
            );
        }
    }
}