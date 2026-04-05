using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class CinemaController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CinemaController()
        {
            _context = new ApplicationDbContext();
        }
        public IActionResult Index(int page = 1, string? query = null)
        {

            var Cinemas = _context.Cinemas.AsQueryable();
            //filter
            if (query is not null)
            {
                Cinemas = Cinemas.Where(e => e.Name.ToLower().Contains(query.Trim().ToLower()));
                ViewBag.Query = query;
            }

            //pagination
            double totalPages = Math.Ceiling(Cinemas.Count() / 3.0);
            Cinemas = Cinemas.Skip((page - 1) * 3).Take(3);


            return View(new CinemasVM()
            {
                Cinemas = Cinemas.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Cinema Cinema , IFormFile Img)
        {
            if (Img is not null && Img.Length > 0)
            {
                var fileName = CreateFile(Img);
                Cinema.Logo = fileName;
            }
            _context.Cinemas.Add(Cinema);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var Cinemas = _context.Cinemas.Find(id);
            if (Cinemas is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);


            return View(Cinemas);
        }

        [HttpPost]
        public IActionResult Update(Cinema Cinema , IFormFile Img)
        {
            var CinemaInDB = _context.Cinemas.AsNoTracking().SingleOrDefault(e => e.Id == Cinema    .Id);
            if (CinemaInDB is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            if (Img is not null && Img.Length > 0)
            {
                // 1. Create new img in wwwroot
                var fileName = CreateFile(Img);

                //2.Delete old img from wwwroot
                var oldFilePath = GetOldFilePath(CinemaInDB.Logo);
                if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                //3 update img in DB
                Cinema.Logo = fileName;
            }
            else
                Cinema.Logo = CinemaInDB.Logo;

            _context.Cinemas.Update(Cinema);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var Cinema= _context.Cinemas.Find(id);
            if (Cinema is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            //2.Delete old img from wwwroot
            var oldFilePath = GetOldFilePath(Cinema.Logo);
            if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _context.Cinemas.Remove(Cinema);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        //service methods
        private string CreateFile(IFormFile Img)
        {
            var fileName = $"{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")}-{Guid.NewGuid().ToString()}{Path.GetExtension(Img.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\cinemas", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                Img.CopyTo(stream);
            }
            return fileName;

        }
        private string GetOldFilePath(string oldFileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\cinemas", oldFileName);
            return filePath;

        }
    }
}
