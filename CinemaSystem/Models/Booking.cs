using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaSystem.Models
{
    public enum BookingStatus
    {
        Pending,    //  معلق لسه مادفعش
        Confirmed,  // دفع وتأكد
        Cancelled   // اتلغى
    }

    public class Booking
    {
        public int BookingId { get; set; }    

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public double TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;


        public string BookingReference { get; set; } = $"BK-{DateTime.Now.Year}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        // BK-2024-XXXX


        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; }

        public int ShowTimeId { get; set; }
        public ShowTime ShowTime { get; set; }

        public Payment? Payment { get; set; }
        public ICollection<BookingSeat>? BookingSeats { get; set; }
    }
}
