using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE} , {SD.EMPLOYEE_ROLE}")]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _repository;
        public CategoryController(IRepository<Category> repository)
        {
            _repository = repository;
        }
        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {

            // var categories = _context.Categories.AsQueryable();
            var categories =await _repository.GetAsync(cancellationToken: cancellationToken);
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
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public IActionResult Create()
        {
            return View(new Category());
        }


        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Category category, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(category);

           // _context.Categories.Add(category);
           // _context.SaveChanges();
           await _repository.CreateAsync(category , cancellationToken: cancellationToken);
           await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Add Category Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            //var category = _context.Categories.Find(id);
            var category =await _repository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (category is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(Category category, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(category);

           _repository.Update(category);
            await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Update Category Successfully";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            //var category = _context.Categories.Find(id);
            var category = await _repository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (category is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

           _repository.Delete(category);
            await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Delete Category Successfully";

            return RedirectToAction(nameof(Index));

        }
    }
}
