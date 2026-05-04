using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace CinemaSystem.Models
{
    public enum SeatStatus
    {
        Reserved,   // محجوز مؤقتاً
        Booked,     // مدفوع فعلاً
        Cancelled   // اتلغى
    }
    public class BookingSeat
    {
        public int Id { get; set; }

        public double  PriceAtBooking { get; set; }
        public SeatStatus SeatStatus { get; set; } = SeatStatus.Reserved;

        public DateTime? ReservedUntil { get; set; } //  وبعدها الحجز يتلغىTimer الـ 10 دقايق

       
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public int SeatId { get; set; }
        public Seat Seat { get; set; }
        public Ticket? Ticket { get; set; }
       
    }
}
