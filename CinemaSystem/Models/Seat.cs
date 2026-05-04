using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Models
{
    public enum SeatType {
        Standard,
        VIP
    }
    public class Seat  
    {
        public int SeatId { get; set; }    

        public char RowLabel { get; set; }    // A, B, C...

        public int SeatNumber { get; set; }   // 1, 2, 3...

       
        public SeatType SeatType { get; set; } = SeatType.Standard;

        public string Label => $"{RowLabel}{SeatNumber}"; // "A1", "B5"

        public int HallId { get; set; }
        public Hall Hall { get; set; }
        public ICollection<BookingSeat>? BookingSeats { get; set; }
    }
}
