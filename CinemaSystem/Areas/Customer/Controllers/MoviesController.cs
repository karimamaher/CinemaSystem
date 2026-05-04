using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CinemaSystem.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class MoviesController : Controller
    {
        private readonly IRepository<Movie> _repository;
        private readonly IRepository<ShowTime> _showTimeRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinema> _cinemaRepository;

        public MoviesController(IRepository<Movie> repository,
            IRepository<ShowTime> showTimeRepository, 
            IRepository<Category> categoryRepository, 
            IRepository<Cinema> cinemaRepository)
        {
            _repository = repository;
            _showTimeRepository = showTimeRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
        }

        public async Task<IActionResult> Index(MovieFilterVM movieFilterVM, int page = 1, CancellationToken cancellationToken = default)
        {

            var Movies = await _repository.GetAsync(includes: [e => e.Category, e => e.Cinema, e => e.Actors], cancellationToken: cancellationToken);

            //filter
            if (movieFilterVM.Name is not null)
            {
                Movies = Movies.Where(e => e.Name.ToLower().Contains(movieFilterVM.Name.Trim().ToLower()));
                ViewBag.Name = movieFilterVM.Name;
            }
            //movie Category  filter
            if (movieFilterVM.CategoryId is not null)
            {
                Movies = Movies.Where(e => e.CategoryId == movieFilterVM.CategoryId);
                ViewBag.CategoryId = movieFilterVM.CategoryId;
            }
            //movie Brand  filter
            if (movieFilterVM.CinemaId is not null)
            {
                Movies = Movies.Where(e => e.CinemaId == movieFilterVM.CinemaId);
                ViewBag.CinemaId = movieFilterVM.CinemaId;
            }

            //pagination
            double totalPages = Math.Ceiling(Movies.Count() / 3.0);
            Movies = Movies.Skip((page - 1) * 3).Take(3);


            return View(new MoviesVM()
            {
                Movies = Movies.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
                Categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken),
                Cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken),
            });
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default) {

            var movie = await _repository.GetOneAsync(e => e.Id == id ,
                includes: [e=>e.Cinema, e => e.Category, e => e.Actors] ,cancellationToken:cancellationToken);

            if (movie is null) return NotFound();

            var showTimes= await _showTimeRepository.GetAsync(e=>e.MovieId==id ,
               includes: [e => e.Movie , e => e.Hall]  ,cancellationToken: cancellationToken);

            return View(new MovieDetailsVM
            {
                Movie = movie,
                ShowTimes = showTimes
            });
        }
    }


}
