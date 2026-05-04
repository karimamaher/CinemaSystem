using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string QRCode { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.Now;

        public bool IsScanned { get; set; } = false;

        public int BookingSeatId { get; set; }
        public BookingSeat BookingSeat { get; set; }
    }
}
