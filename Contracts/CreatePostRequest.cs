
using System.Reflection;
using System.Security.Principal;

namespace Blog.WebApi.Contracts;

public class CreatePostRequest
{
    public string Title { get; set; }
    public string Content { get; set; }
    public List<string>? Tags { get; set; }
    public long? UserId { get; set; }
    public IFormFile? File { get; set; }

}
