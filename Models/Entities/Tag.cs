using Postgrest.Attributes;
using Postgrest.Models;


namespace Blog.WebApi.Models.Entities;
[Table("tags")]
public class Tag : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("key")]
    public Guid Key { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("name")]
    public string Name { get; set; }

    public Tag()
    {

    }

    public Tag(string name)
    {
        Name = name;
        Key = Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }
}

