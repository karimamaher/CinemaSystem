using CinemaSystem.Models;
using CinemaSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IRepository<Seat> _seatRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<BookingSeat> _bookingSeatRepository;
        private readonly IRepository<ShowTime> _showTimeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(IRepository<Seat> seatRepository, IRepository<Booking> bookingRepository,
            UserManager<ApplicationUser> userManager, IRepository<ShowTime> showTimeRepository,
            IRepository<BookingSeat> bookingSeatRepository, IRepository<Payment> paymentRepository)
        {
            _seatRepository = seatRepository;
            _bookingRepository = bookingRepository;
            _userManager = userManager;
            _showTimeRepository = showTimeRepository;
            _bookingSeatRepository = bookingSeatRepository;
            _paymentRepository = paymentRepository;
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmSeats(int showTimeId, List<int> selectedSeatIds)
        {
            if (selectedSeatIds == null || !selectedSeatIds.Any())
            {
                TempData["error-notification"] = "Please select at least one seat";
                return RedirectToAction("SelectSeats", "Seats", new { area = SD.CUSTOMER_AREA, showTimeId });
            }


            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            //عشان السعر 
            var showTime = await _showTimeRepository.GetOneAsync(e => e.ShowTimeId == showTimeId,
                includes: [e => e.Movie, e => e.Hall]);
            if (showTime is null) return NotFound();


            // ── 4. تحقق إن الكراسي لسه متاحة ──
            // بتسأل: في أي كرسي من اللي اختاره المستخدم محجوز من حد تاني؟
            var alreadyBooked = await _bookingSeatRepository.GetAsync(
                bs => bs.Booking.ShowTimeId == showTimeId
                           && bs.SeatStatus != SeatStatus.Cancelled // يعنى كدا يمحجوز مؤقت او اتحجز خلاص
                           && selectedSeatIds.Contains(bs.SeatId));

            if (alreadyBooked.Any())
            {
                TempData["error-notification"] = "Some seats were just taken. Please reselect.";
                return RedirectToAction("SelectSeats", "Seats", new { area = SD.CUSTOMER_AREA, showTimeId });
            }

            //هحسب السعر
            double totalPrice = showTime.Price * selectedSeatIds.Count();

            // أنشىء Booking 
            var booking = new Booking()
            {
                TotalPrice = totalPrice,
                ApplicationUserId = user.Id,
                ShowTimeId = showTimeId,
            };
           await _bookingRepository.CreateAsync(booking);
            await _bookingRepository.CommitAsync();

            // . أنشئ BookingSeat لكل كرسي اختاره المستخدم ──
            foreach (var seatId in selectedSeatIds)
            {
                await _bookingSeatRepository.CreateAsync(new BookingSeat()
                {
                    BookingId= booking.BookingId,
                    SeatId = seatId,
                    PriceAtBooking = showTime.Price,
                    SeatStatus = SeatStatus.Reserved,
                    ReservedUntil= DateTime.Now.AddMinutes(10),
                });
           
            }
            await _bookingSeatRepository.CommitAsync();

            return RedirectToAction("Index", "Payment", new { area = SD.CUSTOMER_AREA, bookingId = booking.BookingId  });

        }

        [HttpGet]
        public async Task<IActionResult> MyBooking(string? query = null, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();


            var userBooking = await _bookingRepository.GetAsync(e => e.ApplicationUserId == user.Id,
               includes: [ e => e.ShowTime,
                           e => e.ShowTime.Movie,
                           e => e.ShowTime.Hall,
                           e => e.BookingSeats,
                           e => e.BookingSeats,
                             e => e.Payment]

                , cancellationToken: cancellationToken);


            var bookingSeats = await _bookingSeatRepository.GetAsync(
             bs => userBooking.Select(b => b.BookingId).Contains(bs.BookingId),
               includes: [bs => bs.Seat]);

            //filter
            if (query is not null)
            {
                userBooking = userBooking.Where(e => e.BookingId == Convert.ToInt32(query));
                ViewBag.Query = query;
            }

            var UsrBookingsVM = new List<BookingVM>();

            foreach (var item in userBooking)
            {
                UsrBookingsVM.Add(new BookingVM
                {
                    BookingId = item.BookingId,
                    PaymentId= item.Payment.Id,
                    MovieName = item.ShowTime.Movie.Name,
                    StartTime = item.ShowTime.StartTime,


                    Seats = bookingSeats
                   .Where(s => s.BookingId == item.BookingId)
                  .Select(s => s.Seat.Label)
                  .ToList()  ,
     

                    TotalPrice = item.TotalPrice,
                    BookingStatus = item.Status.ToString() ,
                    
                    PaidAt= item.Payment.PaymentDate
                });
            }


                return View(UsrBookingsVM);

            }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingRepository.GetOneAsync(
                b => b.BookingId == id,
                includes: [
                    b => b.ShowTime,
                   b => b.ShowTime.Movie,
                   b => b.ShowTime.Hall,
                   b => b.BookingSeats,
                    b => b.Payment
                ]);

            if (booking is null) return NotFound();

           var  bookingSeats = await _bookingSeatRepository.GetAsync(
                  bs => bs.BookingId == id,
                  includes: [bs => bs.Seat]);

            return View( new BookingDetailsVM
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingReference,

                MovieName = booking.ShowTime.Movie.Name,
                HallName = booking.ShowTime.Hall.HallName,
                StartTime = booking.ShowTime.StartTime,

                Seats = bookingSeats
                .Select(s => s.Seat.Label)
                .ToList(),


                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.Status.ToString(),
                PaymentStatus = booking.Payment.Status.ToString(),
            });
        }
    }
}