namespace CinemaSystem.ViewModels
{
    public class BookingDetailsVM
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; }

        public string MovieName { get; set; }
        public string HallName { get; set; }
        public DateTime StartTime { get; set; }

        public List<string> Seats { get; set; }

        public double TotalPrice { get; set; }

        public string BookingStatus { get; set; }
        public string PaymentStatus { get; set; }
    }
}
