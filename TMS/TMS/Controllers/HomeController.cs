using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;

namespace TMS.Controllers
{
    public class HomeController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        public UserService _userService { get; set; }

        public HomeController()
        {
            _userService = new UserService(unitOfWork);
        }

        public ActionResult Index()
        {
            var name = User.Identity.Name;      
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser requester = _userService.GetUserById(id);
            if (requester != null)
            {
                ViewBag.LayoutName = requester.Fullname;
                ViewBag.LayoutAvatarURL = requester.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}