namespace Szkola2.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Role { get; set; }
    }
}
