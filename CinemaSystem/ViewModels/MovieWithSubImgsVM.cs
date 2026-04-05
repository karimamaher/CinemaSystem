using Microsoft.Build.Tasks.Deployment.Bootstrapper;

namespace CinemaSystem.ViewModels
{
    public class MovieWithSubImgsVM
    {
        public Movie Movie { get; set; } = null!;
        public IEnumerable<MovieSubImg> MovieSubImgs { get; set; } = [];

        public IEnumerable<Category> Categories { get; set; } = [];
        public IEnumerable<Cinema> Cinemas { get; set; } = [];
    }
}
