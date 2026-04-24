namespace EventBooking.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string Role { get; set; } = UserRoles.User;
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
