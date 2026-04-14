using CinemaSystem.Utility;
using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class HomeController : Controller
    {

/*        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinema> _cinemaRepository;

        public HomeController(IRepository<Movie> movieRepository, IRepository<Actor> actorRepository, IRepository<Category> categoryRepository, IRepository<Cinema> cinemaRepository)
        {
            _movieRepository = movieRepository;
            _actorRepository = actorRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
        }*/

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
