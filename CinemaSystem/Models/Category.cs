using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required()]
        [Length(3, 10)]
        public string Name { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
}
