using CinemaSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE} , {SD.EMPLOYEE_ROLE}")]
    public class CinemaController : Controller
    {
        private readonly IRepository<Cinema> _repository;
        public CinemaController(IRepository<Cinema> repository)
        {
            _repository = repository;
        }
        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {

            //var Cinemas = _context.Cinemas.AsQueryable();
            var Cinemas =await _repository.GetAsync(cancellationToken: cancellationToken);
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
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public IActionResult Create()
        {
            return View(new Cinema());
        }


        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Cinema Cinema , IFormFile Img, CancellationToken cancellationToken = default)
        {
            ModelState.Remove("Img");
            if (!ModelState.IsValid)
                return View(Cinema);

            if (Img is not null && Img.Length > 0)
            {
                var fileName = CreateFile(Img);
                Cinema.Logo = fileName;
            }



           await _repository.CreateAsync(Cinema, cancellationToken: cancellationToken);
              await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Add Cinema Successfully";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            // var Cinemas = _context.Cinemas.Find(id);
            var Cinemas = await _repository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (Cinemas is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);


            return View(Cinemas);
        }


        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(Cinema Cinema , IFormFile Img, CancellationToken cancellationToken = default)
        {

            // var CinemaInDB = _context.Cinemas.AsNoTracking().SingleOrDefault(e => e.Id == Cinema.Id);
            var CinemaInDB = await _repository.GetOneAsync(e => e.Id == Cinema.Id, tracked: false, cancellationToken: cancellationToken);
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

            ModelState.Remove("Img");
            if (!ModelState.IsValid)
                return View(Cinema);

            _repository.Update(Cinema);
              await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Update Cinema Successfully";

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            //var Cinema= _context.Cinemas.Find(id);
            var Cinema = await _repository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken); 
            if (Cinema is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            //2.Delete old img from wwwroot
            var oldFilePath = GetOldFilePath(Cinema.Logo);
            if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

           _repository.Delete(Cinema);
              await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Delete Cinema Successfully";

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
