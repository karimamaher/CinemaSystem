using CinemaSystem.Areas.Admin.Controllers;
using CinemaSystem.Models;
using CinemaSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static CinemaSystem.ViewModels.MovieFilterVM;

namespace MovieSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE} , {SD.EMPLOYEE_ROLE}")]
    public class MovieController : Controller
    {
        private readonly IRepository<Movie> _repository;
        private readonly IMovieSubImgRepository _movieSubImgRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IMovieService _movieService;

        public MovieController(IRepository<Movie> repository, IMovieSubImgRepository movieSubImgRepository, IRepository<Category> categoryRepository, IRepository<Cinema> cinemaRepository, IMovieService movieService)
        {
            _repository = repository;
            _movieSubImgRepository = movieSubImgRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _movieService = movieService;
        }

        public async Task<IActionResult> Index(MovieFilterVM movieFilterVM, int page = 1, CancellationToken cancellationToken = default)
        {

            /*           var Movies = _context.Movies
                        .Include(e=>e.Actors)
                         .Include(e => e.Category)
                            .Include(e => e.Cinema)
                        .AsQueryable();*/

            var Movies = await _repository.GetAsync(includes: [e => e.Category, e => e.Cinema , e => e.Actors], cancellationToken: cancellationToken);

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
                // Categories = _context.Categories.AsEnumerable(),
                //            Cinemas = _context.Cinemas.AsEnumerable()
                Categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken),
                Cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken),
            });
        }


        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create( CancellationToken cancellationToken = default)
        {
            ViewBag.Categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken);
            ViewBag.Cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken);
            return View(new Movie());
        }


        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Movie movie , IFormFile Img, List<IFormFile>? SubImgs, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) 
            {
                ViewBag.Categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken);
                ViewBag.Cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken);
                return View(movie);
            }
               


            if (Img is not null && Img.Length > 0)
            {
                var fileName =await _movieService.CreateFileAsync(Img);
                movie.MainImg = fileName;
            }

            await  _repository.CreateAsync(movie, cancellationToken);
               await _repository.CommitAsync(cancellationToken: cancellationToken);

            if (SubImgs is not null)
            {
                foreach (var item in SubImgs)
                {
                    if (item is not null && item.Length > 0)
                    {
                        var fileName =await _movieService.CreateFileAsync(item, ImageType.SubImg);
                      await  _movieSubImgRepository.CreateAsync(new()
                        {
                            MovieId = movie.Id,
                            SubImg = fileName
                        }, cancellationToken: cancellationToken);
                    }
                }
                   await _repository.CommitAsync(cancellationToken: cancellationToken);
            }

            TempData["success-notification"] = "Add Movie Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            // var Movie = _context.Movies.Find(id);
            var Movie = await _repository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (Movie is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            return View(new MovieWithSubImgsVM()
            {
                Movie = Movie, 
                MovieSubImgs = await _movieSubImgRepository.GetAsync(e=>e.MovieId== id, cancellationToken: cancellationToken),
                Categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken),
                Cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken)
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(Movie Movie , IFormFile Img, List<IFormFile>? SubImgs, CancellationToken cancellationToken = default)
        {

            // var MovieInDB = _context.Movies.AsNoTracking().SingleOrDefault(e => e.Id == Movie.Id);
            var MovieInDB = await _repository.GetOneAsync(e => e.Id == Movie.Id, tracked: false, cancellationToken: cancellationToken);
            if (MovieInDB is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            if (Img is not null && Img.Length > 0)
            {
                // 1. Create new img in wwwroot
                var fileName =await _movieService.CreateFileAsync(Img);

                //2.Delete old img from wwwroot
                var oldFilePath = _movieService.GetOldFilePath(MovieInDB.MainImg);
                if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                //3 update img in DB
                Movie.MainImg = fileName;
            }
            else
                Movie.MainImg = MovieInDB.MainImg;

            ModelState.Remove("Img");
            if (!ModelState.IsValid)
                return View(new MovieWithSubImgsVM()
                {
                    Movie = Movie,
                    MovieSubImgs =await _movieSubImgRepository.GetAsync(e => e.MovieId == Movie.Id ,cancellationToken:cancellationToken),
                    Categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken),
                    Cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken)
                });


           _repository.Update(Movie);
               await _repository.CommitAsync(cancellationToken: cancellationToken);

            if (SubImgs is not null)
            {
                var movieSubImages =await _movieSubImgRepository.GetAsync(e => e.MovieId == Movie.Id, cancellationToken: cancellationToken);

                foreach (var item in movieSubImages)
                {
                    var oldFilePath = _movieService.GetOldFilePath(item.SubImg, ImageType.SubImg);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
              _movieSubImgRepository.DeleteRange(movieSubImages);

                foreach (var item in SubImgs)
                {
                    if (item is not null && item.Length > 0)
                    {
                        var fileName =await _movieService.CreateFileAsync(item, ImageType.SubImg);
                       await _movieSubImgRepository.CreateAsync(new()
                        {
                            MovieId = Movie.Id,
                            SubImg = fileName
                        }, cancellationToken: cancellationToken);
                    }
                }
                   await _repository.CommitAsync(cancellationToken: cancellationToken);
            }

            TempData["success-notification"] = "Update Movie Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            // var Movie= _context.Movies.Find(id);
            var Movie = await _repository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (Movie is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            //2.Delete old img from wwwroot
            var oldFilePath = _movieService.GetOldFilePath(Movie.MainImg);
            if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _repository.Delete(Movie);
               await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Delete Movie Successfully";

            return RedirectToAction(nameof(Index));

        }



    }
}
