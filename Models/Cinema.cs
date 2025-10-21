namespace CinemaProject.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;             // اسم السينما
        public string? Location { get; set; }          // العنوان أو المدينة
        public string? Description { get; set; }       // تفاصيل عن السينما
        public bool IsActive { get; set; } = true;    // هل السينما تعمل حاليًا
        public string? Image { get; set; }
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
