namespace CinemaSystem.ViewModels
{
    public class PaymentVM
    {
    
         public int BookingId { get; set; }
        public string BookingReference { get; set; }
        public string MovieName { get; set; }
        public string HallName { get; set; }
        public DateTime ShowDateTime { get; set; }
        public double TotalPrice { get; set; }
        public List<string> SelectedSeats { get; set; } = new(); // ["A1","A2","B3"]
    }
}