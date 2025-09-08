using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class AdminController : Controller
    {
        public IActionResult Users()
        {
            ViewData["Title"] = "All Users";
            return View("~/Views/Dashboard/Admin/Users.cshtml");
        }

        public IActionResult Areas()
        {
            ViewData["Title"] = "All Affected Areas";
            return View("~/Views/Dashboard/Admin/Areas.cshtml");
        }

        public IActionResult ReliefCenters()
        {
            ViewData["Title"] = "All Relief Centers";
            return View("~/Views/Dashboard/Admin/ReliefCenters.cshtml");
        }

        public IActionResult AidRequests()
        {
            ViewData["Title"] = "All Aid Requests";
            return View("~/Views/Dashboard/Admin/AidRequests.cshtml");
        }

        public IActionResult AidPreparation()
        {
            ViewData["Title"] = "Aid Preparation";
            return View("~/Views/Dashboard/Admin/AidPreparation.cshtml");
        }
    }
}
