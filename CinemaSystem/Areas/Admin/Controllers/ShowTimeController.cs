using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE} , {SD.EMPLOYEE_ROLE}")]
    public class ShowTimeController : Controller
    {
        private readonly IRepository<ShowTime> _repository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Hall> _hallRepository;
        public ShowTimeController(IRepository<ShowTime> repository,
            IRepository<Hall> hallRepository,
            IRepository<Movie> movieRepository)
        {
            _repository = repository;
            _hallRepository = hallRepository;
            _movieRepository = movieRepository;
        }
        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {


            var showTimes = await _repository.GetAsync(includes: [e=> e.Movie , e=> e.Hall ], cancellationToken: cancellationToken);
            //filter
            if (query is not null)
            {
                showTimes = showTimes.Where(e => e.ShowTimeId == Convert.ToInt32(query));
                ViewBag.Query = query;
            }

            //pagination
            double totalPages = Math.Ceiling(showTimes.Count() / 3.0);
            showTimes = showTimes.Skip((page - 1) * 3).Take(3);


            return View(new ShowTimesVM()
            {
                ShowTimes = showTimes.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }


        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Movies = await _movieRepository.GetAsync();
            ViewBag.Halls = await _hallRepository.GetAsync();
            return View(new ShowTime());
        }


        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(ShowTime ShowTime, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) 
            {
                ViewBag.Movies = await _movieRepository.GetAsync();
                ViewBag.Halls = await _hallRepository.GetAsync();
                return View(ShowTime);
            }
               

            await _repository.CreateAsync(ShowTime, cancellationToken: cancellationToken);
            await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Add ShowTime Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            
            var ShowTime = await _repository.GetOneAsync(e => e.ShowTimeId == id, cancellationToken: cancellationToken);
            if (ShowTime is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            return View(ShowTime);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(ShowTime ShowTime, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(ShowTime);

            _repository.Update(ShowTime);
            await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Update ShowTime Successfully";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            //var ShowTime = _context.showTimes.Find(id);
            var ShowTime = await _repository.GetOneAsync(e => e.ShowTimeId == id, cancellationToken: cancellationToken);
            if (ShowTime is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);

            _repository.Delete(ShowTime);
            await _repository.CommitAsync(cancellationToken: cancellationToken);

            TempData["success-notification"] = "Delete ShowTime Successfully";

            return RedirectToAction(nameof(Index));

        }
    }
}
