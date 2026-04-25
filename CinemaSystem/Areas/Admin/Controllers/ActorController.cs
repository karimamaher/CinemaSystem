using CinemaSystem.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ActorSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE} , {SD.EMPLOYEE_ROLE}")]
    public class ActorController : Controller
    {
        private readonly IRepository<Actor> _repository;
        private readonly IRepository<Movie> _movieRepository;

        public ActorController(IRepository<Actor> repository, IRepository<Movie> movieRepository)
        {
            _repository = repository;
            _movieRepository = movieRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {

            //var Actors = _context.Actors.Include(e => e.Movie).AsQueryable();
            var Actors =await _repository.GetAsync(includes:[e =>e.Movie], cancellationToken: cancellationToken);
            //filter
            if (query is not null)
            {
                Actors = Actors.Where(e => e.Name.ToLower().Contains(query.Trim().ToLower()));
                ViewBag.Query = query;
            }

            //

            //pagination
            double totalPages = Math.Ceiling(Actors.Count() / 3.0);
            Actors = Actors.Skip((page - 1) * 3).Take(3);


            return View(new ActorsVM()
            {
                Actors = Actors.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create( CancellationToken cancellationToken = default)
        {
            ViewBag.Movies =await _movieRepository.GetAsync(cancellationToken: cancellationToken);
            return View(new Actor());
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Actor Actor, IFormFile Img, CancellationToken cancellationToken = default)
        {
            ModelState.Remove("Img");
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = await _movieRepository.GetAsync(cancellationToken: cancellationToken);
                return View(Actor);
            }


            if (Img is not null && Img.Length > 0)
            {
                var fileName = CreateFile(Img);
                Actor.Img = fileName;
            }
           await _repository.CreateAsync(Actor, cancellationToken);
          await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Add Actor Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            // var Actors = _context.Actors.Find(id);
            var Actors = await _repository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (Actors is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            ViewBag.Movies = await _movieRepository.GetAsync(cancellationToken: cancellationToken);
            return View(Actors);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(Actor Actor, IFormFile Img, CancellationToken cancellationToken = default)
        {



            // var ActorInDB = _context.Actors.AsNoTracking().SingleOrDefault(e => e.Id == Actor.Id);
            var ActorInDB = await _repository.GetOneAsync(e => e.Id == Actor.Id, tracked: false, cancellationToken: cancellationToken);
            if (ActorInDB is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            if (Img is not null && Img.Length > 0)
            {
                // 1. Create new img in wwwroot
                var fileName = CreateFile(Img);

                //2.Delete old img from wwwroot
                var oldFilePath = GetOldFilePath(ActorInDB.Img);
                if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                //3 update img in DB
                Actor.Img = fileName;
            }
            else
                Actor.Img = ActorInDB.Img;


            ModelState.Remove("Img");
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = await _movieRepository.GetAsync(cancellationToken: cancellationToken);
                return View(Actor);
            }


            _repository.Update(Actor);
          await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Update Actor Successfully";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            // var Actor = _context.Actors.Find(id);
            var Actor = await _repository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (Actor is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            //2.Delete old img from wwwroot
            var oldFilePath = GetOldFilePath(Actor.Img);
            if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _repository.Delete(Actor);
          await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Delete Actor Successfully";

            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {

            // var actor = _context.Actors.Include(e => e.Movie)
            //   .FirstOrDefault(e => e.Id == id);
            var actor =await _repository.GetOneAsync(expression: e => e.Id == id, includes: [e => e.Movie], cancellationToken: cancellationToken);

            if (actor is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            return View(actor);
        }

        //service methods
        private string CreateFile(IFormFile Img)
        {
            var fileName = $"{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")}-{Guid.NewGuid().ToString()}{Path.GetExtension(Img.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\actors", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                Img.CopyTo(stream);
            }
            return fileName;

        }
        private string GetOldFilePath(string oldFileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\actors", oldFileName);
            return filePath;

        }
    }
}
