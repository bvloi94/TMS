using System.Web.Mvc;
using TMS.DAL;
using TMS.Services;

namespace TMS.Controllers
{
    public class ProfileController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        public UserService _userService { get; set; }

        public ProfileController()
        {
            _userService = new UserService(unitOfWork);
        }
        // GET: Profile
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }
    }
}