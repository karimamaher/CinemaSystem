using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Threading.Tasks;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var users =  _userManager.Users.AsQueryable();
            //filter
            if (query is not null)
            {
                users = users.Where(e => e.NormalizedUserName!.Contains(query.Trim().ToUpper()));
                ViewBag.Query = query;
            }

            //pagination
            double totalPages = Math.Ceiling(users.Count() / 3.0);
           var usersList = users.Skip((page - 1) * 3).Take(3).ToList();

            //Mapping
            Dictionary<ApplicationUser , string> userRoles = new Dictionary<ApplicationUser , string>();

            foreach (var item in usersList)
            {
                userRoles.Add(item, (await _userManager.GetRolesAsync(item)).FirstOrDefault()!);
            }


            return View(new ApplicationUserWithFilterVM()
            {
                UserRoles = userRoles.ToDictionary(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }


        [HttpGet]
        public IActionResult InternalRegister()
        {
            return View(new InternalRegisterVM()
            {
                Roles = _roleManager.Roles.Where(r => r.Name != "SuperAdmin").ToList()
            });
        }


        [HttpPost]
        public async Task<IActionResult> InternalRegister(InternalRegisterVM internalRegisterVM)
        {
            if (!ModelState.IsValid) 
            {
                internalRegisterVM.Roles = _roleManager.Roles.Where(r => r.Name != "SuperAdmin").ToList();
                return View(internalRegisterVM);
            }


            var user = new ApplicationUser()
            {
                FirstName = internalRegisterVM.FirstName,
                LastName = internalRegisterVM.LastName,
                Email = internalRegisterVM.Email,
                UserName = internalRegisterVM.UserName,
                Address = internalRegisterVM.Address
            };


            var result = await _userManager.CreateAsync(user, internalRegisterVM.Password);

            if (!result.Succeeded)
            {
                TempData["error-notification"] = string.Join(",",
                    result.Errors.Select(e => e.Description));

                internalRegisterVM.Roles = _roleManager.Roles.Where(r => r.Name != "SuperAdmin").ToList();
                return View(internalRegisterVM);
            }
            // الادمن بيفعله هو الايميل 
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            await _userManager.AddToRoleAsync(user, internalRegisterVM.RoleName);

            TempData["success-notification"] = "Create Account Successfully";
            return RedirectToAction(nameof(Index));           
        }




        [HttpGet]
        public async Task<IActionResult> UpdateRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, SD.SUPER_ADMIN_ROLE)) return NotFound();

            var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault()!;

            return View(new UserWithRoleVM()
            {
                ApplicationUser = user,
                RoleName = userRole,
                  IdentityRoles = _roleManager.Roles.AsEnumerable()
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(UserWithRoleVM userWithRoleVM)
        {
            var user = await _userManager.FindByIdAsync(userWithRoleVM.Id);
            if (user is null) return NotFound();


            var roles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, roles);


            await _userManager.AddToRoleAsync(user, userWithRoleVM.RoleName);
            TempData["success-notification"] = $"Update Role To User: {user.UserName} Successfully";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, SD.SUPER_ADMIN_ROLE)) return NotFound();

            user.LockoutEnabled = !user.LockoutEnabled;
            if (!user.LockoutEnabled)  
            {
                user.LockoutEnd = DateTime.Now.AddDays(3);
                TempData["warning-notification"] = $"Lock User: {user.UserName} Successfully";
            }
            else
            {
                user.LockoutEnd = null;
                TempData["warning-notification"] = $"UnLock User: {user.UserName} Successfully";
            }
          
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
