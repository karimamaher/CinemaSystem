using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CinemaSystem.Utility.DbInitializers
{
    public class DbInitializer : IDbInitializer
    {

        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManger;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManger,
            ApplicationDbContext context,
            ILogger<DbInitializer> logger)
        {
            _roleManager = roleManager;
            _userManger = userManger;
            _context = context;
            _logger = logger;
        }
        public async Task Initialize()
        {
            try
            {

                if (_context.Database.GetPendingMigrations().Any())
                    _context.Database.Migrate();


                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.SUPER_ADMIN_ROLE));
                    await _roleManager.CreateAsync(new IdentityRole(SD.ADMIN_ROLE));
                    await _roleManager.CreateAsync(new IdentityRole(SD.CUSTOMER_ROLE));
                    await _roleManager.CreateAsync(new IdentityRole(SD.EMPLOYEE_ROLE));

                    await _userManger.CreateAsync(new ApplicationUser()
                    {
                        Email = "SuperAdmin@Cinema.com",
                        EmailConfirmed = true,
                        FirstName = "Super",
                        LastName = "Admin",
                        UserName = "SuperAdmin"
                    }, password: "SuperAdmin123#");

                    var user = await _userManger.FindByEmailAsync("SuperAdmin@Cinema.com");

                    await _userManger.AddToRoleAsync(user!, SD.SUPER_ADMIN_ROLE);

                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

        }
    }
}
