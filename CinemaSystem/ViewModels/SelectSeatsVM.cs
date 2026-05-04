namespace CinemaSystem.ViewModels
{
    public class SelectSeatsVM
    {
        public int ShowTimeId { get; set; }
        public string MovieName { get; set; }
        public string HallName { get; set; }

        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public double Price { get; set; }

        public IEnumerable<Seat> Seats { get; set; }
        public List<int> BookedSeatIds { get; set; }
    }
}
