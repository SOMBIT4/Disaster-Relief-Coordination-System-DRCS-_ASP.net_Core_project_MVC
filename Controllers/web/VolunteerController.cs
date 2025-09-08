using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class VolunteerController : Controller
    {
        public IActionResult Tasks()
        {
            ViewData["Title"] = "Assigned Tasks";
            return View("~/Views/Dashboard/Volunteer/Tasks.cshtml");
        }

        public IActionResult Tracking()
        {
            ViewData["Title"] = "Rescue Tracking Tasks";
            return View("~/Views/Dashboard/Volunteer/Tracking.cshtml");
        }
    }
}
