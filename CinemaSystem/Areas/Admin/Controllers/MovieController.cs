using CinemaSystem.Areas.Admin.Controllers;
using CinemaSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using static CinemaSystem.ViewModels.MovieFilterVM;

namespace MovieSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MovieService _movieService;

        public MovieController()
        {
            _context = new ApplicationDbContext();
            _movieService = new();
        }
        public IActionResult Index(MovieFilterVM movieFilterVM, int page = 1)
        {

            var Movies = _context.Movies.Include(e=>e.Actors).AsQueryable();
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
                Categories = _context.Categories.AsEnumerable(),
                Cinemas = _context.Cinemas.AsEnumerable()
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.AsEnumerable();
            ViewBag.Cinemas = _context.Cinemas.AsEnumerable();
            return View();
        }
        [HttpPost]
        public IActionResult Create(Movie movie , IFormFile Img, List<IFormFile>? SubImgs)
        {
            if (Img is not null && Img.Length > 0)
            {
                var fileName = _movieService.CreateFile(Img);
                movie.MainImg = fileName;
            }

            _context.Movies.Add(movie);
            _context.SaveChanges();

            if (SubImgs is not null)
            {
                foreach (var item in SubImgs)
                {
                    if (item is not null && item.Length > 0)
                    {
                        var fileName = _movieService.CreateFile(item, ImageType.SubImg);
                        _context.MovieSubImgs.Add(new()
                        {
                            MovieId = movie.Id,
                            SubImg = fileName
                        });
                    }
                }
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var Movie = _context.Movies.Find(id);
            if (Movie is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            return View(new MovieWithSubImgsVM()
            {
                Movie = Movie, 
                MovieSubImgs = _context.MovieSubImgs.Where(e=>e.MovieId== id),
                Categories = _context.Categories.AsEnumerable(),
                Cinemas = _context.Cinemas.AsEnumerable()
            });
        }

        [HttpPost]
        public IActionResult Update(Movie Movie , IFormFile Img, List<IFormFile>? SubImgs)
        {
            var MovieInDB = _context.Movies.AsNoTracking().SingleOrDefault(e => e.Id == Movie    .Id);
            if (MovieInDB is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            if (Img is not null && Img.Length > 0)
            {
                // 1. Create new img in wwwroot
                var fileName = _movieService.CreateFile(Img);

                //2.Delete old img from wwwroot
                var oldFilePath = _movieService.GetOldFilePath(MovieInDB.MainImg);
                if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                //3 update img in DB
                Movie.MainImg = fileName;
            }
            else
                Movie.MainImg = MovieInDB.MainImg;

            _context.Movies.Update(Movie);
            _context.SaveChanges();

            if (SubImgs is not null)
            {
                var movieSubImgs = _context.MovieSubImgs.Where(e => e.MovieId == Movie.Id);

                foreach (var item in movieSubImgs)
                {
                    var oldFilePath = _movieService.GetOldFilePath(item.SubImg, ImageType.SubImg);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                _context.MovieSubImgs.RemoveRange(movieSubImgs);

                foreach (var item in SubImgs)
                {
                    if (item is not null && item.Length > 0)
                    {
                        var fileName = _movieService.CreateFile(item, ImageType.SubImg);
                        _context.MovieSubImgs.Add(new()
                        {
                            MovieId = Movie.Id,
                            SubImg = fileName
                        });
                    }
                }
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var Movie= _context.Movies.Find(id);
            if (Movie is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            //2.Delete old img from wwwroot
            var oldFilePath = _movieService.GetOldFilePath(Movie.MainImg);
            if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _context.Movies.Remove(Movie);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        //service methods

    }
}
