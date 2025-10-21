namespace CinemaProject.ViewModels
{
    public class FilterVM
    {
        public string MovieName { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int CategoryId { get; set; }
        public bool IsHot { get; set; }
    }
}