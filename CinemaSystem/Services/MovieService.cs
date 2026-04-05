namespace CinemaSystem.Services
{
    public enum ImageType
    {
        MainImg ,
        SubImg,
        ActorImg
    }
    public class MovieService
    {
        public string CreateFile(IFormFile Img , ImageType imageType = ImageType.MainImg )
        {
            var fileName = $"{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")}-{Guid.NewGuid().ToString()}{Path.GetExtension(Img.FileName)}";

            var filePath = string.Empty;
            if (imageType == ImageType.MainImg) 
            {
              filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movies", fileName);
            }else if(imageType == ImageType.SubImg)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movies\\SubImgs", fileName);

            }else if(imageType == ImageType.ActorImg)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\actors", fileName);

            }

                using (var stream = System.IO.File.Create(filePath))
                {
                    Img.CopyTo(stream);
                }
            return fileName;

        }
        public string GetOldFilePath(string oldFileName, ImageType imageType = ImageType.MainImg)
        {
            var filePath = string.Empty;
            if (imageType == ImageType.MainImg)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movies", oldFileName);
            }
            else if (imageType == ImageType.SubImg)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movies\\SubImgs", oldFileName);

            }
            else if (imageType == ImageType.ActorImg)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\actors", oldFileName);

            }
            return filePath;

        }
    }
}
