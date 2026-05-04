using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CinemaSystem.Utility.DbInitializers
{
    public class DbInitializer : IDbInitializer
    {

        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<Hall> _hallRepository;
        private readonly IRepository<Seat> _seatRepository;
        private readonly UserManager<ApplicationUser> _userManger;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManger,
            ApplicationDbContext context,
            ILogger<DbInitializer> logger,
            IRepository<Seat> seatRepository,
            IRepository<Hall> hallRepository)
        {
            _roleManager = roleManager;
            _userManger = userManger;
            _context = context;
            _logger = logger;
            _seatRepository = seatRepository;
            _hallRepository = hallRepository;
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


                //هخزن القاعات
                var halls = await _hallRepository.GetAsync();

                if (!halls.Any())
                {
                    var hall1 = new Hall
                    {
                        HallName = "Hall 1",
                        HallType = "Standard",
                        Rows = 5,
                        SeatsPerRow = 10
                    };

                    var hall2 = new Hall
                    {
                        HallName = "VIP Hall",
                        HallType = "VIP",
                        Rows = 4,
                        SeatsPerRow = 8
                    };

                    await _hallRepository.CreateAsync(hall1);
                    await _hallRepository.CreateAsync(hall2);
                    await _hallRepository.CommitAsync();
                }

                //هخزن الكراسى
                var seats = await _seatRepository.GetAsync();

                if (!seats.Any())
                {
                    var hallsFromDb = await _hallRepository.GetAsync();

                    foreach (var hall in hallsFromDb)
                    {
                        for (char row = 'A'; row < 'A' + hall.Rows; row++)
                        {
                            for (int num = 1; num <= hall.SeatsPerRow; num++)
                            {
                                var seatType = (row == 'A' || row == 'B')
                                    ? SeatType.VIP
                                    : SeatType.Standard;

                                await _seatRepository.CreateAsync(new Seat
                                {
                                    RowLabel = row,
                                    SeatNumber = num,
                                    HallId = hall.HallId,
                                    SeatType = seatType
                                });
                            }
                        }
                    }

                    await _seatRepository.CommitAsync();
                }




            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

        }
    }
}
