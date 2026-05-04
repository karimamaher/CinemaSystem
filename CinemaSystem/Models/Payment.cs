namespace CinemaSystem.Models
{
    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed,
        Refunded
    }

    public enum PaymentMethod
    {
        Visa,
        Cash,
        VodafoneCash
    }
    public class Payment
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Visa;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string SessionId { get; set; } = string.Empty;
        public string ?TransactionId { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; }

    }
}
