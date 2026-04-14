namespace CinemaSystem.Services.IServices
{
    public interface IMovieService
    {
        
       Task<string> CreateFileAsync(IFormFile Img, ImageType imageType = ImageType.MainImg);
        string GetOldFilePath(string oldFileName, ImageType imageType = ImageType.MainImg);
    }
}
