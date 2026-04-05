using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController()
        {
            _context = new ApplicationDbContext();
        }
        public IActionResult Index(int page = 1, string? query = null)
        {

            var categories = _context.Categories.AsQueryable();
            //filter
            if (query is not null)
            {
                categories = categories.Where(e => e.Name.ToLower().Contains(query.Trim().ToLower()));
                ViewBag.Query = query;
            }

            //pagination
            double totalPages = Math.Ceiling(categories.Count() / 3.0);
            categories = categories.Skip((page - 1) * 3).Take(3);


            return View(new CategoriesVM()
            {
                Categories = categories.AsEnumerable(),
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
        public IActionResult Create(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var category = _context.Categories.Find(id);
            if (category is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            return View(category);
        }

        [HttpPost]
        public IActionResult Update(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }
    }
}
