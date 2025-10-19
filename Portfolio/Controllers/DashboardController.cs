using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Admin()
        {
            return View();
        }
    }
}
