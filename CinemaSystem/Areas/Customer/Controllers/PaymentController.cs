using CinemaSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Threading.Tasks;

namespace CinemaSystem.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<BookingSeat> _bookingSeatRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ILogger<PaymentController> _logger;
        private readonly ApplicationDbContext _context;

        public PaymentController(IRepository<Booking> bookingRepository
            , IRepository<Payment> paymentRepository,
            IRepository<BookingSeat> bookingSeatRepository,
            ILogger<PaymentController> logger,
            ApplicationDbContext context,
            IRepository<Ticket> ticketRepository,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager)
        {
            _bookingRepository = bookingRepository;

            _paymentRepository = paymentRepository;
            _bookingSeatRepository = bookingSeatRepository;
            _logger = logger;
            _context = context;
            _ticketRepository = ticketRepository;
            _emailSender = emailSender;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int bookingId)
        {
            // جيب الـ Booking مع كل العلاقات
            var booking = await _bookingRepository.GetOneAsync(
                 b => b.BookingId == bookingId,
                includes: [
                    b => b.ShowTime,
                    b => b.ShowTime.Movie,
                    b => b.ShowTime.Hall,
                    b => b.BookingSeats,
                ]);

            if (booking is null) return NotFound();

            var userBookingSeats = await _bookingSeatRepository.GetAsync(
                       bs => bs.BookingId == bookingId,
                        includes: [bs => bs.Seat]);

            var seatLabels = userBookingSeats.Select(bs => bs.Seat.Label).ToList();


            return View(new PaymentVM
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingReference,
                MovieName = booking.ShowTime.Movie.Name,
                HallName = booking.ShowTime.Hall.HallName,
                ShowDateTime = booking.ShowTime.StartTime,
                TotalPrice = booking.TotalPrice,

                SelectedSeats = seatLabels,
            });
        }

        public async Task<IActionResult> Pay(int bookingId) 
        {

            var booking = await _bookingRepository.GetOneAsync(
                b => b.BookingId == bookingId,
                includes: [
                    b => b.ShowTime,
                    b => b.ShowTime.Movie,
                    b => b.BookingSeats,
                ]);

            if (booking is null) return NotFound();

            var userBookingSeats = await _bookingSeatRepository.GetAsync(
            bs => bs.BookingId == bookingId,
             includes: [bs => bs.Seat]);

            var seatLabels = string.Join(", ", userBookingSeats.Select(bs => bs.Seat.Label));

            var payment = new Payment()
            {
                BookingId= bookingId,
                Price = booking.TotalPrice
            };
            await _paymentRepository.CreateAsync(payment);
            await _paymentRepository.CommitAsync();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Payment/Success?paymentId={payment.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Payment/Cancel?paymentId={payment.Id}"

            };

            Console.WriteLine("Seats Count: " + userBookingSeats.Count());
            Console.WriteLine("Price: " + booking.ShowTime.Price);
            Console.WriteLine("Total: " + booking.TotalPrice);

            options.LineItems.Add(
               new SessionLineItemOptions
                {
                   PriceData = new SessionLineItemPriceDataOptions
                   {
                     Currency = "usd",
                     ProductData = new SessionLineItemPriceDataProductDataOptions
                     {
                         Name = $"{booking.ShowTime.Movie.Name} — Cinema Ticket",

                         Description = $"Ref: {booking.BookingReference} · " +
                              $"Seats: {seatLabels}",
                     },
                       UnitAmount = (long)booking.ShowTime.Price * 100 ,
                   },
                    Quantity = userBookingSeats.Count(),
              });


            var service = new SessionService();
            var session = service.Create(options);
            payment.SessionId = session.Id;
            await _paymentRepository.CommitAsync();

            return Redirect(session.Url);
        }

      
        [HttpGet]
        public async Task<IActionResult> Success(int paymentId)
        {

            var transaction = _context.Database.BeginTransaction();

            try
            {
                //  1. جيب الـ Payment 
                var payment = await _paymentRepository.GetOneAsync(
                e => e.Id == paymentId,
                includes: [e => e.Booking]);

                if (payment is null) return NotFound();

                // ── 2. تحقق من Stripe إن الدفع اتأكد فعلاً ──
                var service = new SessionService();
                var session = service.Get(payment.SessionId);

                    // ── 3. حدّث الـ Payment ──
                    payment.Status = PaymentStatus.Success;
                    payment.TransactionId = session.PaymentIntentId;
                    _paymentRepository.Update(payment);

                    // ── 4. حدّث الـ Booking ──
                    var booking = await _bookingRepository.GetOneAsync(
                        b => b.BookingId == payment.BookingId
                        , includes: [  b => b.ShowTime,
                                       b => b.ShowTime.Movie,
                                      b => b.ShowTime.Hall,]);

                    if (booking is null) return NotFound();

                    booking.Status = BookingStatus.Confirmed;
                        _bookingRepository.Update(booking);

                //5. اجيب كل الكراسى اللى مستخدم حجزها 
                var userBookingSeats = await _bookingSeatRepository.GetAsync(
                   bs => bs.BookingId == payment.BookingId,
                    includes: [bs => bs.Seat , bs => bs.Ticket]);

                //6. انشىء ticket لكل كرسى المستخدم حجزه وابعتهم على الايميل 
                foreach (var item in userBookingSeats)
                {
                    await _ticketRepository.CreateAsync(new Ticket
                    {
                        BookingSeatId = item.Id,
                        QRCode = Guid.NewGuid().ToString(),
                    });
                }

                await _paymentRepository.CommitAsync();

                //7.  لل user ابعت الايميل فيه التذاكر 

                var user = await _userManager.GetUserAsync(User);
                if (user is null) return NotFound();

                var ticketLines = userBookingSeats.Select(bs =>
                 $"  - Seat: {bs.Seat.Label}  |   TicketId: {bs.Ticket!.Id}|   QRCode: {bs.Ticket!.QRCode}  ");

                var body = $"Hi {user.UserName},\n\n" +
                           $"Your booking is confirmed!\n\n" +
                           $"Movie : {booking.ShowTime.Movie.Name}\n" +
                           $"Hall  : {booking.ShowTime.Hall.HallName}\n" +
                           $"Date  : {booking.ShowTime.StartTime:ddd dd MMM yyyy · hh:mm tt}\n" +
                           $"Total : {booking.TotalPrice} EGP\n\n" +
                           $"Your Tickets:\n" +
                           string.Join("\n", ticketLines) +
                           $"\n\nEnjoy the movie! 🍿";

                await _emailSender.SendEmailAsync(
                    user.Email,
                    $"Your Tickets — {booking.ShowTime.Movie.Name}",
                    body);
   

                transaction.Commit();

                TempData["success-notification"] = "Payment successful! Check your email for your tickets 🎬";
                return View(new PaymentSuccessVM
                {
                    BookingId = booking.BookingId,
                    BookingReference = booking.BookingReference,

                    MovieName = booking.ShowTime.Movie.Name,
                    HallName = booking.ShowTime.Hall.HallName,
                    StartTime = booking.ShowTime.StartTime,

                    Seats = userBookingSeats.Select(s => s.Seat.Label).ToList(),

                    TotalPrice = booking.TotalPrice,

                    PaymentId = payment.Id,
                    PaymentDate = payment.PaymentDate,
                    PaymentStatus = payment.Status.ToString(),

                    UserEmail = user.Email
                });          

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                transaction.Rollback();

                return BadRequest();
            }
        }



        [HttpGet]
        public async Task<IActionResult> Cancel(int paymentId)
        {
            // ── 1. جيب الـ Payment ──
            var payment = await _paymentRepository.GetOneAsync(
                p => p.Id == paymentId);

            if (payment is not null)
            {
                // ── 2. حدّث الـ Payment Status ──
                payment.Status = PaymentStatus.Failed;
                _paymentRepository.Update(payment);

                // ── 3. حدّث الـ Booking Status ──
                var booking = await _bookingRepository.GetOneAsync(
                    b => b.BookingId == payment.BookingId);

                if (booking is not null)
                {
                    booking.Status = BookingStatus.Cancelled;
                    _bookingRepository.Update(booking);
                }

                await _paymentRepository.CommitAsync();
            }

            TempData["error-notification"] = "Payment Failed .";
            return RedirectToAction("Index", "Payment", new { area = SD.CUSTOMER_AREA, bookingId = payment.BookingId });
        }




      
        public async Task<IActionResult> Refund(int paymentId)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var payment = await _paymentRepository.GetOneAsync(
                    p => p.Id == paymentId,
                    includes: [p => p.Booking]);

                if (payment is null) return NotFound();

                
                if (payment.Status != PaymentStatus.Success)
                    return BadRequest("Payment is not eligible for refund.");

                // ── تحقق إن مش عدش 24 ساعة ──
                if (DateTime.Now > payment.PaymentDate.AddHours(24))
                {
                    TempData["error-notification"] = "Refund period has expired (24 hours).";
                    return RedirectToAction("Details", "Booking",
                        new { area = SD.CUSTOMER_AREA,  id =  payment.BookingId });
                }


                var options = new RefundCreateOptions()
                {
                    Reason = RefundReasons.Unknown,
                    Amount = ((long)payment.Booking.TotalPrice * 100) - (5 * 100),
                    PaymentIntent = payment.TransactionId
                };

                var service = new RefundService();
                var session = service.Create(options);

                // ── حدّث Payment ──
                payment.Status = PaymentStatus.Refunded;
                _paymentRepository.Update(payment);              

                // ── حدّث Booking ──
                var booking = await _bookingRepository.GetOneAsync(
                    b => b.BookingId == payment.BookingId);

                if (booking is not null)
                {
                    booking.Status = BookingStatus.Cancelled;
                    _bookingRepository.Update(booking);
                }

                await _paymentRepository.CommitAsync();
                transaction.Commit();

                // ── ابعت Email للمستخدم ──
                var user = await _userManager.GetUserAsync(User);
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Refund Confirmed",
                    $"Hi {user.UserName},\n\nYour refund has been processed successfully.\nAmount will be returned within 5-10 business days.\n\nSorry to see you go! 🎬");

                TempData["success-notification"] = "Refund processed successfully!";
                return RedirectToAction("Index", "Movies", new { area = SD.CUSTOMER_AREA });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                transaction.Rollback();
                return BadRequest();
            }
        }
    }
}
