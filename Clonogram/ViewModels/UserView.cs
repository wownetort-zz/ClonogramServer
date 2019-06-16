namespace Clonogram.ViewModels
{
    public class UserView
    {
        public string Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Description { get; set; } = "";
        public string AvatarPath { get; set; } = "";
        public string Password { get; set; }
    }
}
