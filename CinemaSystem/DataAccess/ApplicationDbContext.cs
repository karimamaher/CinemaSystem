using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.DataAccess
{
    public class ApplicationDbContext :DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieSubImg> MovieSubImgs { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\ProjectModels;Initial Catalog=CinemaSystem;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;");
        }
    }
}
