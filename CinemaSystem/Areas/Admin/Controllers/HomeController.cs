using CinemaSystem.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE} , {SD.EMPLOYEE_ROLE}")]
    public class HomeController : Controller
    {


        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult NotFoundPage()
        {
            return View();
        }
        public IActionResult StatisticsPage()
        {
            return View(new StatisticsPageVM()
            {
                MoviesCount = _context.Movies.Count(),
                ActorsCount = _context.Actors.Count(),
                CinemasCount = _context.Cinemas.Count(),
                CategoriesCount = _context.Categories.Count()
            });

        }

    }
}
