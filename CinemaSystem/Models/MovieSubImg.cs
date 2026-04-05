using Microsoft.Build.Tasks.Deployment.Bootstrapper;

namespace CinemaSystem.Models
{
    public class MovieSubImg
    {
        public int Id { get; set; }
        public string SubImg { get; set; } = string.Empty;
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }
}
