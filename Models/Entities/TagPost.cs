using Postgrest.Attributes;
using Postgrest.Models;


namespace Blog.WebApi.Models.Entities;
[Table("tagspost")]
public class TagPost : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("key")]
    public Guid Key { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("post_id")]
    public long PostId { get; set; }
    [Column("tag_id")]
    public long TagId { get; set; }

    public TagPost()
    {

    }

    public TagPost(long tagId, long postId)
    {
        TagId = tagId;
        PostId = postId;
        Key = Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }
}
