using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class VolunteerController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Items["role"]?.ToString() ?? "";
            if (role != "Volunteer")
                return Unauthorized();

            return View(); // Views/Dashboard/Volunteer/Index.cshtml
        }
    }
}
