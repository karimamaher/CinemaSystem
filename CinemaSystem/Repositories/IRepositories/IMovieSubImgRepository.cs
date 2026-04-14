namespace CinemaSystem.Repositories.IRepositories
{
    public interface IMovieSubImgRepository: IRepository<MovieSubImg>
    {
        void DeleteRange(IEnumerable<MovieSubImg> movieSubImages);
    }
}
