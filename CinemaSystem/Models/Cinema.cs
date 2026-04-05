namespace CinemaSystem.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
}
