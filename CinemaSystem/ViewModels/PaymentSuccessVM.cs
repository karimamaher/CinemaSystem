namespace CinemaSystem.ViewModels
{
    public class PaymentSuccessVM
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; }

        public string MovieName { get; set; }
        public string HallName { get; set; }
        public DateTime StartTime { get; set; }

        public List<string> Seats { get; set; } = new();

        public double TotalPrice { get; set; }

        public int PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }

        public string UserEmail { get; set; }
    }
}
