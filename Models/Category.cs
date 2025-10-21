namespace CinemaProject.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;            // نوع الفيلم (أكشن، دراما، ...)
        public string? Description { get; set; }       // وصف بسيط للتصنيف
        public bool IsActive { get; set; } = true;    // تفعيل/إخفاء التصنيف

        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
