namespace Blog.WebApi.Contracts;

public class UserResponse
{
    public long Id { get; set; }
    public Guid Key { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
