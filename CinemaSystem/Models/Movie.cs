using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Price { get; set; }
        public bool Status { get; set; }
        public DateTime DateTime  { get; set; }
        public string MainImg { get; set; } = string.Empty;

        public List<Actor> Actors { get; set; } = [];

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } = null!;
    }
}
