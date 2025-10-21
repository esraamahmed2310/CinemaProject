namespace CinemaProject.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }      
        public decimal Price { get; set; }
        public bool Status { get; set; } = true;
        public DateTime ShowTime { get; set; }        
        public string? MainImg { get; set; }           
        public string? SubImages { get; set; }         
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } = null!;

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}