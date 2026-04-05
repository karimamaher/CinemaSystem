
namespace CinemaSystem.ViewModels
{
    public class CinemasVM
    {
        public IEnumerable<Cinema> Cinemas { get; set; } = null!;
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
