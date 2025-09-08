using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class UserController : Controller
    {
        public IActionResult AidRequest()
        {
            ViewData["Title"] = "Request For Aid";
            return View("~/Views/Dashboard/User/AidRequest.cshtml");
        }

        public IActionResult Donate()
        {
            ViewData["Title"] = "Donate";
            return View("~/Views/Dashboard/User/Donate.cshtml");
        }

        public IActionResult Donations()
        {
            ViewData["Title"] = "All Donations";
            return View("~/Views/Dashboard/User/Donations.cshtml");
        }

        public IActionResult AidRequests()
        {
            ViewData["Title"] = "All Aid Requests";
            return View("~/Views/Dashboard/User/AidRequests.cshtml");
        }
    }
}
