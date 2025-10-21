namespace CinemaProject.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;          // اسم الممثل بالكامل
        public string? Description { get; set; }       
        public string? ProfileImage { get; set; }      // صورة شخصية
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }

}

