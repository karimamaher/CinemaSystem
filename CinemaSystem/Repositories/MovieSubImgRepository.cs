using CinemaSystem.Repositories.IRepositories;

namespace CinemaSystem.Repositories
{
    public class MovieSubImgRepository : Repository<MovieSubImg> ,IMovieSubImgRepository
    {
        public MovieSubImgRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void DeleteRange(IEnumerable<MovieSubImg> movieSubImages)
        {
            foreach (var item in movieSubImages)
            {
                _context.MovieSubImgs.Remove(item);
            }
        }

    }
}
