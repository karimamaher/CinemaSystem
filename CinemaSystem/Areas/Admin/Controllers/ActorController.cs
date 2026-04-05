using CinemaSystem.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActorSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ActorController()
        {
            _context = new ApplicationDbContext();
        }
        public IActionResult Index(int page = 1, string? query = null)
        {

            var Actors = _context.Actors.Include(e => e.Movie).AsQueryable();
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
        public IActionResult Create()
        {
            ViewBag.Movies = _context.Movies.AsEnumerable();
            return View();
        }
        [HttpPost]
        public IActionResult Create(Actor Actor , IFormFile Img)
        {
            if (Img is not null && Img.Length > 0)
            {
                var fileName = CreateFile(Img);
                Actor.Img = fileName;
            }
            _context.Actors.Add(Actor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var Actors = _context.Actors.Find(id);
            if (Actors is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            ViewBag.Movies = _context.Movies.AsEnumerable();
            return View(Actors);
        }

        [HttpPost]
        public IActionResult Update(Actor Actor , IFormFile Img)
        {
            var ActorInDB = _context.Actors.AsNoTracking().SingleOrDefault(e => e.Id == Actor    .Id);
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

            _context.Actors.Update(Actor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var Actor= _context.Actors.Find(id);
            if (Actor is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            //2.Delete old img from wwwroot
            var oldFilePath = GetOldFilePath(Actor.Img);
            if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _context.Actors.Remove(Actor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }
        public IActionResult Details(int id)
        {

            var actor = _context.Actors.Include(e=>e.Movie)
                .FirstOrDefault(e=>e.Id == id);

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
