using Microsoft.EntityFrameworkCore;

namespace CinemaProject.Models
{
    [PrimaryKey(nameof(MovieId), nameof(SubImg))]
    public class MovieSubImage
    {
        
        public int MovieId { get; set; }
        public string SubImg { get; set; } = null!;
        public Movie Movie { get; set; } 

    }
}
