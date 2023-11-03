
using Postgrest.Attributes;
using Postgrest.Models;

namespace Blog.WebApi.Models.Entities;

[Table("posts")]
public class Post : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("key")]
    public Guid Key { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("title")]
    public string Title { get; set; }
    [Column("content")]
    public string Content { get; set; }
    [Column("active")]
    public bool Active { get; set; }
    [Column("user_id")]
    public long UserId { get; set; }

    public Post()
    {

    }

    public Post(string title, string content, long userId)
    {
        Title = title;
        Content = content;
        UserId = userId;
        Active = true;
    }

}
