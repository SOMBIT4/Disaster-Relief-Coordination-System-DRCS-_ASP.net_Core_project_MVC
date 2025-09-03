using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class DashboardController : Controller
    {
        public IActionResult Admin()
        {
            return View("~/Views/Dashboard/Admin/Index.cshtml");
        }

        public IActionResult Volunteer()
        {
            return View("~/Views/Dashboard/Volunteer/Index.cshtml");
        }

        public IActionResult User()
        {
            return View("~/Views/Dashboard/User/Index.cshtml");
        }
    }
}
