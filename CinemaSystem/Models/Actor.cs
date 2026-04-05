using Microsoft.Build.Tasks.Deployment.Bootstrapper;

namespace CinemaSystem.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Img { get; set; } = string.Empty;
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }
}
