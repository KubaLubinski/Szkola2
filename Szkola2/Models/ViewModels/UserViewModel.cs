using System.ComponentModel.DataAnnotations;

namespace Szkola2.Models.ViewModels
{
    public class UserViewModel:AppUser
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Proszę wpisać Email użytkownika")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Proszę wpisać imię użytkownika")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Proszę wpisać nazwisko użytkownika")]
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Role { get; set; }
    }
}
