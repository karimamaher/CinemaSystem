using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaSystem.Models
{
    public class ShowTime  //العرض
    {
        public int ShowTimeId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now; 

        public DateTime EndTime { get; set; }= DateTime.Now.AddHours(2);

        public double Price { get; set; }

        
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }

        public int HallId { get; set; }
        public Hall? Hall { get; set; }
    }
}
