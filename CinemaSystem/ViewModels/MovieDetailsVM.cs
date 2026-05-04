namespace CinemaSystem.ViewModels
{
    public class MovieDetailsVM
    {
        public Movie Movie { get; set; } = null!;   

        public IEnumerable<ShowTime> ShowTimes { get; set; } = null!;
    }
}
