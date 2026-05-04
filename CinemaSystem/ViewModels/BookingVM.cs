namespace CinemaSystem.ViewModels
{
    public class BookingVM
    {
        public int BookingId { get; set; }
        public int  PaymentId { get; set; }
        public string MovieName { get; set; }
        public DateTime StartTime { get; set; }
        public List<string> Seats { get; set; }
        public double TotalPrice { get; set; }
        public string BookingStatus { get; set; }

        public DateTime PaidAt { get; set; }  //وقت الدفع

      //يقدر يعمل refund ولالا 
        public bool CanRefund =>
            BookingStatus == "Confirmed" &&
            DateTime.Now <= PaidAt.AddHours(24);
    }
}
