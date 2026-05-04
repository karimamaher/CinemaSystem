using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CinemaSystem.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class SeatsController : Controller
    {
        private readonly IRepository<Seat> _seatRepository;
        private readonly IRepository<ShowTime> _showTimeRepository;
        private readonly IRepository<BookingSeat> _bookingSeatRepository;

        public SeatsController(IRepository<Seat> seatRepository,
            IRepository<ShowTime> showTimeRepository,
            IRepository<BookingSeat> bookingSeatRepository)
        {
            _seatRepository = seatRepository;
            _showTimeRepository = showTimeRepository;
            _bookingSeatRepository = bookingSeatRepository;
        }


        //دى لعرض الكراسى وتبين المحجوز وانه يختار الكرسى
        public async Task<IActionResult> SelectSeats(int showTimeId)
        {
            
            var showTime = await _showTimeRepository.GetOneAsync(e => e.ShowTimeId == showTimeId , 
                includes:[e => e.Hall , e => e.Movie]);

            if (showTime is null) return NotFound();

            //كل الكراسى اللى فى القاعه
            var seats = await _seatRepository.GetAsync(e => e.HallId == showTime.HallId);

            //الكراسى المحجوزه فى قاعة العرض
            var bookedSeats = await _bookingSeatRepository.GetAsync(
               bs => bs.Booking.ShowTimeId == showTimeId
               && bs.Booking.Status == BookingStatus.Confirmed,
               includes: [bs => bs.Booking]);

            //الكراسى المحجوزه بدل ماهما اوبجكت كامل هاخد منهم ال هي بس 
            var bookedSeatIds = bookedSeats.Select(b => b.SeatId).ToList();


            return View(new SelectSeatsVM()
            {
                ShowTimeId = showTimeId,
                MovieName = showTime.Movie.Name,
                HallName = showTime.Hall.HallName,
                Rows = showTime.Hall.Rows,
                SeatsPerRow = showTime.Hall.SeatsPerRow,
                Price = showTime.Price,
                Seats = seats,
                BookedSeatIds = bookedSeatIds,
            });
        }
    }
}
