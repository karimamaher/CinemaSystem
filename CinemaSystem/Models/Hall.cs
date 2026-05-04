using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Models
{
    public class Hall
    {
        public int HallId { get; set; }

        [Required]
        [MaxLength(100)]
        public string HallName { get; set; }

        [MaxLength(50)]
        public string HallType { get; set; } // Standard / VIP / IMAX

        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }

        public int TotalSeats => Rows * SeatsPerRow;

        public ICollection<ShowTime>? ShowTimes { get; set; }
    }
}
