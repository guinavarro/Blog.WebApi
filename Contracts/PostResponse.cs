
using Blog.WebApi.Models;

namespace Blog.WebApi.Contracts;

public class PostResponse
{
    public PostResponse()
    {

    }
    public PostResponse(Guid key, string title, string content, bool active, DateTime createdAt)
    {
        Key = key;
        Title = title;
        Content = content;
        Active = active;
        CreatedAt = createdAt;
    }

    public Guid Key { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<TagJson> Tags { get; private set; }
    public string? FileUrl { get; private set; }

    public void SetTagsPost(List<TagJson> tags) => Tags = tags;
    public void SetFileUrl(string fileUrl) => FileUrl = fileUrl;
}
