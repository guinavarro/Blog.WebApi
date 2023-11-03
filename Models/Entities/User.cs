
using Postgrest.Attributes;
using Postgrest.Models;

namespace Blog.WebApi.Models.Entities;

[Table("users")]
public class User : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("key")]
    public Guid Key { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("username")]
    public string Username { get; set; }
    [Column("email")]
    public string Email { get; set; }
    [Column("passwordHash")]
    public string PasswordHash { get; set; }
}
