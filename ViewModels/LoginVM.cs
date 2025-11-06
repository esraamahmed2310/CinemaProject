using System.ComponentModel.DataAnnotations;

namespace CinemaProject.ViewModels
{
    public class LoginVM
    {
        public int Id { get; set; }

        [Required, Display(Name = "User Name OR Email")]
        public string UserNameOREmail { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
