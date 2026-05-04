namespace CinemaSystem.ViewModels
{
    public class ShowTimesVM
    {
        public IEnumerable<ShowTime> ShowTimes { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
