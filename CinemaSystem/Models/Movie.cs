using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required()]
        [Length(4, 30)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        [DataType(DataType.Currency)]
        [CustomPrice(10000,50000)]
        public double Price { get; set; }
        public bool Status { get; set; }

        [Required()]
        [DataType(DataType.DateTime)]
        public DateTime DateTime  { get; set; }

        
        public string MainImg { get; set; } = string.Empty;

        public List<Actor>? Actors { get; set; } = [];

        [Required()]
        public int CategoryId { get; set; }
        public Category ? Category { get; set; }

        [Required()]
        public int CinemaId { get; set; }
        public Cinema ? Cinema { get; set; }

        public ICollection<ShowTime>? ShowTimes { get; set; }
    }
}
